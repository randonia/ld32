using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject mPlayer;
    private Camera mMainCamera;

    private const float kHorizontalMoveDistanceCap = 2f;
    private const float kHorizontalCorrectionSpeed = 2f;
    private const float kVerticalMoveDistanceCap = 5.0f;
    private float kCameraSize;

    // Use this for initialization
    void Start()
    {
        mPlayer = GameObject.Find("Player");
        mMainCamera = GetComponent<Camera>();
        kCameraSize = mMainCamera.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 posDelta = mPlayer.transform.position - transform.position;
        transform.Translate(kHorizontalCorrectionSpeed * (posDelta.x / kHorizontalMoveDistanceCap) * Time.deltaTime, 0f, 0f);
        mMainCamera.orthographicSize = kCameraSize + Mathf.Abs(posDelta.x / kHorizontalMoveDistanceCap) * 0.5f;
    }
}