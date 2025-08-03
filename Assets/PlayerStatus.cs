using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : ObjectStatus
{
    [Header("Player Specific")]
    public int lives = 3; // Number of lives the player has
    public float respawnTime = 2f; // Time before respawning
    
    private Vector3 spawnPoint; // Where to respawn the player
    
    void Start()
    {
        // Store spawn point
        spawnPoint = transform.position;
        
        // Subscribe to death event for player-specific behavior
        OnDeath += HandlePlayerDeath;
        
        Debug.Log("Player Status initialized. Health: " + health + "/" + maxHealth);
    }
    
    void HandlePlayerDeath()
    {
        lives--;
        Debug.Log($"Player died! Lives remaining: {lives}");
        
        if (lives > 0)
        {
            // Respawn the player
            StartCoroutine(RespawnPlayer());
        }
        else
        {
            // Game Over
            GameOver();
        }
    }
    
    IEnumerator RespawnPlayer()
    {
        // Disable player controls during respawn
        Movement movement = GetComponent<Movement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
        
        yield return new WaitForSeconds(respawnTime);
        
        // Reset player state
        health = maxHealth;
        isAlive = true;
        isHitten = false;
        
        // Reset position
        transform.position = spawnPoint;
        
        // Reset velocity
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
        
        // Re-enable controls
        if (movement != null)
        {
            movement.enabled = true;
        }
        
        Debug.Log("Player respawned!");
    }
    
    void GameOver()
    {
        Debug.Log("GAME OVER!");
        
        // Disable player controls
        Movement movement = GetComponent<Movement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
        
        // You can add game over UI, restart logic, etc. here
    }
    
    // Override the Die method to prevent automatic destruction
    protected override void HandleDeath()
    {
        // Don't destroy the player object, just trigger death event
        isAlive = false;
        OnDeath?.Invoke();
    }
}
