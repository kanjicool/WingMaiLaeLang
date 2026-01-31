using UnityEngine;
using System.Collections;

public class HumanController : MonoBehaviour
{
    [Header("Settings")]
    public HumanData data;
    public Animator anim;

    [Header("Lane Settings")]
    public float laneChangeSpeed = 10f;
    public int currentLane = 1;

    private bool isActive = false;
    private float nextAttackTime = 0f;

    private Transform playerTransform;
    private bool isDead = false;
    
    
    void Start()
    {
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }

        //if (anim) anim.Play("Idle", 0, Random.Range(0f, 1f));

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
        if (!isActive) return;

        //switch (data.type)
        //{
        //    case HumanData.HumanType.Civilian: HandleCivilianBehavior(distanceToPlayer); break;
        //    case HumanData.HumanType.Police: HandleRunnerBehavior(distanceToPlayer); break;
        //    case HumanData.HumanType.Soldier: HandleSoldierBehavior(distanceToPlayer); break;
        //}

        float laneDistance = GameManager.Instance.laneDistance;
        float targetX = (currentLane - 1) * laneDistance;
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, laneChangeSpeed * Time.deltaTime);
        transform.position = newPos;

    }

    void HandleCivilianBehavior(float dist)
    {
        if (dist < 8f)
        {
            if (anim) anim.SetBool("IsDucking", true); // ใช้ท่า Duck แทนการกลัว
            anim.SetBool("IsRunning", false);
        }
        else
        {
            if (anim) anim.SetBool("IsDucking", false);
        }
    }

    void HandleRunnerBehavior(float dist)
    {
        transform.Translate(Vector3.forward * data.moveSpeed * Time.deltaTime);
        if (anim) anim.SetBool("IsRunning", true);

        // Raycast เช็คข้างหน้า ถ้าเจอสิ่งกีดขวาง ให้เปลี่ยนเลน
        if (Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 3f))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                ChangeLaneRandomly();
            }
        }
    }

    void HandleSoldierBehavior(float dist)
    {
        transform.LookAt(playerTransform);

        if (anim)
        {
            anim.SetBool("IsRunning", false);
        }

        if (Time.time >= nextAttackTime)
        {
            Shoot();
            nextAttackTime = Time.time + data.attackRate;
        }
    }

    void Shoot()
    {
        // Logic สร้างกระสุนวิ่งเข้าหา Player
        Debug.Log("Soldier Shooting!");
        if (data.projectilePrefab != null)
        {
            Instantiate(data.projectilePrefab, transform.position + Vector3.up, transform.rotation);
        }

        // ถ้าไม่มี projectile ก็สั่งลดจำนวนซอมบี้โดยตรงเลยก็ได้ (HitScan)
        // SwarmManager.Instance.RemoveZombie(); 
    }

    void ChangeLaneRandomly()
    {
        // สุ่มเลนซ้ายขวา แต่ไม่ให้ออกนอกขอบ (0-2)
        int direction = Random.Range(0, 2) == 0 ? -1 : 1;
        int target = currentLane + direction;

        if (target >= 0 && target <= 2)
        {
            currentLane = target;
        }
    }

    public void OnEaten()
    {
        if (anim) anim.SetTrigger("Dead");
        // Logic การตายและเพิ่มจำนวนซอมบี้
        // SwarmManager.Instance.AddZombie(data.zombieValue);
        Destroy(gameObject, 1f); // รอเล่นท่าตายจบแป๊บนึง
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    // ถ้าโดนซอมบี้ชน
    //    if (other.CompareTag("Zombie"))
    //    {
    //        // ปิด Collider กันชนซ้ำ
    //        GetComponent<Collider>().enabled = false;
    //        OnEaten();
    //    }
    //}
}
