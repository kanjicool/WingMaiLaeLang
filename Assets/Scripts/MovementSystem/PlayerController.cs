using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float forwardSpeed = 10f;
    public float laneDistance = 4f;
    public float laneSwitchSpeed = 10f;
    public bool isGameActive = false;

    [Header("Start Settings")]
    public int startLane = 1; // 0=Left, 1=Center, 2=Right

    [Header("Jump Settings")]
    public float jumpHeight = 2.0f;
    public float gravityValue = -20f;   // ·√ß¥÷ß¥Ÿ¥ (§Ë“ª°µ‘ª√–¡“≥ -9.81)

    [Header("Slide Settings")]
    public float slideDuration = 1.0f;
    public float slideHeight = 0.5f; // §«“¡ ŸßµÕπ ‰≈¥Ï (ª°µ‘ Player  Ÿß 2)
    public float slideCenterY = 0.25f; // ®ÿ¥°÷Ëß°≈“ßµÕπ ‰≈¥Ï

    // State Variables
    private int currentLane = 1; // 0=Left, 1=Center, 2=Right
    private Vector3 verticalVelocity;
    private bool isSliding = false;
    private float originalHeight;
    private Vector3 originalCenter;


    // References
    private CharacterController controller;
    private GameControls controls;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new GameControls();

        originalHeight = controller.height;
        originalCenter = controller.center;

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
        controls.Gameplay.MoveLane.performed -= OnMoveLanePerformed; // ¬°‡≈‘°°“√ºŸ°‡æ◊ËÕ°—π Memory Leak
        controls.Gameplay.Jump.performed -= OnJumpPerformed;
        controls.Gameplay.Slide.performed -= OnSlidePerformed;
        controls.Gameplay.Disable();
    }


    private void OnMoveLanePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (input.x > 0) // °¥¢«“
        {
            ChangeLane(1);
        }
        else if (input.x < 0) // °¥´È“¬
        {
            ChangeLane(-1);
        }
    }


    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (controller.isGrounded)
        {
            //  Ÿµ√ø‘ ‘° Ï: v = sqrt(h * -2 * g)
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);

            // anim.SetTrigger("Jump");
        }
    }

    private void OnSlidePerformed(InputAction.CallbackContext context)
    {
        // ∂È“Õ¬ŸË∫πæ◊Èπ·≈–¬—ß‰¡Ë ‰≈¥Ï „ÀÈ‡√‘Ë¡ ‰≈¥Ï
        if (controller.isGrounded && !isSliding)
        {
            StartCoroutine(SlideRoutine());
        }
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

        // anim.SetBool("IsSliding", true);
        
        yield return new WaitForSeconds(slideDuration);

        controller.height = originalHeight;
        controller.center = originalCenter;

        // anim.SetBool("IsSliding", false);

        isSliding = false;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        currentLane = startLane;

        float startX = (currentLane - 1) * laneDistance;
        Vector3 startPos = transform.position;
        startPos.x = startX;
        transform.position = startPos;

    }

    void Update()
    {
        if (!isGameActive) return;

        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; 
        }

        verticalVelocity.y += gravityValue * Time.deltaTime;

        float targetX = (currentLane - 1) * laneDistance;
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, laneSwitchSpeed * Time.deltaTime);
        float xMove = newPos.x - transform.position.x;

        Vector3 moveVector = new Vector3(xMove, verticalVelocity.y * Time.deltaTime, forwardSpeed * Time.deltaTime);
        controller.Move(moveVector);
    }


}
