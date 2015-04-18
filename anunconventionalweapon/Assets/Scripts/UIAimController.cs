using System.Collections;
using UnityEngine;

public class UIAimController : MonoBehaviour
{
    private LineRenderer mRenderer;

    private float mShotAmount;

    public float ShotAmount { get { return ShotAmount; } set { mShotAmount = Mathf.Clamp(value, 0, 1); } }

    // Use this for initialization
    void Start()
    {
        mRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        mRenderer.SetPosition(0, transform.position);
        mRenderer.SetPosition(1, transform.position + transform.up * mShotAmount);
    }
}