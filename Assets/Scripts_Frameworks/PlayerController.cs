using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private GameUI gameUI;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool faceMoveDirection;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 1.8f;
    [SerializeField] private float jumpForce = 5f;

    private Animator animator;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isRunning;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        gameUI = FindAnyObjectByType<GameUI>();
    }

    void Update()
    {
        UpdateAnimator();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Hold Shift = run, release = walk. Only updates the flag;
    // the animator is updated in UpdateAnimator().
    public void Run(InputAction.CallbackContext context)
    {
        if (context.performed)
            isRunning = true;
        else if (context.canceled)
            isRunning = false;
    }

    public void Attack(InputAction.CallbackContext context)
    {
        // Without this check the trigger fires on started, performed and canceled
        if (context.performed)
            animator.SetTrigger("Attack");
    }

 

   

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    void FixedUpdate()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement = forward * moveInput.y + right * moveInput.x;

        // Running actually moves the character faster now
        float currentMoveSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = movement.x * currentMoveSpeed;
        velocity.z = movement.z * currentMoveSpeed;

        rb.linearVelocity = velocity;

        if (faceMoveDirection && movement.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
    }

    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // 0 when idle, up to 1 when moving
        float inputMagnitude = new Vector3(moveInput.x, 0f, moveInput.y).magnitude;

        animator.SetFloat("Speed", inputMagnitude);

        // Only "running" if Shift is held AND the player is actually moving
        animator.SetBool("Run", isRunning && inputMagnitude > 0.01f);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Coin coin = collision.gameObject.GetComponent<Coin>();

            if (coin != null)
            {
                gameUI.AddScore(coin.GetScoreValue());
                Destroy(coin.gameObject);
            }
        }
    }

}