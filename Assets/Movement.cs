using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    
    [Header("Components")]
    private Rigidbody rb;
    public Animator animator;
    private SpriteRenderer spriteRenderer;
    public Transform visualContainer; // Container for sprite and light that will be rotated

    public GameObject light;
    
    [Header("Input")]
    private Vector2 inputVector;
    private Vector3 currentVelocity;
    private bool facingRight = true;

    private float OriginalY; // Store original Y position to reset later

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        OriginalY = transform.position.y;
        // Ensure we have a Rigidbody component
        if (rb == null)
        {
            Debug.LogError("Movement script requires a Rigidbody component!");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleAnimation();
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0f, 0f);
        transform.position = new Vector3(transform.position.x, OriginalY, transform.position.z);
    }
    
    void HandleInput()
    {
        // Get input from WASD or Arrow Keys
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        inputVector = new Vector2(horizontal, vertical).normalized;
    }
    
    void HandleMovement()
    {
        if (inputVector != Vector2.zero)
        {
            // Create target velocity using X and Z axes (Y remains unchanged)
            Vector3 targetVelocity = new Vector3(inputVector.x * moveSpeed, rb.velocity.y, inputVector.y * moveSpeed);
            
            // Accelerate towards target velocity
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            // Decelerate to zero on X and Z, preserve Y velocity
            Vector3 targetVelocity = new Vector3(0, rb.velocity.y, 0);
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, deceleration * Time.deltaTime);
        }
        
        // Apply movement to Rigidbody
        rb.velocity = currentVelocity;
    }
    
    void HandleAnimation()
    {
        if (animator != null)
        {
            // Set speed parameter for walk animation (any movement triggers walking)
            bool isMoving = inputVector != Vector2.zero;
            
            // Handle sprite flipping for left/right movement
            if (inputVector.x < 0 && facingRight) // Moving left and currently facing right
            {
                Flip();
                animator.SetFloat("Horizontal", 1f);
            }
            else if (inputVector.x > 0 && !facingRight) // Moving right and currently facing left
            {
                Flip();
                animator.SetFloat("Horizontal", 1f);
            }
            else if (inputVector.x < 0) // Moving left (already facing left)
            {
                animator.SetFloat("Horizontal", 1f);
            }
            else if (inputVector.x > 0) // Moving right (already facing right)
            {
                animator.SetFloat("Horizontal", 1f);
            }
            else if (isMoving) // Moving up/down only (W/S keys)
            {
                // Keep current horizontal direction for up/down movement
                animator.SetFloat("Horizontal", 1f);
            }
            else
            {
                // Not moving, set horizontal to 0 but keep last direction
                animator.SetFloat("Horizontal", 0f);
            }
        }
    }
    
    void Flip()
    {
        // Switch the direction flag
        facingRight = !facingRight;

        // Use scale flipping instead of rotation to avoid camera issues
        if (visualContainer != null)
        {
            // Flip the visual container's X scale
            Vector3 scale = visualContainer.localScale;
            scale.x *= -1f;
            visualContainer.localScale = scale;
        }
        else
        {
            // Fallback: flip the entire character's scale if no visual container is assigned
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }
}
