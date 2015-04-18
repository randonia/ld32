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

    private Vector3 kBoundBox;
    public float mSpeed = 2.0f;

    /// <summary>
    /// Set at the beginning of update
    /// </summary>
    private bool mIsGrounded;

    /// <summary>
    /// This gets reset each time the player hits the ground while they can't jump.
    /// </summary>
    private bool mCanJump;

    public float kJumpSpeed = 5f;

    public bool IsGrounded { get { return mIsGrounded; } }

    private int kTerrainMask;

    public string DebugString
    {
        get
        {
            return string.Format("Is Grounded: {0}\nInput: {1}\nJumpPressed: {2}\nCanJump: {3}",
                IsGrounded, mInputAxes, mInputJumpPressed, mCanJump);
        }
    }

    // Use this for initialization
    void Start()
    {
        mInputAxes = Vector2.zero;
        kBoundBox = GetComponent<BoxCollider>().bounds.size;
        kTerrainMask = LayerMask.GetMask("Terrain");
    }

    // Update is called once per frame
    void Update()
    {
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
        }
    }

    #endregion State Ticks
}