using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Animation")]
    public Animator playerAnimator;
    
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float energyDepletionRate = 1f;

    [Header("Movement Settings")]
    public float forwardSpeed = 10f;

    public float laneChangeSmoothTime = 0.15f;
    public float maxLaneSpeed = 20f;
    public float rotationSpeed = 10f;

    public float laneSwitchSpeed = 10f;

    [Header("Power Ups")]
    public bool isX2Active = false;
    public bool isInvincible = false;
    public float powerUpDuration = 5f;
    public bool isSpeedBoosted = false;
    public bool isSlowDrainActive = false;

    [Header("Start Settings")]
    public int startLane = 1;
    public int startingZombies = 1;
    public bool isGameActive = false;


    [Header("Jump Settings")]
    public float jumpHeight = 2.0f;
    public float gravityValue = -20f;

    [Header("Slide Settings")]
    public float slideDuration = 0.5f;
    public float slideHeight = 0.5f;
    public float slideCenterY = 0.25f;
    public float slideCooldown = 1.0f;

    // State Variables
    private int currentLane = 1;
    private Vector3 verticalVelocity;
    private bool isSliding = false;
    private float originalHeight;
    private Vector3 originalCenter;

    private float nextSlideTime = 0f;
    private Coroutine currentSlideRoutine;

    private float currentVelocityX;

    // References
    private CharacterController controller;
    private GameControls controls;

    public bool isX2Warning, isInvinWarning, isSpeedWarning, isSlowWarning;

    void Awake()
    {
        Instance = this;
        controller = GetComponent<CharacterController>();
        controls = new GameControls();
        originalHeight = controller.height;
        originalCenter = controller.center;

        if (playerAnimator == null) playerAnimator = GetComponentInChildren<Animator>();

    }
     
    void Start()
    {
        currentEnergy = maxEnergy;
        ResetPlayerPosition();
    }

    public void ResetPlayerPosition()
    {
        currentLane = startLane;
        float laneDistance = GameManager.Instance.laneDistance;
        float startX = (currentLane - 1) * laneDistance;
        Vector3 startPos = new Vector3(startX, 0, 0);

        controller.enabled = false;
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
        controller.enabled = true;

        if (playerAnimator)
        {
            playerAnimator.Rebind();
            playerAnimator.SetBool("IsRunning", false);
        }


        if (SwarmManager.Instance != null)
        {
            SwarmManager.Instance.ResetSwarm();

            for (int i = 0; i < startingZombies; i++)
            {
                SwarmManager.Instance.AddZombie();
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
        if (controller.isGrounded || isSliding)
        {
            if (isSliding)
            {
                StopSlideImmediate();
            }

            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);

            if (playerAnimator) playerAnimator.SetTrigger("Jump");
            if (SwarmManager.Instance != null) SwarmManager.Instance.HordeJump();
        }
    }

    private void OnSlidePerformed(InputAction.CallbackContext context)
    {
        if (!isGameActive) return;
        if (controller.isGrounded && !isSliding && Time.time >= nextSlideTime)
        {
            currentSlideRoutine = StartCoroutine(SlideRoutine());

            if (playerAnimator) playerAnimator.SetTrigger("Slide");
            if (SwarmManager.Instance != null) SwarmManager.Instance.HordeSlide();

            nextSlideTime = Time.time + slideCooldown;
        }
    }

    void StopSlideImmediate()
    {
        if (currentSlideRoutine != null) StopCoroutine(currentSlideRoutine);

        controller.height = originalHeight;
        controller.center = originalCenter;

        isSliding = false;

        if (playerAnimator) playerAnimator.ResetTrigger("Slide");
    }

    void Update()
    {
        if (playerAnimator)
        {
            playerAnimator.SetBool("IsRunning", isGameActive);
        }

        if (!isGameActive) return;

        DepleteEnergy();

        // Gravity
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        verticalVelocity.y += gravityValue * Time.deltaTime;

        // Lane Movement
        float laneDistance = GameManager.Instance.laneDistance;
        float targetX = (currentLane - 1) * laneDistance;

        float nextX = Mathf.SmoothDamp(transform.position.x, targetX, ref currentVelocityX, laneChangeSmoothTime, maxLaneSpeed);
        float xMoveDelta = nextX - transform.position.x;
        Vector3 finalMoveVector = new Vector3(xMoveDelta, verticalVelocity.y * Time.deltaTime, forwardSpeed * Time.deltaTime);
        controller.Move(finalMoveVector);

        Vector3 directionToLook = new Vector3(currentVelocityX, 0, forwardSpeed);
        if (directionToLook != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, rotationSpeed * Time.deltaTime);
        }

    }



    private void OnTriggerEnter(Collider other)
    {
        if (!isGameActive) return;

        if (other.CompareTag("Human"))
        {
            SwarmManager.Instance.AddZombie();
            HumanController human = other.GetComponent<HumanController>();

            float energyRecoveryAmount = 10f; 
            AddEnergy(energyRecoveryAmount);

            if (human == null) human = other.GetComponentInParent<HumanController>();

            if (human != null)
            {
                human.OnEaten();
            }
            else
            {
                Destroy(other.gameObject);
            }

        }
        else if (other.CompareTag("Obstacle"))
        {
            if (isInvincible)
            {
                Destroy(other.gameObject);
            }
            else
            {
                SwarmManager.Instance.RemoveZombie();
            }
        }
        else if (other.CompareTag("Item"))
        {

            Destroy(other.gameObject);
        }
    }

    public void CollectItem(string itemType)
    {
        StartCoroutine(PowerUpRoutine(itemType));
    }

    IEnumerator PowerUpRoutine(string itemType)
    {
        float warnTime = 1.5f; // จะเริ่มกระพริบก่อนหมด 1.5 วินาที

        // --- เริ่มต้นใช้งาน Buff ---
        if (itemType == "x2") isX2Active = true;
        else if (itemType == "Invincible") isInvincible = true;
        else if (itemType == "Speed") { forwardSpeed += 5f; isSpeedBoosted = true; }
        else if (itemType == "SlowDrain") { energyDepletionRate -= 0.5f; isSlowDrainActive = true; }

        // รอจนถึงช่วงใกล้หมด
        yield return new WaitForSeconds(powerUpDuration - warnTime);

        // --- เริ่มสถานะ Warning ---
        if (itemType == "x2") isX2Warning = true;
        else if (itemType == "Invincible") isInvinWarning = true;
        else if (itemType == "Speed") isSpeedWarning = true;
        else if (itemType == "SlowDrain") isSlowWarning = true;

        yield return new WaitForSeconds(warnTime);

        // --- ปิด Buff และ Warning ---
        if (itemType == "x2") { isX2Active = false; isX2Warning = false; }
        else if (itemType == "Invincible") { isInvincible = false; isInvinWarning = false; }
        else if (itemType == "Speed") { forwardSpeed -= 5f; isSpeedBoosted = false; isSpeedWarning = false; }
        else if (itemType == "SlowDrain") { energyDepletionRate += 0.5f; isSlowDrainActive = false; isSlowWarning = false; }
    }

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

    private void DepleteEnergy()
    {
        if (currentEnergy > 0)
        {
            currentEnergy -= energyDepletionRate * Time.deltaTime;
        }
        else
        {
            currentEnergy = 0;
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameActive = false;
        Debug.Log("Energy Depleted! Game Over");
        // ใส่ Logic จบเกมของคุณตรงนี้ เช่น แสดง Pop-up หรือหยุดการเคลื่อนที่
    }

    public void AddEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
    }

}