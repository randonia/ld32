using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private enum PlayerState
    {
        Idle,
        Moving
    }

    /// <summary>
    /// Player's state, defaults to Idle
    /// </summary>
    private PlayerState mState = PlayerState.Idle;

    public GameObject PREFAB_DEBRIS;
    private GameObject mHook;
    private HookController mHookController;
    private GameObject mShotRenderer;
    private GameObject mMainCameraGO;
    private Camera mMainCamera;

    #region Input member variables

    private Vector2 mInputAxes;
    private bool mInputJumpPressed;
    private bool mInputHookShoot;

    private bool mInputShootDebris;
    private bool mInputLastTickShootDebris;

    #endregion Input member variables

    private Vector3 kBoundBox;

    private float mSpeed = 4.0f;

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

    public float kJumpSpeed = 7f;

    public bool IsGrounded { get { return mIsGrounded; } }

    private int kTerrainMask;
    private float mJumpTimer = float.MinValue;
    private const float kJumpDuration = 0.25f;

    // FOR TESTING
    private int mAmmoCount = 1;

    public string AmmoCount { get { return mAmmoCount.ToString(); } }

    private const int kMaxAmmoCount = 10;

    public string AmmoWeight { get { return string.Format("{0}%", ((mAmmoCount / (float)kMaxAmmoCount) * 100f).ToString()); } }

    private Stack<GameObject> mDebris;

    public string DebugString
    {
        get
        {
            return string.Format("DebrisShotLength: {0}\nCanShoot: {1}\nTime: {2}\nShotTimer: {3}",
                DebrisShotLength, CanShootDebris, Time.time, (mShootStartTimer + kShootMaxTimer));
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
                break;
            case PlayerState.Moving:
                TickMoving();
                break;
        }
        // Test for hook shoot
        if (CanUseHook && mInputHookShoot)
        {
            ShootHookAtMouse();
        }
        if (CanShootDebris && mInputShootDebris)
        {
            Debug.Log("Starting shoot");
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
                Debug.Log("Finished shooting" + mInputShootDebris + "," + (Time.time > mShootStartTimer + kShootMaxTimer));
                FinishShootingDebris();
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
        }
        newDebris.transform.position = mShotRenderer.transform.position;
        newDebris.GetComponent<Rigidbody>().velocity = mShootDir * kMaxThrowForce * shootVal;
        newDebris.GetComponent<DebrisController>().StartThrow();
        // Assumes only one collider
        Physics.IgnoreCollision(GetComponent<Collider>(), newDebris.GetComponent<Collider>());
        mShootStartTimer = -1;
    }

    private void StartShootDebris()
    {
        mShootStartTimer = Time.time;
    }

    private void ShootHookAtMouse()
    {
        mHookController.Direction = Input.mousePosition - mMainCamera.WorldToScreenPoint(transform.position);
        mHookController.StartHooking();
    }

    private void CustomGravity()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        bool gravityFall = Time.time > mJumpTimer + (kJumpDuration * ((mInputJumpPressed) ? 2 : 1));
        body.velocity = transform.up * kJumpSpeed * ((gravityFall) ? -1 : 1);
    }

    private void GetGroundStatus()
    {
        if (Physics.CheckSphere(transform.position + (transform.up * -1 * kBoundBox.y * 0.5f), 0.1f, kTerrainMask))
        {
            Debug.DrawLine(transform.position, transform.position + (transform.up * -1 * kBoundBox.y * 0.5f), Color.red);
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
    }

    private void PickUpAmmo(GameObject gameObject)
    {
        mDebris.Push(gameObject);
        gameObject.SetActive(false);
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
        if (mInputAxes.sqrMagnitude != 0)
        {
            mState = PlayerState.Moving;
        }
    }

    private void TickMoving()
    {
        if (mInputAxes.sqrMagnitude != 0)
        {
            RaycastHit2D castHit = Physics2D.BoxCast(transform.position, kBoundBox * 0.5f, 0, mInputAxes, 0.2f);
            Debug.DrawRay(transform.position, mInputAxes, Color.green);
            if (castHit.collider == null)
            {
                Vector3 newPos = transform.position;
                newPos.Set(newPos.x + mInputAxes.x * mSpeed * Time.deltaTime, newPos.y, newPos.z);
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
}