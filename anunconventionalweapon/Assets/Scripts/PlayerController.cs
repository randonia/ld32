using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private enum PlayerState
    {
        Idle,
        Moving,
        Jumping
    }

    /// <summary>
    /// Player's state, defaults to Idle
    /// </summary>
    private PlayerState mState = PlayerState.Idle;

    private Vector2 mInputAxes;

    public string InputAxes { get { return mInputAxes + "," + mInputAxes.sqrMagnitude; } }

    private Vector2 kBoundBox;
    public float mSpeed = 2.0f;

    // Use this for initialization
    void Start()
    {
        mInputAxes = Vector2.zero;
        kBoundBox = GetComponent<BoxCollider2D>().bounds.size * 0.8f;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        switch (mState)
        {
            case PlayerState.Idle:
                TickIdle();
                break;
            case PlayerState.Moving:
                TickMoving();
                break;
            case PlayerState.Jumping:
                TickJumping();
                break;
        }
    }

    private void GetInput()
    {
        mInputAxes.x = Input.GetAxis("Horizontal");
        //mInputAxes.y = Input.GetAxis("Vertical");
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
    }

    private void TickJumping()
    {
        throw new System.NotImplementedException();
    }

    #endregion State Ticks
}