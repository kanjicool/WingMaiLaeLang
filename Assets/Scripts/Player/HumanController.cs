using UnityEngine;
using System.Collections;

public class HumanController : MonoBehaviour
{
    [Header("Settings")]
    public HumanData data;
    private Animator anim;

    [Header("Lane Settings")]
    public float laneChangeSpeed = 10f;
    public int currentLane = 1;

    [Header("Detection Settings")]
    public float laneCheckDistance = 6.0f;
    public float jumpCheckDistance = 5.0f;
    public float jumpTriggerDist = 2.0f;
    public LayerMask obstacleLayer;


    [Header("Jump Settings (For Runner)")]
    public float jumpHeight = 1.5f;
    public float jumpDuration = 0.8f;

    // State
    private bool isActive = false;
    private bool isJumping = false;
    private float nextAttackTime = 0f;
    private bool isDead = false;

    // Movement Internal
    private Transform playerTransform;
    private float verticalVelocityY = 0f;


    void Start()
    {
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        
        anim = GetComponentInChildren<Animator>();

        if (anim) anim.Play("Idle", 0, Random.Range(0f, 1f));

        transform.rotation = Quaternion.Euler(0, 180, 0);

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

        float distanceToPlayer = transform.position.z - playerTransform.position.z;
        if (distanceToPlayer < data.detectRange && distanceToPlayer > 0) isActive = true;
        
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

            float newX = Mathf.Lerp(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
            float newY = verticalVelocityY;

            transform.position = new Vector3(newX, newY, transform.position.z);
        }

    }

    void HandleCivilianBehavior(float dist)
    {
        transform.LookAt(playerTransform);

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
        Quaternion runRotation = Quaternion.LookRotation(Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, runRotation, 10f * Time.deltaTime);
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
        transform.LookAt(playerTransform);

        if (anim) anim.SetBool("IsRunning", false);

        if (Time.time >= nextAttackTime)
        {
            Shoot();
            nextAttackTime = Time.time + data.attackRate;
        }
    }

    void Shoot()
    {
        Debug.Log("Soldier Shooting!");
        if (data.projectilePrefab != null)
        {
            Instantiate(data.projectilePrefab, transform.position + Vector3.up, transform.rotation);
        }

        // ถ้าไม่มี projectile ก็สั่งลดจำนวนซอมบี้โดยตรงเลยก็ได้ (HitScan)
        // SwarmManager.Instance.RemoveZombie(); 
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
        if (anim) anim.SetTrigger("Dead");

        if (data.bloodEffectPrefab != null)
        {
            Vector3 bloodPos = transform.position + Vector3.up * 1.0f;
            GameObject blood = Instantiate(data.bloodEffectPrefab, bloodPos, Quaternion.identity);
            Destroy(blood, 2f);
        }

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        Destroy(gameObject, 2f);
    }

}
