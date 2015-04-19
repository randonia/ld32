using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private enum PlayerState
    {
        Idle,
        Playing,
        Dead
    }

    private enum Direction
    {
        Left,
        Right
    }

    /// <summary>
    /// Player's state, defaults to Idle
    /// </summary>
    private PlayerState mState = PlayerState.Playing;

    private Direction mFacingDirection = Direction.Right;

    public GameObject PREFAB_DEBRIS;
    private GameObject mPlayerWalkRenderer;
    private GameObject mPlayerIdleRenderer;
    private GameObject mHook;
    private HookController mHookController;
    private GameObject mShotRenderer;
    private GameObject mMainCameraGO;
    private Camera mMainCamera;

    #region Input member variables

    private Vector2 mInputAxes;
    private bool mInputJumpPressed;
    private bool mInputSprintPressed;
    private bool mInputHookShoot;

    private bool mInputShootDebris;
    private bool mInputLastTickShootDebris;

    #endregion Input member variables

    private Vector3 kBoundBox;

    private float mSpeed = 5.0f;
    private const float kSprintMultiplier = 1.5f;

    // Can only sprint if <=30%
    private const float kSprintWeightThreshold = 0.3f;

    private const float kWalkWeightHighThreshold = 0.8f;
    private const float kWalkWeightHighMultiplier = 0.3f;
    private const float kWalkWeightMedThreshold = 0.4f;
    private const float kWalkWeightMedMultiplier = 0.6f;
    private const float kWalkWeightLowThreshold = 0.25f;
    private const float kWalkWeightLowMultiplier = 1f;

    /// <summary>
    /// Set at the beginning of update
    /// </summary>
    private bool mIsGrounded;

    /// <summary>
    /// This gets reset each time the player hits the ground while they can't jump.
    /// </summary>
    private bool mCanJump;

    private bool mLongJump;

    /// <summary>
    /// Rules for using hook:
    /// <para>HookController not null</para>
    /// <para>HookController reporting available HookController::IsAvailable</para>
    /// </summary>
    private bool CanUseHook { get { return mHookController != null && mHookController.IsAvailable; } }

    /// <summary>
    /// Rules for shooting debris:
    /// <para>Hook not in use</para>
    /// <para>Aren't currently in shoot state</para>
    /// <para>Have Ammo</para>
    /// </summary>
    private bool CanShootDebris { get { return CanUseHook && !mInputLastTickShootDebris && mAmmoCount > 0; } }

    private float mShootStartTimer = -1;
    private const float kShootMaxTimer = 0.5f;
    private Vector3 mShootDir;
    private const float kMaxThrowForce = 10.0f;

    public bool IsCurrentlyShooting { get { return mShootStartTimer != -1; } }

    public float DebrisShotLength { get { return (IsCurrentlyShooting) ? (Time.time - mShootStartTimer) / kShootMaxTimer : -1; } }

    private const float kJumpSpeed = 7f;

    public float JumpSpeed { get { return kJumpSpeed * RunWeightMultiplier; } }

    public bool IsGrounded { get { return mIsGrounded; } }

    private int kTerrainMask;
    private float mJumpTimer = float.MinValue;
    private const float kJumpDuration = 0.25f;

    // FOR TESTING
    private int mAmmoCount = 1;

    public string AmmoCount { get { return mAmmoCount.ToString(); } }

    private const int kMaxAmmoCount = 10;

    private float AmmoWeightFloat { get { return (mAmmoCount / (float)kMaxAmmoCount); } }

    public string AmmoWeight { get { return string.Format("{0}%", (AmmoWeightFloat * 100f).ToString()); } }

    private float GetFacingRotation { get { return mFacingDirection.Equals(Direction.Left) ? 180f : 0; } }

    public Color AmmoWeightUIColor
    {
        get
        {
            float weight = AmmoWeightFloat;

            if (weight >= kWalkWeightHighThreshold)
            {
                return Color.red;
            }
            if (weight >= kWalkWeightMedThreshold)
            {
                return Color.yellow;
            }
            return Color.white;
        }
    }

    private bool IsSprinting { get { return mInputSprintPressed && AmmoWeightFloat <= kSprintWeightThreshold; } }

    private float RunWeightMultiplier { get { return (AmmoWeightFloat >= kWalkWeightHighThreshold) ? kWalkWeightHighMultiplier : (AmmoWeightFloat >= kWalkWeightMedThreshold) ? kWalkWeightMedMultiplier : kWalkWeightLowMultiplier; } }

    private Stack<GameObject> mDebris;

    public string DebugString
    {
        get
        {
            return string.Format("State: {0}",
                mState.ToString());
        }
    }

    // Use this for initialization
    void Start()
    {
        mDebris = new Stack<GameObject>();
        mInputAxes = Vector2.zero;
        kBoundBox = GetComponent<BoxCollider>().bounds.size;
        kTerrainMask = LayerMask.GetMask("Terrain");
        // Set up the hook
        mHook = GameObject.Find("Hook");
        mHookController = mHook.GetComponent<HookController>();
        mHook.SetActive(false);
        // Set up the camera
        mMainCameraGO = GameObject.Find("Main Camera");
        mMainCamera = mMainCameraGO.GetComponent<Camera>();
        // Get the shot renderer
        mShotRenderer = GameObject.Find("playershotrenderer");
        // Get the player's renderer
        foreach (Transform child in transform)
        {
            if (child.name.Equals("renderer_idle"))
            {
                mPlayerIdleRenderer = child.gameObject;
            }
            if (child.name.Equals("renderer_walk"))
            {
                mPlayerWalkRenderer = child.gameObject;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CustomGravity();
        GetGroundStatus();
        GetInput();
        switch (mState)
        {
            case PlayerState.Idle:
                TickIdle();
                // Don't do anything
                return;
            case PlayerState.Playing:
                TickMoving();
                break;
        }
        if (!mState.Equals(PlayerState.Dead))
        {
            // Test for hook shoot
            if (CanUseHook && mInputHookShoot)
            {
                ShootHookAtMouse();
            }
            if (CanShootDebris && mInputShootDebris)
            {
                StartShootDebris();
            }
            if (IsCurrentlyShooting && mInputLastTickShootDebris)
            {
                if (mInputShootDebris && Time.time < mShootStartTimer + kShootMaxTimer)
                {
                    TickShootDebris();
                }
                else if (!mInputShootDebris || Time.time > mShootStartTimer + kShootMaxTimer)
                {
                    FinishShootingDebris();
                }
            }
            // Render "logic"
            if (mInputAxes.sqrMagnitude != 0)
            {
                mPlayerIdleRenderer.SetActive(false);
                mPlayerWalkRenderer.SetActive(true);
                mPlayerWalkRenderer.transform.rotation = Quaternion.identity;
                mPlayerWalkRenderer.transform.Rotate(Vector3.up, GetFacingRotation);
            }
            else
            {
                mPlayerWalkRenderer.SetActive(false);
                mPlayerIdleRenderer.SetActive(true);
                mPlayerIdleRenderer.transform.rotation = Quaternion.identity;
                mPlayerIdleRenderer.transform.Rotate(Vector3.up, GetFacingRotation);
            }
        }
    }

    private void TickShootDebris()
    {
        mShootDir = (Input.mousePosition - mMainCamera.WorldToScreenPoint(transform.position)).normalized;
        mShotRenderer.transform.rotation = Quaternion.identity;
        mShotRenderer.transform.Rotate(transform.forward, Mathf.Atan2(mShootDir.y, mShootDir.x) * HookController.RAD2DEG - 90);
    }

    private void FinishShootingDebris()
    {
        float shootVal = DebrisShotLength;
        GameObject newDebris;
        // Error check to make sure something didn't go wrong and to support testing
        if (mDebris.Count == 0)
        {
            newDebris = (GameObject)GameObject.Instantiate(PREFAB_DEBRIS);
        }
        else
        {
            newDebris = mDebris.Pop();
            newDebris.SetActive(true);
        }
        newDebris.transform.position = mShotRenderer.transform.position;
        newDebris.GetComponent<Rigidbody>().velocity = mShootDir * kMaxThrowForce * shootVal;
        newDebris.GetComponent<DebrisController>().StartThrow();
        // Assumes only one collider
        Physics.IgnoreCollision(GetComponent<Collider>(), newDebris.GetComponent<Collider>());
        mShootStartTimer = -1;
        mAmmoCount--;
    }

    private void StartShootDebris()
    {
        mShootStartTimer = Time.time;
    }

    private void ShootHookAtMouse()
    {
        mHookController.Direction = Input.mousePosition - mMainCamera.WorldToScreenPoint(mShotRenderer.transform.position);
        mHookController.StartHooking();
    }

    private void CustomGravity()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        bool gravityFall = Time.time > mJumpTimer + (kJumpDuration * ((mInputJumpPressed) ? 2 : 1));
        body.velocity = transform.up * JumpSpeed * ((gravityFall) ? -1 : 1);
    }

    private void GetGroundStatus()
    {
        if (Physics.CheckSphere(transform.position + (transform.up * -1 * kBoundBox.y * 0.25f), 0.1f, kTerrainMask))
        {
            Debug.DrawLine(transform.position, transform.position + (transform.up * -1 * kBoundBox.y * 0.25f), Color.red);
            mIsGrounded = true;
        }
        else
        {
            mIsGrounded = false;
        }
        mCanJump = mIsGrounded;
    }

    private void GetInput()
    {
        mInputAxes.x = Input.GetAxis("Horizontal");
        mInputJumpPressed = Input.GetButton("Jump");
        mInputSprintPressed = Input.GetButton("Sprint");
        mInputHookShoot = Input.GetMouseButtonDown(1);
        mInputLastTickShootDebris = mInputShootDebris;
        mInputShootDebris = Input.GetMouseButton(0);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Consume enemy
        }
        if (collision.gameObject.CompareTag("Debris"))
        {
            if (CanPickUpAmmo())
            {
                PickUpAmmo(collision.gameObject);
            }
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Die
            GameObject playerDeath = (GameObject)GameObject.Find("PlayerDeath");
            playerDeath.transform.position = transform.position;
            playerDeath.GetComponent<ParticleSystem>().Play();
            FadeOut();
        }
    }

    private void FadeOut()
    {
        mState = PlayerState.Dead;
        gameObject.layer = LayerMask.NameToLayer("DepartingEffect");
        iTween.FadeTo(mPlayerWalkRenderer, iTween.Hash("alpha", 0.0f, "time", 0.25f));
        iTween.FadeTo(mPlayerIdleRenderer, iTween.Hash("alpha", 0.0f, "time", 0.25f));
        Invoke("FadeDeathMenu", 0.25f);
    }

    void FadeDeathMenu()
    {
        GameObject.Find("MenuController").GetComponent<MenuController>().MenuTransition(MenuController.MenuState.Death);
    }

    public void EndGame()
    {
        mState = PlayerState.Idle;
        // Do a dance?
    }

    private void PickUpAmmo(GameObject gameObject)
    {
        if (gameObject != null)
        {
            mDebris.Push(gameObject);
            gameObject.SetActive(false);
            gameObject.GetComponent<DebrisController>().PickedUp();
        }
        mAmmoCount++;
    }

    private bool CanPickUpAmmo()
    {
        // Only one requirement for now
        return mAmmoCount < kMaxAmmoCount;
    }

    #region State Ticks

    private void TickIdle()
    {
    }

    private void TickMoving()
    {
        Vector3 topOfCapsule = transform.position + ((mInputAxes.x > 0) ? 1 : -1) * transform.right * kBoundBox.x * 0.5f + Vector3.up * kBoundBox.y * 0.5f;
        Vector3 bottomOfCapsule = transform.position + ((mInputAxes.x > 0) ? 1 : -1) * transform.right * kBoundBox.x * 0.5f + Vector3.up * kBoundBox.y * -0.1f;
        Debug.DrawLine(topOfCapsule, bottomOfCapsule, Color.red);
        if (mInputAxes.sqrMagnitude != 0)
        {
            if (mInputAxes.x > 0)
            {
                mFacingDirection = Direction.Right;
            }
            else if (mInputAxes.x < 0)
            {
                mFacingDirection = Direction.Left;
            }
            if (!Physics.CheckCapsule(topOfCapsule, bottomOfCapsule, kBoundBox.x * 0.5f,
                kTerrainMask))
            {
                Vector3 newPos = transform.position;
                newPos.Set(newPos.x + mInputAxes.x * mSpeed * ((IsSprinting) ? kSprintMultiplier : 1) * Time.deltaTime, newPos.y, newPos.z);
                GetComponent<Rigidbody>().MovePosition(newPos);
            }
            else
            {
                // Do nothing/correct
            }
        }
        if (mCanJump && mInputJumpPressed)
        {
            mCanJump = mIsGrounded = false;
            GetComponent<Rigidbody>().velocity = transform.up * kJumpSpeed;
            mJumpTimer = Time.time;
        }
    }

    #endregion State Ticks

    void OnDrawGizmos()
    {
        Vector3 topOfCapsule = transform.position + ((mInputAxes.x > 0) ? 1 : -1) * transform.right * kBoundBox.x * 0.5f + Vector3.up * kBoundBox.y * 0.5f;
        Vector3 bottomOfCapsule = transform.position + ((mInputAxes.x > 0) ? 1 : -1) * transform.right * kBoundBox.x * 0.5f + Vector3.up * kBoundBox.y * -0.1f;
        Debug.DrawLine(topOfCapsule, bottomOfCapsule, Color.red);
        Gizmos.DrawWireSphere(topOfCapsule, kBoundBox.x * 0.5f);
        Gizmos.DrawWireSphere(bottomOfCapsule, kBoundBox.x * 0.5f);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position + (transform.up * -1 * kBoundBox.y * 0.25f), 0.1f);
    }
}