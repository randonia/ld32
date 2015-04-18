﻿using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private enum EnemyState
    {
        Roaming,
        Hooked
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

    private EnemyState mState;
    public PatrolMode mPatrolMode;
    private RenderDirection mRenderDirection;

    public Vector3[] PatrolPoints;
    private int mCurrPatrolNode;
    private PatrolDir mPatrolOscillateDir = PatrolDir.Forward;
    private const float kNodeThreshold = 0.5f;
    private const float kSpeed = 0.5f;

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
                        mCurrPatrolNode = Mathf.Max(PatrolPoints.Length - 1, 0);
                    }
                    break;
            }
        }
    }

    private void TickHooked()
    {
    }

    #endregion Tick methods

    public void GetHooked()
    {
        mState = EnemyState.Hooked;
        Rigidbody body = GetComponent<Rigidbody>();
        body.useGravity = true;
        body.isKinematic = false;
        body.mass = 10;
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