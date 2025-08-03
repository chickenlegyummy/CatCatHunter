using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDamage : MonoBehaviour
{
    [Header("Attack Settings")]
    public float damage = 25f; // Damage dealt by this attack
    public LayerMask enemyLayers = -1; // Which layers are considered enemies
    public bool attackOnce = true; // If true, each enemy can only be hit once per attack
    
    [Header("Attack Properties")]
    public bool isPlayerAttack = true; // Is this a player attack or enemy attack?
    public string targetTag = "Enemy"; // Tag of objects that can be damaged
    
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>(); // Track hit objects for this attack
    private bool isAttackActive = false;
    
    void Start()
    {
        // If this is an attack effect, it should start inactive
        if (isPlayerAttack)
        {
            gameObject.SetActive(false);
        }
    }
    
    void OnEnable()
    {
        // Reset hit tracking when attack becomes active
        hitObjects.Clear();
        isAttackActive = true;
        
        // Auto-disable after a short time if this is a temporary attack effect
        if (isPlayerAttack)
        {
            StartCoroutine(DisableAfterTime());
        }
    }
    
    void OnDisable()
    {
        isAttackActive = false;
        hitObjects.Clear();
    }
    
    // Handle collision with 2D colliders
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }
    
    // Handle collision with 3D colliders
    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    private void HandleCollision(GameObject hitObject)
    {
        if (!isAttackActive)
            return;
            
        // Check if we've already hit this object (if attackOnce is true)
        if (attackOnce && hitObjects.Contains(hitObject))
            return;
            
        // Check if the object has the correct tag
        if (!string.IsNullOrEmpty(targetTag) && !hitObject.CompareTag(targetTag))
            return;
            
        // Check layer mask
        if (enemyLayers != -1 && (enemyLayers.value & (1 << hitObject.layer)) == 0)
            return;
            
        // Try to get ObjectStatus component
        ObjectStatus objectStatus = hitObject.GetComponent<ObjectStatus>();
        if (objectStatus != null && objectStatus.CanTakeDamage())
        {
            // Deal damage
            objectStatus.TakeDamage(damage);
            
            // Add to hit objects if attackOnce is true
            if (attackOnce)
            {
                hitObjects.Add(hitObject);
            }
            
            // Add knockback or other effects here if desired
            ApplyKnockback(hitObject);
            
            Debug.Log($"Attack hit {hitObject.name} for {damage} damage!");
        }
    }
    
    private void ApplyKnockback(GameObject hitObject)
    {
        // Optional: Apply knockback force
        Rigidbody rb = hitObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 knockbackDirection = (hitObject.transform.position - transform.position).normalized;
            float knockbackForce = 5f; // Adjust as needed
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
    }
    
    private IEnumerator DisableAfterTime()
    {
        // Wait for attack duration, then disable
        yield return new WaitForSeconds(0.6f); // Should match attack effect duration
        
        if (gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }
    
    // Public method to activate attack (call this when attack starts)
    public void ActivateAttack()
    {
        hitObjects.Clear();
        isAttackActive = true;
        gameObject.SetActive(true);
    }
    
    // Public method to deactivate attack
    public void DeactivateAttack()
    {
        isAttackActive = false;
        gameObject.SetActive(false);
    }
}
