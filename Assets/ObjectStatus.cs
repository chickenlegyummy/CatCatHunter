using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectStatus : MonoBehaviour
{
    [Header("Status Settings")]
    public bool isAlive = true; // Track if the object is alive
    public float maxHealth = 100f; // Maximum health of the object
    public float health = 100f; // Current health of the object
    public bool isHitten = false; // Track if the object is currently hit
    public float hitCooldown = 1f; // Cooldown time after being hit
    
    [Header("Visual Feedback")]
    public Color hitColor = Color.red; // Color when hit
    public float hitFlashDuration = 0.2f; // How long the hit flash lasts
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float hitCooldownTimer = 0f;
    
    // Events for other scripts to listen to
    public System.Action<float> OnHealthChanged; // Triggered when health changes
    public System.Action OnDeath; // Triggered when object dies
    public System.Action OnHit; // Triggered when object is hit
    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Initialize health to max health if not set
        if (health <= 0)
        {
            health = maxHealth;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Handle hit cooldown
        if (hitCooldownTimer > 0f)
        {
            hitCooldownTimer -= Time.deltaTime;
            if (hitCooldownTimer <= 0f)
            {
                isHitten = false;
                // Reset color back to original
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }
    }
    
    // Method to take damage
    public void TakeDamage(float damage)
    {
        // Don't take damage if already dead or currently in hit cooldown
        if (!isAlive || isHitten)
            return;
            
        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);
        
        // Trigger hit state
        isHitten = true;
        hitCooldownTimer = hitCooldown;
        
        // Visual feedback - flash red
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
        }
        
        // Trigger events
        OnHealthChanged?.Invoke(health);
        OnHit?.Invoke();
        
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {health}/{maxHealth}");
        
        // Check if dead
        if (health <= 0f && isAlive)
        {
            Die();
        }
    }
    
    // Method to heal
    public void Heal(float healAmount)
    {
        if (!isAlive)
            return;
            
        health += healAmount;
        health = Mathf.Clamp(health, 0f, maxHealth);
        
        OnHealthChanged?.Invoke(health);
        
        Debug.Log($"{gameObject.name} healed {healAmount}. Health: {health}/{maxHealth}");
    }
    
    // Method to handle death
    private void Die()
    {
        isAlive = false;
        OnDeath?.Invoke();
        
        Debug.Log($"{gameObject.name} has died!");
        
        // Call virtual method for custom death handling
        HandleDeath();
    }
    
    // Virtual method that can be overridden in derived classes
    protected virtual void HandleDeath()
    {
        // Default behavior: destroy after delay
        StartCoroutine(DeathSequence());
    }
    
    private IEnumerator DeathSequence()
    {
        // Wait a bit before destroying (allows for death animation/effects)
        yield return new WaitForSeconds(1f);
        
        // Destroy the game object
        Destroy(gameObject);
    }
    
    // Method to check if can take damage
    public bool CanTakeDamage()
    {
        return isAlive && !isHitten;
    }
    
    // Method to get health percentage
    public float GetHealthPercentage()
    {
        return health / maxHealth;
    }
}
