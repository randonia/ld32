using System.Collections;
using UnityEngine;

public class HookController : MonoBehaviour
{
    public const float RAD2DEG = 57.2957795f;
    private GameObject mPlayer;
    private GameObject mRendererObject;

    private Vector3 mOrigin;

    public Vector3 Origin { get { return mOrigin; } set { mOrigin = value; } }

    private Vector3 mDirection;

    public Vector3 Direction { get { return mDirection; } set { mDirection = value.normalized; } }

    private const float kSpeed = 0.5f;

    private const float kMaxLength = 10.0f;

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
    }

    public void StartHooking()
    {
        mRendererObject.transform.rotation = Quaternion.identity;
        mRendererObject.transform.Rotate(transform.forward, RAD2DEG * Mathf.Atan2(mDirection.y, mDirection.x) - 90);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(mDirection * Time.deltaTime * kSpeed);
    }
}