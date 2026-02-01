using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Animation")]
    public Animator playerAnimator;

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

    // State Variables
    private int currentLane = 1;
    private Vector3 verticalVelocity;
    private bool isSliding = false;
    private float originalHeight;
    private Vector3 originalCenter;

    private float currentVelocityX;

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

        if (playerAnimator == null) playerAnimator = GetComponentInChildren<Animator>();

    }
     
    void Start()
    {
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
        if (controller.isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            if (playerAnimator) playerAnimator.SetTrigger("Jump");
            if (SwarmManager.Instance != null ) SwarmManager.Instance.HordeJump();
        }
    }

    private void OnSlidePerformed(InputAction.CallbackContext context)
    {
        if (!isGameActive) return;
        if (controller.isGrounded && !isSliding)
        {
            StartCoroutine(SlideRoutine());
            if (playerAnimator) playerAnimator.SetTrigger("Slide");
            if (SwarmManager.Instance != null) SwarmManager.Instance.HordeSlide();
        }
    }

    void Update()
    {
        if (playerAnimator)
        {
            playerAnimator.SetBool("IsRunning", isGameActive);
        }

        if (!isGameActive) return;

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
        if (itemType == "x2") isX2Active = true;
        else if (itemType == "Invincible") isInvincible = true;
        else if (itemType == "Speed") forwardSpeed += 5f;

        yield return new WaitForSeconds(powerUpDuration);

        if (itemType == "x2") isX2Active = false;
        else if (itemType == "Invincible") isInvincible = false;
        else if (itemType == "Speed") forwardSpeed -= 5f;
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
}