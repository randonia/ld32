using System.Collections;
using UnityEngine;

public class HookController : MonoBehaviour
{
    public const float RAD2DEG = 57.2957795f;
    private GameObject mPlayer;
    private GameObject mRendererObject;

    private enum HookState
    {
        Ready,
        Hooking,
        Attached
    }

    private HookState mState;

    public string State { get { return mState.ToString(); } }

    private readonly Vector3 kPlayerHookOffset = Vector3.up * 0.55f;

    public Vector3 Origin { get { return mPlayer.transform.position + kPlayerHookOffset; } }

    private Vector3 mDirection;

    public Vector3 Direction { get { return mDirection; } set { mDirection = value.normalized; } }

    private const float kSpeed = 15f;

    private GameObject mLatchedTarget;

    private const float kMaxLength = 5;
    private readonly float kMaxLengthSqr = Mathf.Pow(kMaxLength, 2);

    private LineRenderer mLineRenderer;

    public bool IsAvailable { get { return mState.Equals(HookState.Ready); } }

    // Use this for initialization
    void Start()
    {
        mPlayer = GameObject.Find("Player");
        // Silly workaround to get a child by name
        foreach (Transform child in transform)
        {
            if (child.name.Equals("renderer"))
            {
                mRendererObject = child.gameObject;
                break;
            }
        }

        mLineRenderer = GetComponent<LineRenderer>();
    }

    public void StartHooking()
    {
        gameObject.SetActive(true);
        mState = HookState.Hooking;
        transform.position = Origin;
        mRendererObject.transform.rotation = Quaternion.identity;
        mRendererObject.transform.Rotate(transform.forward, RAD2DEG * Mathf.Atan2(mDirection.y, mDirection.x) - 90);
        mLineRenderer.SetPosition(0, Origin);
        mLineRenderer.SetPosition(1, Origin);
    }

    public void StopHooking()
    {
        mState = HookState.Ready;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        switch (mState)
        {
            case HookState.Ready:
                TickIdle();
                break;
            case HookState.Hooking:
                TickHooking();
                if ((transform.position - Origin).sqrMagnitude >= kMaxLengthSqr)
                {
                    StopHooking();
                }
                break;
            case HookState.Attached:
                TickAttached();
                break;
        }
    }

    private void TickAttached()
    {
        // Bring down the target!
        if (mLatchedTarget != null)
        {
            transform.position = mLatchedTarget.transform.position;
            // Move the line renderer
            mLineRenderer.SetPosition(0, Origin + transform.forward);
            mLineRenderer.SetPosition(1, transform.position + transform.forward);
            // Update the rotation
            mRendererObject.transform.rotation = Quaternion.identity;
            Vector3 facingAngle = (mLatchedTarget.transform.position - Origin).normalized;
            mRendererObject.transform.Rotate(transform.forward, RAD2DEG * Mathf.Atan2(facingAngle.y, facingAngle.x) - 90);
        }
        else
        {
            StopHooking();
        }
    }

    private void TickIdle()
    {
        // Do nothing
    }

    private void TickHooking()
    {
        transform.Translate(mDirection * Time.deltaTime * kSpeed);
        // Move the line renderer
        mLineRenderer.SetPosition(0, Origin + transform.forward);
        mLineRenderer.SetPosition(1, transform.position + transform.forward);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.DrawLine(Origin, other.transform.position, Color.red, 1.0f);
        if (other.gameObject.CompareTag("Enemy"))
        {
            // Hook the enemy
            if (!mState.Equals(HookState.Attached) && other.gameObject.GetComponent<EnemyController>().GetHooked())
            {
                LatchOn(other.gameObject);
            }
        }
        if (other.gameObject.CompareTag("Terrain"))
        {
            StopHooking();
        }
    }

    private void LatchOn(GameObject other)
    {
        mState = HookState.Attached;
        mLatchedTarget = other;
    }
}