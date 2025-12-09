using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach this to each coin object in your endless runner game
public class CoinCollector : MonoBehaviour
{
    [Header("Coin Value")]
    public int coinValue = 1; // How many coins this gives
    
    [Header("Collection Settings")]
    public bool destroyOnCollect = true;
    public string playerTag = "Player";
    
    [Header("Effects (Optional)")]
    public GameObject collectEffect; // Particle effect when collected
    public AudioClip collectSound;   // Sound when collected
    public AudioSource audioSource;  // Audio source to play sound
    
    private bool isCollected = false;
    
    // For 3D collision
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isCollected)
        {
            CollectCoin();
        }
    }
    
    // For 2D collision (if your game is 2D)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !isCollected)
        {
            CollectCoin();
        }
    }
    
    void CollectCoin()
    {
        isCollected = true;
        
        // Add coins to player using CoinsDisplay
        if (CoinsDisplay.Instance != null)
        {
            CoinsDisplay.Instance.AddCoins(coinValue);
        }
        else
        {
            Debug.LogWarning("⚠️ CoinsDisplay not found! Coin not added.");
        }
        
        // Play collect effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Play collect sound
        if (collectSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
        }
        
        // Destroy or hide the coin
        if (destroyOnCollect)
        {
            // Delay destruction if sound is playing
            if (collectSound != null && audioSource != null)
            {
                Destroy(gameObject, collectSound.length);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}