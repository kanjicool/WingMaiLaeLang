using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance; // เพิ่ม Singleton เพื่อให้คนอื่นเรียกใช้ได้ง่าย

    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float laneDistance = 4f;
    public float laneSwitchSpeed = 10f;
    public bool isGameActive = false;

    [Header("Swarm Settings (New)")]
    public ZombieData defaultZombieData; // [ใหม่] ข้อมูลซอมบี้ตัวเริ่มต้น
    public Transform zombieContainer;
    public int startingZombies = 1;
    public float formationRadius = 1.5f;
    public List<GameObject> zombies = new List<GameObject>();

    [Header("Power Ups")]
    public bool isX2Active = false;
    public bool isInvincible = false;
    public float powerUpDuration = 5f;

    [Header("Start Settings")]
    public int startLane = 1;

    [Header("Jump Settings")]
    public float jumpHeight = 2.0f;
    public float gravityValue = -20f;

    [Header("Slide Settings")]
    public float slideDuration = 1.0f;
    public float slideHeight = 0.5f;
    public float slideCenterY = 0.25f;

    // State Variables
    private int currentLane = 1;
    private Vector3 verticalVelocity;
    private bool isSliding = false;
    private float originalHeight;
    private Vector3 originalCenter;

    // References
    private CharacterController controller;
    private GameControls controls;

    void Awake()
    {
        Instance = this;
        controller = GetComponent<CharacterController>();
        controls = new GameControls();
        originalHeight = controller.height;
        originalCenter = controller.center;

        if (zombieContainer == null)
        {
            GameObject container = new GameObject("ZombieContainer");
            container.transform.parent = this.transform;
            container.transform.localPosition = Vector3.zero;
            zombieContainer = container.transform;
        }
    }

    void Start()
    {
        ResetPlayerPosition();
    }

    public void ResetPlayerPosition()
    {
        currentLane = startLane;
        float startX = (currentLane - 1) * laneDistance;
        Vector3 startPos = new Vector3(startX, 1, 0);

        controller.enabled = false;
        transform.position = startPos;
        controller.enabled = true;

        ClearSwarm();

        // [แก้] ส่ง defaultZombieData เข้าไป
        if (defaultZombieData != null)
        {
            for (int i = 0; i < startingZombies; i++)
            {
                AddZombie(defaultZombieData);
            }
        }
    }

    void OnEnable()
    {
        controls.Gameplay.Enable();
        controls.Gameplay.MoveLane.performed += OnMoveLanePerformed;
        controls.Gameplay.Jump.performed += OnJumpPerformed;
        controls.Gameplay.Slide.performed += OnSlidePerformed;
    }

    void OnDisable()
    {
        controls.Gameplay.MoveLane.performed -= OnMoveLanePerformed;
        controls.Gameplay.Jump.performed -= OnJumpPerformed;
        controls.Gameplay.Slide.performed -= OnSlidePerformed;
        controls.Gameplay.Disable();
    }

    private void OnMoveLanePerformed(InputAction.CallbackContext context)
    {
        if (!isGameActive) return;
        Vector2 input = context.ReadValue<Vector2>();
        if (input.x > 0) ChangeLane(1);
        else if (input.x < 0) ChangeLane(-1);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!isGameActive) return;
        if (controller.isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }
    }

    private void OnSlidePerformed(InputAction.CallbackContext context)
    {
        if (!isGameActive) return;
        if (controller.isGrounded && !isSliding)
        {
            StartCoroutine(SlideRoutine());
        }
    }

    void Update()
    {
        if (!isGameActive) return;

        // Gravity
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        verticalVelocity.y += gravityValue * Time.deltaTime;

        // Lane Movement
        float targetX = (currentLane - 1) * laneDistance;
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, laneSwitchSpeed * Time.deltaTime);
        float xMove = newPos.x - transform.position.x;

        // Apply Move
        Vector3 moveVector = new Vector3(xMove, verticalVelocity.y * Time.deltaTime, forwardSpeed * Time.deltaTime);
        controller.Move(moveVector);
    }

    // --- SWARM SYSTEM ---

    public void AddZombie(ZombieData typeToSpawn)
    {
        if (typeToSpawn == null) return;

        int amountToAdd = isX2Active ? 2 : 1;

        for (int i = 0; i < amountToAdd; i++)
        {
            GameObject newZom = Instantiate(typeToSpawn.prefab, zombieContainer);
            ZombieController zCtrl = newZom.GetComponent<ZombieController>();

            // สร้าง Anchor
            GameObject anchor = new GameObject("Z_Anchor");
            anchor.transform.parent = this.transform;

            // สุ่มตำแหน่งรอบๆ
            Vector3 pos = Random.insideUnitSphere * formationRadius;
            pos.y = 0;
            anchor.transform.localPosition = pos;

            // สั่ง Initialize (ส่ง Anchor ให้ซอมบี้ถือไว้)
            if (zCtrl != null)
            {
                zCtrl.Initialize(typeToSpawn, anchor.transform, this);
            }

            zombies.Add(newZom);
        }
    }

    public void RemoveZombie()
    {
        // เช็คอมตะ
        if (isInvincible) return;

        if (zombies.Count > 0)
        {
            // เอาตัวสุดท้ายออก
            int lastIndex = zombies.Count - 1;
            GameObject deadZom = zombies[lastIndex];
            zombies.RemoveAt(lastIndex);

            // [เพิ่ม] ลบ Anchor ทิ้งด้วยเพื่อกัน Memory Leak
            ZombieController zCtrl = deadZom.GetComponent<ZombieController>();
            // เราต้องแน่ใจว่า ZombieController มีตัวแปรเก็บ anchor ไว้ (ถ้าไม่มีให้แก้ ZombieController.cs เพิ่ม public Transform myAnchor;)
            // แต่ถ้าใน ZombieController ไม่มี field public ให้เข้าถึง anchor เราอาจต้องปล่อยไปก่อน
            // วิธีแก้เบื้องต้น: ทำลายตัวซอมบี้ไปเลย

            deadZom.transform.parent = null;
            Destroy(deadZom); // ทำลายทันทีหรือหน่วงเวลาก็ได้

            // เช็ค Game Over
            if (zombies.Count <= 0)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
        else
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    void ClearSwarm()
    {
        foreach (GameObject z in zombies)
        {
            if (z != null) Destroy(z);
        }
        zombies.Clear();

        // [เสริม] ลบ Anchor ที่ตกค้างในลูกของ Player ทั้งหมด (ถ้าต้องการคลีนจริงๆ)
        // foreach (Transform child in transform) { if (child.name == "Z_Anchor") Destroy(child.gameObject); }
    }

    // --- INTERACTION ---

    private void OnTriggerEnter(Collider other)
    {
        if (!isGameActive) return;

        // 1. ชนคน
        if (other.CompareTag("Human"))
        {
            HumanController human = other.GetComponent<HumanController>();
            ZombieData typeToSpawn = defaultZombieData;

            // ถ้าคนมี Data ว่าดรอปซอมบี้แบบไหนให้ใช้แบบนั้น (ถ้ามี)
            // if (human != null && human.data.dropZombieType != null) typeToSpawn = human.data.dropZombieType;

            AddZombie(typeToSpawn);
            Destroy(other.gameObject);
        }
        // 2. ชนสิ่งกีดขวาง
        else if (other.CompareTag("Obstacle"))
        {
            if (isInvincible)
            {
                Debug.Log("Invincible! Obstacle smashed.");
                Destroy(other.gameObject); // ชนพังเลย
            }
            else
            {
                RemoveZombie();
                // Destroy(other.gameObject); // สิ่งกีดขวางมักไม่หาย
            }
        }
        // 3. ชนไอเทม (ต้องสร้างสคริปต์ ItemPickup หรือใช้ Tag เช็คชื่อ)
        else if (other.CompareTag("Item"))
        {
            // สมมติว่ามีสคริปต์ ItemPickup ติดที่เหรียญ/กล่อง
            ItemPickup item = other.GetComponent<ItemPickup>();
            if (item != null)
            {
                CollectItem(item.itemType); // ส่ง string เช่น "x2", "Invincible"
            }

            Destroy(other.gameObject);
        }
    }

    // --- POWER UPS ---

    public void CollectItem(string itemType)
    {
        StartCoroutine(PowerUpRoutine(itemType));
    }

    IEnumerator PowerUpRoutine(string itemType)
    {
        Debug.Log("PowerUp Activated: " + itemType);

        if (itemType == "x2") isX2Active = true;
        else if (itemType == "Invincible") isInvincible = true;
        else if (itemType == "Speed") forwardSpeed += 5f;

        yield return new WaitForSeconds(powerUpDuration);

        // หมดเวลา - คืนค่า
        if (itemType == "x2") isX2Active = false;
        else if (itemType == "Invincible") isInvincible = false;
        else if (itemType == "Speed") forwardSpeed -= 5f;

        Debug.Log("PowerUp Ended: " + itemType);
    }

    // --- UTILS ---

    private void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;
        if (targetLane >= 0 && targetLane <= 2) currentLane = targetLane;
    }

    IEnumerator SlideRoutine()
    {
        isSliding = true;
        controller.height = slideHeight;
        controller.center = new Vector3(0, slideCenterY, 0);
        yield return new WaitForSeconds(slideDuration);
        controller.height = originalHeight;
        controller.center = originalCenter;
        isSliding = false;
    }
}