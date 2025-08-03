using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    
    [Header("AI Settings")]
    public bool chasePlayer = true;
    public bool attackPlayer = true;
    
    private Transform player;
    private ObjectStatus objectStatus;
    private Rigidbody rb;
    private Animator animator;
    private float lastAttackTime = 0f;
    private bool facingRight = true;
    
    // States
    private enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }
    
    private EnemyState currentState = EnemyState.Idle;
    
    void Start()
    {
        // Get components
        objectStatus = GetComponent<ObjectStatus>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        // Find the player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        
        // Subscribe to death event
        if (objectStatus != null)
        {
            objectStatus.OnDeath += OnEnemyDeath;
        }
    }
    
    void Update()
    {
        if (objectStatus != null && !objectStatus.isAlive)
        {
            currentState = EnemyState.Dead;
            return;
        }
        
        if (player == null)
            return;
            
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case EnemyState.Idle:
                if (chasePlayer && distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chasing;
                }
                break;
                
            case EnemyState.Chasing:
                if (distanceToPlayer <= attackRange && attackPlayer)
                {
                    currentState = EnemyState.Attacking;
                }
                else if (distanceToPlayer > detectionRange)
                {
                    currentState = EnemyState.Idle;
                }
                else
                {
                    ChasePlayer();
                }
                break;
                
            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange)
                {
                    currentState = EnemyState.Chasing;
                }
                else
                {
                    AttackPlayer();
                }
                break;
        }
        
        // Update animation
        UpdateAnimation();
    }
    
    void ChasePlayer()
    {
        if (player == null || rb == null)
            return;
            
        Vector3 direction = (player.position - transform.position).normalized;
        
        // Move towards player
        Vector3 moveVelocity = new Vector3(direction.x * moveSpeed, rb.velocity.y, direction.z * moveSpeed);
        rb.velocity = moveVelocity;
        
        // Handle facing direction
        if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && facingRight)
        {
            Flip();
        }
    }
    
    void AttackPlayer()
    {
        // Check attack cooldown
        if (Time.time < lastAttackTime + attackCooldown)
            return;
            
        // Stop movement during attack
        if (rb != null)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
        
        // Try to damage player
        PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            // In a more complex system, you'd have an attack animation and collision detection
            // For now, we'll just deal damage directly if close enough
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                // You might want to use the ObjectStatus instead if PlayerStatus inherits from it
                // playerStatus.TakeDamage(attackDamage);
                Debug.Log($"Enemy attacked player for {attackDamage} damage!");
            }
        }
        
        lastAttackTime = Time.time;
        
        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }
    
    void UpdateAnimation()
    {
        if (animator == null)
            return;
            
        // Set movement animation
        bool isMoving = currentState == EnemyState.Chasing;
        animator.SetBool("IsMoving", isMoving);
        
        // Set state animations
        animator.SetBool("IsAttacking", currentState == EnemyState.Attacking);
    }
    
    void OnEnemyDeath()
    {
        Debug.Log($"Enemy {gameObject.name} has died!");
        
        // Stop all movement
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
        
        // Disable AI
        currentState = EnemyState.Dead;
        this.enabled = false;
        
        // You can add death effects, drop items, etc. here
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (objectStatus != null)
        {
            objectStatus.OnDeath -= OnEnemyDeath;
        }
    }
}
