using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifeTime = 3f;

    [Header("VFX")]
    public GameObject hitVFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Zombie"))
        {
            if (PlayerController.Instance != null)
            {
                if (PlayerController.Instance.isInvincible)
                {
                    SpawnHitEffect();
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    if (SwarmManager.Instance != null)
                    {
                        SwarmManager.Instance.RemoveZombie();
                    }
                }
            }

            SpawnHitEffect();
            Destroy(gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

    void SpawnHitEffect()
    {
        if (hitVFX != null)
        {
            GameObject vfx = Instantiate(hitVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }
    }
}
