using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class HumanController : MonoBehaviour
{
    [Header("Settings")]
    public HumanData data;
    private Animator anim;
    private AudioSource audioSource;

    [Header("Lane Settings")]
    public float laneChangeSmoothTime = 0.15f;
    public float maxLaneSpeed = 20f;
    public int currentLane = 1;

    [Header("Detection Settings")]
    public float laneCheckDistance = 6.0f;
    public float jumpCheckDistance = 5.0f;
    public float jumpTriggerDist = 2.0f;
    public LayerMask obstacleLayer;

    [Header("Cleanup Settings")]
    public float despawnDistance = 40f;
    public float cleanupBehindDist = 10f;

    [Header("Jump Settings (For Runner)")]
    public float jumpHeight = 1.5f;
    public float jumpDuration = 0.8f;

    // State
    private bool isActive = false;
    private bool isJumping = false;
    private float nextAttackTime = 0f;
    private bool isDead = false;

    private bool isDespawning = false;

    // Movement Internal
    private Transform playerTransform;
    private float verticalVelocityY = 0f;
    private float currentVelocityX;


    void Start()
    {
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }

        transform.SetParent(null);

        anim = GetComponentInChildren<Animator>();

        if (anim) anim.Play("Idle", 0, Random.Range(0f, 1f));

        transform.rotation = Quaternion.Euler(0, 180, 0);

        // --- Setup Audio ---
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        audioSource.volume = data.volume;

    }

    public void SetupLane(int laneIndex)
    {
        currentLane = laneIndex;

        if (GameManager.Instance != null)
        {
            float xPos = (currentLane - 1) * GameManager.Instance.laneDistance;
            transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
        }
    }


    void Update()
    {
        if (isDead || playerTransform == null) return;

        if (isDespawning) return;
        
        float distZ = transform.position.z - playerTransform.position.z;

        if (distZ < -cleanupBehindDist)
        {
            Destroy(gameObject);
            return;
        }

        if (distZ > despawnDistance)
        {
            if (data.type == HumanData.HumanType.Runner && isActive)
            {
                StartCoroutine(FadeOutAndDestroy());
                return;
            }
        }


        float distanceToPlayer = transform.position.z - playerTransform.position.z;

        if (!isActive && distanceToPlayer < data.detectRange && distanceToPlayer > 0)
        {
            isActive = true;
            PlayScreamSound(); // เล่นเสียงตกใจ
        }

        if (!isActive)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToPlayer);
            }
            return;
        }

        switch (data.type)
        {
            case HumanData.HumanType.Civilian:
                HandleCivilianBehavior(distanceToPlayer);
                break;
            case HumanData.HumanType.Runner:
                HandleRunnerBehavior(distanceToPlayer);
                break;
            case HumanData.HumanType.Soldier:
                HandleSoldierBehavior(distanceToPlayer);
                break;
        }

        if (GameManager.Instance != null)
        {
            float laneDistance = GameManager.Instance.laneDistance;
            float targetX = (currentLane - 1) * laneDistance;

            float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref currentVelocityX, laneChangeSmoothTime, maxLaneSpeed);
            float newY = verticalVelocityY;

            transform.position = new Vector3(newX, newY, transform.position.z);

            float visualForwardSpeed = (data.type == HumanData.HumanType.Runner) ? 10f : -10f;
            
            Vector3 directionToLook = new Vector3(currentVelocityX, 0, visualForwardSpeed);

            if (directionToLook != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
                // หมุนตัวอย่างนุ่มนวล
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }

    }

    void HandleCivilianBehavior(float dist)
    {
        if (dist < 8f)
        {
            if (anim)
            {
                anim.SetBool("IsDucking", true);
                anim.SetBool("IsRunning", false);
            }
        }
        else
        {
            if (anim) anim.SetBool("IsDucking", false);
        }
    }

    void HandleRunnerBehavior(float dist)
    {
        transform.Translate(Vector3.forward * data.moveSpeed * Time.deltaTime, Space.World);

        if (anim) anim.SetBool("IsRunning", true);

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float rayDist = 4.0f;

        Debug.DrawRay(origin, Vector3.forward * jumpCheckDistance, Color.yellow); // ระยะมองเห็น
        Debug.DrawRay(origin, Vector3.forward * jumpTriggerDist, Color.red);

        if (Physics.Raycast(origin, Vector3.forward, out RaycastHit hit, rayDist, obstacleLayer, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.CompareTag("Jumpable") && !isJumping)
            {
                Debug.Log("Runner sees Jumpable!");
                if (hit.distance <= jumpTriggerDist && !isJumping)
                {
                    StartCoroutine(JumpRoutine());
                }
            }
            else if (hit.collider.CompareTag("Obstacle"))
            {
                if (Mathf.Abs(transform.position.x - ((currentLane - 1) * GameManager.Instance.laneDistance)) < 0.5f)
                {
                    FindSafeLane();
                }

                Debug.Log("Runner sees Obstacle! Changing Lane."); 
            }
        }
    }

    void HandleSoldierBehavior(float dist)
    {
        if (anim) anim.SetBool("IsRunning", false);
        if (anim) anim.SetBool("IsIdleGun", true);

        if (Time.time >= nextAttackTime)
        {
            Shoot();
            nextAttackTime = Time.time + data.attackRate;
        }
    }

    void Shoot()
    {
        Debug.Log("Soldier Shooting!");

        if (data.shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(data.shootSound);
        }

        if (data.projectilePrefab != null)
        {
            // aimbot
            //Instantiate(data.projectilePrefab, transform.position + Vector3.up, transform.rotation);

            // normal
            Quaternion bulletRotation = Quaternion.Euler(0, 180, 0);
            Instantiate(data.projectilePrefab, transform.position + Vector3.up * 1.5f, bulletRotation);
        }
    }

    IEnumerator JumpRoutine()
    {
        isJumping = true;
        if (anim) anim.SetTrigger("Jump");

        float timer = 0;
        float startY = transform.position.y;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / jumpDuration;

            verticalVelocityY = startY + (4 * jumpHeight * progress * (1 - progress));

            yield return null;
        }

        verticalVelocityY = startY; 
        isJumping = false;
    }

    void FindSafeLane()
    {
        bool isLeftSafe = IsLaneSafe(currentLane - 1);
        bool isRightSafe = IsLaneSafe(currentLane + 1);

        if (isLeftSafe && isRightSafe)
        {
            ChangeLane(Random.Range(0, 2) == 0 ? -1 : 1);
        }
        else if (isRightSafe)
        {
            ChangeLane(1);
        }
        else if (isLeftSafe)
        {
            ChangeLane(-1);
        }
    }


    bool IsLaneSafe(int targetLaneIndex)
    {
        if (targetLaneIndex < 0 || targetLaneIndex > 2) return false;
        if (GameManager.Instance == null) return false;

        float laneX = (targetLaneIndex - 1) * GameManager.Instance.laneDistance;
        Vector3 rayOrigin = new Vector3(laneX, transform.position.y + 0.5f, transform.position.z);
        
        Debug.DrawRay(rayOrigin, Vector3.forward * laneCheckDistance, Color.blueViolet, 10.0f);

        if (Physics.Raycast(rayOrigin, Vector3.forward, laneCheckDistance, obstacleLayer, QueryTriggerInteraction.Collide))
        {
            return false;
        }

        return true;
    }

    void ChangeLane(int direction)
    {
        int target = currentLane + direction;
        if (target >= 0 && target <= 2)
        {
            currentLane = target;
        }
    }


    public void OnEaten()
    {
        isDead = true;

        StopAllCoroutines();

        if (anim) anim.SetTrigger("Dead");

        if (data.deathSound != null)
        {
            // ใช้ PlayClipAtPoint เพราะเดี๋ยวตัวนี้จะโดน Destroy เสียงจะได้ไม่ขาดหาย
            AudioSource.PlayClipAtPoint(data.deathSound, transform.position, data.volume);
        }

        if (data.bloodEffectPrefab != null)
        {
            Vector3 bloodPos = transform.position + Vector3.up * 1.0f;
            GameObject blood = Instantiate(data.bloodEffectPrefab, bloodPos, Quaternion.identity);
            Destroy(blood, 2f);
        }

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        StartCoroutine(DeathFallRoutine());

        Destroy(gameObject, 2f);
    }

    IEnumerator DeathFallRoutine()
    {
        float fallSpeed = 0f;
        float gravity = 50f; // แรงดึงดูด (ปรับได้)

        while (transform.position.y > 0.05f)
        {
            fallSpeed += gravity * Time.deltaTime;

            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

            yield return null;
        }

        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;
    }

    IEnumerator FadeOutAndDestroy()
    {
        isDespawning = true; // ล็อคไม่ให้ Update ทำงานซ้อน
        float timer = 0f;
        float duration = 1.0f; // ใช้เวลา 1 วินาทีในการหายตัว
        Vector3 initialScale = transform.localScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // ย่อขนาดลงจาก 1 -> 0
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, progress);

            // (Optional) ถ้า Runner วิ่งอยู่ ให้มันวิ่งต่อไปด้วยระหว่างที่ตัวเล็กลง
            if (data.type == HumanData.HumanType.Runner)
            {
                transform.Translate(Vector3.forward * data.moveSpeed * Time.deltaTime, Space.World);
            }

            yield return null;
        }

        Destroy(gameObject);
    }


    void PlayScreamSound()
    {
        if (data.screamSound != null && audioSource != null)
        {
            // ใช้ PlayOneShot เพื่อไม่ให้ตัดเสียงอื่นที่อาจเล่นอยู่
            audioSource.PlayOneShot(data.screamSound);
        }
    }

}
