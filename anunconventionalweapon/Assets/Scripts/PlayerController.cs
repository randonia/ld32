using System.Collections;
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

    private GameObject mHook;
    private HookController mHookController;
    private GameObject mMainCameraGO;
    private Camera mMainCamera;

    #region Input member variables

    private Vector2 mInputAxes;
    private bool mInputJumpPressed;
    private bool mInputHookShoot;

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

    private bool CanShoot { get { return mHookController != null && mHookController.IsAvailable; } }

    public float kJumpSpeed = 7f;

    public bool IsGrounded { get { return mIsGrounded; } }

    private int kTerrainMask;
    private float mJumpTimer = float.MinValue;
    private const float kJumpDuration = 0.25f;

    public string DebugString
    {
        get
        {
            return string.Format("Is Grounded: {0}\nHookState: {1}\nCanShoot: {2}",
                IsGrounded, (mHookController != null) ? mHookController.State : "off", CanShoot);
        }
    }

    // Use this for initialization
    void Start()
    {
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
        if (CanShoot && mInputHookShoot)
        {
            ShootHookAtMouse();
        }
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
        mInputHookShoot = Input.GetMouseButton(1);
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