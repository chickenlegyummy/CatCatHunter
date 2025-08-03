using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 10f;

    private bool isAttacking = false; // Track if the player is currently attacking

    private float attackCooldown = 0.85f; // Cooldown time for attacks
    public float attackEffectDuration = 0.6f; // Duration for attack effect (can be adjusted in inspector)

    [Header("Components")]
    private Rigidbody rb;
    public Animator animator;
    private SpriteRenderer spriteRenderer;
    public Transform visualContainer; // Container for sprite and light that will be rotated

    public GameObject light;
    public GameObject attackEffect; // Attack effect sprite object
    private AttackDamage attackDamageComponent; // Reference to attack damage component

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

        // Make sure attack effect is initially disabled
        if (attackEffect != null)
        {
            attackEffect.SetActive(false);
            
            // Get the AttackDamage component from the attack effect
            attackDamageComponent = attackEffect.GetComponent<AttackDamage>();
            if (attackDamageComponent == null)
            {
                Debug.LogWarning("Attack effect doesn't have an AttackDamage component! Adding one automatically.");
                attackDamageComponent = attackEffect.AddComponent<AttackDamage>();
            }
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

        if (attackCooldown > 0f)
        {
            attackCooldown -= Time.deltaTime; // Reduce cooldown over time
        }
    }

    void HandleInput()
    {
        // Get input from WASD or Arrow Keys
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        inputVector = new Vector2(horizontal, vertical).normalized;

        // One key press = one attack, with cooldown prevention
        if (Input.GetButtonDown("Fire1") && attackCooldown <= 0f)
        {
            isAttacking = true;
            Attack();
            attackCooldown = 0.85f;
            // Start coroutine to end attack after animation
            StartCoroutine(WaitForAttackEnd());
            // Start separate coroutine to hide attack effect after its duration
            StartCoroutine(HideAttackEffectAfterDuration());
        }
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

    void Attack()
    {
        if (animator != null)
        {
            animator.SetBool("Attack", true);
        }
        
        // Show attack effect and activate damage
        if (attackEffect != null)
        {
            attackEffect.SetActive(true);
            
            // Activate the attack damage
            if (attackDamageComponent != null)
            {
                attackDamageComponent.ActivateAttack();
            }
        }
        
        Debug.Log("Player attack activated!");
    }

    void AttackEnd()
    {
        isAttacking = false; // Reset attack state
        if (animator != null)
        {
            animator.SetBool("Attack", false);
        }
        
        // Note: Attack effect is handled separately and may still be visible
    }

    IEnumerator WaitForAttackEnd()
    {
        // Wait for attack animation duration (shorter than cooldown)
        yield return new WaitForSeconds(0.2f); // Adjust this to match attack animation length
        AttackEnd();
    }

    IEnumerator HideAttackEffectAfterDuration()
    {
        // Wait for the full attack effect duration
        yield return new WaitForSeconds(attackEffectDuration);
        
        // Hide attack effect
        if (attackEffect != null)
        {
            attackEffect.SetActive(false);
        }
    }
}
