using System.Collections;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    public GameObject PREFAB_DEBRIS;

    private const float kRandXRange = 2f;
    private const float kRandYRange = 5f;
    private AudioSource mAudioExplosion;

    // Use this for initialization
    void Start()
    {
        mAudioExplosion = GetComponent<AudioSource>();
        if (mAudioExplosion != null)
        {
            mAudioExplosion.Play();
        }
    }

    public void StartDebrisExplosion()
    {
        // Create some debris
        int countToCreate = Random.Range(3, 5);
        for (int i = 0; i < countToCreate; ++i)
        {
            Quaternion randRot = Quaternion.identity;
            randRot.z = Random.Range(0f, 180f);
            GameObject debris = (GameObject)GameObject.Instantiate(PREFAB_DEBRIS, transform.position, randRot);
            debris.GetComponent<Rigidbody>().velocity = RandomExplosionVector();
        }
    }

    public void StartRegularExplosion()
    {
        // Do some regular explody stuff
    }

    private Vector3 RandomExplosionVector()
    {
        return new Vector3(Random.value * kRandXRange - kRandXRange * 0.5f, Random.value * kRandYRange, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<ParticleSystem>().isStopped)
        {
            GameObject.Destroy(gameObject);
        }
    }
}