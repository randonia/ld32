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

    private DebrisState mState = DebrisState.Idle;

    public Sprite[] DebrisSprites;

    private const float kLifeSpan = 5.0f;
    private const float kThrownLifeSpan = 20.0f;
    private float mStartTime;

    // Use this for initialization
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = GetRandomSprite();
        mStartTime = Time.time;
    }

    private Sprite GetRandomSprite()
    {
        return DebrisSprites[Random.Range(0, DebrisSprites.Length - 1)];
    }

    /// <summary>
    /// Once a debris is thrown, it no longer can be used by the player!
    /// </summary>
    public void StartThrow()
    {
        mState = DebrisState.Thrown;
        mStartTime = Time.time;
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
        // Nothing really, deactivated
    }

    // These really should be taken out
    private static Color[] DEBUGCOLORS = { Color.red, Color.green, Color.black, Color.blue, Color.white, Color.yellow, Color.magenta, Color.cyan };

    private Color myColor = DEBUGCOLORS[Random.Range(0, DEBUGCOLORS.Length - 1)];

    private void TickThrown()
    {
        if (Time.time > mStartTime + kThrownLifeSpan)
        {
            GameObject.Destroy(gameObject);
        }
        Debug.DrawLine(transform.position, transform.position + Vector3.forward, myColor, kThrownLifeSpan);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            FadeOut();
        }
    }

    public void FadeOut()
    {
        gameObject.layer = LayerMask.NameToLayer("DepartingEffect");
        iTween.FadeTo(gameObject, iTween.Hash("alpha", 0.0f, "time", 0.25f, "oncomplete", "DestroyObject"));
    }

    void DestroyObject()
    {
        GameObject.Destroy(gameObject);
    }
}