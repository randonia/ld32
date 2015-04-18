using System.Collections;
using UnityEngine;

public class DebrisController : MonoBehaviour
{
    private enum DebrisState
    {
        Idle,
        Carried,
        Thrown
    }

    private DebrisState mState;

    public Sprite[] DebrisSprites;

    private const float kLifeSpan = 5.0f;
    private float mStartTime;

    // Use this for initialization
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = GetRandomSprite();
        mStartTime = Time.time;
        mState = DebrisState.Idle;
    }

    private Sprite GetRandomSprite()
    {
        return DebrisSprites[Random.Range(0, DebrisSprites.Length - 1)];
    }

    // Update is called once per frame
    void Update()
    {
        switch (mState)
        {
            case DebrisState.Idle:
                TickIdle();
                break;
            case DebrisState.Carried:
                TickCarried();
                break;
            case DebrisState.Thrown:
                TickThrown();
                break;
        }
    }

    private void TickIdle()
    {
        if (Time.time > mStartTime + kLifeSpan)
        {
            GameObject.Destroy(gameObject);
        }
    }

    private void TickCarried()
    {
        throw new System.NotImplementedException();
    }

    private void TickThrown()
    {
        throw new System.NotImplementedException();
    }
}