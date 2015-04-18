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

    #region Input member variables

    private Vector2 mInputAxes;
    private bool mInputJumpPressed;

    #endregion Input member variables

    private Vector2 kBoundBox;
    public float mSpeed = 2.0f;

    /// <summary>
    /// Set at the beginning of update
    /// </summary>
    private bool mIsGrounded;

    /// <summary>
    /// This gets reset each time the player hits the ground while they can't jump.
    /// </summary>
    private bool mCanJump;

    private float mJumpVelocity;

    public float kJumpSpeed = 5f;

    public bool IsGrounded { get { return mIsGrounded; } }

    public string DebugString
    {
        get
        {
            return string.Format("Is Grounded: {0}\nInput: {1}\njumpVelocity: {2}\nJumpPressed: {3}\nCanJump: {4}\nJumpProbe: {5}",
                IsGrounded, mInputAxes, mJumpVelocity, mInputJumpPressed, mCanJump, mJumpVelocity * Time.deltaTime);
        }
    }

    // Use this for initialization
    void Start()
    {
        mInputAxes = Vector2.zero;
        kBoundBox = GetComponent<BoxCollider2D>().bounds.size * 0.8f;
    }

    // Update is called once per frame
    void Update()
    {
        // Apply vertical movement
        mJumpVelocity = (IsGrounded) ? 0.0f : mJumpVelocity + Physics2D.gravity.y * 3f * Time.deltaTime;
        transform.Translate(0, mJumpVelocity * Time.deltaTime, 0);

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
    }

    private void GetGroundStatus()
    {
        RaycastHit2D castHit = Physics2D.BoxCast(transform.position, kBoundBox, 0, transform.up * -1.0f, Mathf.Max(0.2f, Mathf.Abs(mJumpVelocity * Time.deltaTime)));
        if (castHit.collider != null)
        {
            transform.position.Set(transform.position.x, castHit.point.y, 0);
        }
        mIsGrounded = castHit.collider != null;
        mCanJump = mIsGrounded;
    }

    private void GetInput()
    {
        mInputAxes.x = Input.GetAxis("Horizontal");
        mInputJumpPressed = Input.GetButton("Jump");
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
            Debug.DrawRay(transform.position, mInputAxes);
            if (castHit.collider == null)
            {
                transform.Translate(mInputAxes.x * mSpeed * Time.deltaTime, 0.0f, 0.0f);
            }
            else
            {
                // Do nothing/correct
            }
        }
        if (mCanJump && mInputJumpPressed)
        {
            mCanJump = mIsGrounded = false;
            mJumpVelocity = kJumpSpeed;
        }
    }

    #endregion State Ticks
}