using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public bool isAlive = true; // Track if the object is alive
    public float health = 100f; // Health of the object
    public bool isHitten = false; // Track if the object is currently hit
    public float hitCooldown = 1f; // Cooldown time after being hit
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
