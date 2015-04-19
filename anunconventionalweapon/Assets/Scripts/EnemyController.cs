using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private enum EnemyState
    {
        Roaming,
        Hooked,
        Exploding
    }

    public enum PatrolMode
    {
        Oscillate,
        Linear
    }

    private enum PatrolDir
    {
        Forward = 1,
        Reverse = -1
    }

    private enum RenderDirection
    {
        Left,
        Right
    }

    public bool mDestroyedByDebrisOnly;
    public bool mHookable;

    public GameObject PREFAB_EXPLOSION;
    private EnemyState mState;
    public PatrolMode mPatrolMode;
    private RenderDirection mRenderDirection;

    public Vector3[] PatrolPoints;
    private int mCurrPatrolNode;
    private PatrolDir mPatrolOscillateDir = PatrolDir.Forward;
    public float kNodeThreshold = 0.5f;
    private const float kSpeed = 1f;

    private Vector3 CurrPatrolDestination
    {
        get
        {
            return (PatrolPoints.Length > mCurrPatrolNode) ? PatrolPoints[mCurrPatrolNode] : transform.position;
        }
    }

    // Use this for initialization
    void Start()
    {
        mState = EnemyState.Roaming;
    }

    // Update is called once per frame
    void Update()
    {
        mRenderDirection = (CurrPatrolDestination.x > transform.position.x) ? RenderDirection.Right : RenderDirection.Left;
        switch (mState)
        {
            case EnemyState.Roaming:
                TickRoaming();
                break;
            case EnemyState.Hooked:
                TickHooked();
                break;
        }
        Quaternion targetRot = transform.rotation;
        targetRot.Set(transform.rotation.x, (mRenderDirection == RenderDirection.Left) ? 0 : 180, transform.rotation.z, transform.rotation.w);
        transform.rotation = targetRot;
    }

    #region Tick methods

    private void TickRoaming()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        Vector3 currDest = CurrPatrolDestination;
        Vector3 moveDir = (currDest - transform.position).normalized;
        body.MovePosition(transform.position + moveDir * Time.deltaTime * kSpeed);

        if ((transform.position - currDest).sqrMagnitude <= kNodeThreshold)
        {
            mCurrPatrolNode += (int)mPatrolOscillateDir;
            switch (mPatrolMode)
            {
                case PatrolMode.Linear:
                    mCurrPatrolNode++;
                    if (mCurrPatrolNode >= PatrolPoints.Length)
                    {
                        mCurrPatrolNode = 0;
                    }
                    break;
                case PatrolMode.Oscillate:
                    if (mPatrolOscillateDir == PatrolDir.Forward && mCurrPatrolNode >= PatrolPoints.Length)
                    {
                        mPatrolOscillateDir = PatrolDir.Reverse;
                        mCurrPatrolNode = Mathf.Max(PatrolPoints.Length - 2, 0);
                    }
                    else if (mPatrolOscillateDir == PatrolDir.Reverse && mCurrPatrolNode <= 0)
                    {
                        mPatrolOscillateDir = PatrolDir.Forward;
                        mCurrPatrolNode = 0;
                    }
                    break;
            }
        }
    }

    private void TickHooked()
    {
    }

    #endregion Tick methods

    public bool GetHooked()
    {
        if (mHookable)
        {
            mState = EnemyState.Hooked;
            Rigidbody body = GetComponent<Rigidbody>();
            body.useGravity = true;
            body.isKinematic = false;
            body.mass = 10;
        }
        else
        {
            // Do nothing? Get stunned?
        }
        return mHookable;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (mState != EnemyState.Exploding && collision.gameObject.CompareTag("Terrain"))
        {
            DoImpactExplosion();
        }
        if (collision.gameObject.CompareTag("Debris") && mDestroyedByDebrisOnly)
        {
            // Handle own explody logic
            DoNonDebrisCreationExplosion();
            collision.gameObject.GetComponent<DebrisController>().FadeOut();
        }
    }

    private void DoNonDebrisCreationExplosion()
    {
        mState = EnemyState.Exploding;
        GameObject.Instantiate(PREFAB_EXPLOSION, transform.position, Quaternion.identity);
        GameObject.Destroy(gameObject);
    }

    private void DoImpactExplosion()
    {
        mState = EnemyState.Exploding;
        GameObject explosion = (GameObject)GameObject.Instantiate(PREFAB_EXPLOSION, transform.position, Quaternion.identity);
        explosion.GetComponent<ExplosionController>().StartDebrisExplosion();
        GameObject.Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        // Don't affect other Gizmos
        Color beginColor = Gizmos.color;

        // Draw each path node
        for (int i = 0; i < PatrolPoints.Length; ++i)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(PatrolPoints[i], Vector3.one * 0.1f);
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, PatrolPoints[i]);
        }
        // Draw current destination
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(CurrPatrolDestination, Vector3.one * 0.2f);
        // Draw the sphere of node-ness
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, kNodeThreshold);

        Gizmos.color = beginColor;
    }
}