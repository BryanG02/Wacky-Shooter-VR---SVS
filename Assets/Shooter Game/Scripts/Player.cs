using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour, ITakeDamage  // Implement damage interface
{
    [SerializeField] private GameManager gameManager;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Hit Feedback")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private AudioClip hitSound;

    [Header("VR Setup")]
    [SerializeField] private Transform vrCamera;
    [SerializeField] private Collider bodyCollider;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Bleeding UI")]
    [SerializeField] private GameObject bleedingPanel;
    [SerializeField] private Image[] bleedImages; // Assign Bleed1-4 in order
        
    [Header("Audio")]
    [SerializeField] private AudioClip enemyDeathSound;

    // Layer constants
    private const string PLAYER_LAYER = "Player";
    private const string ENEMY_BULLET_LAYER = "EnemyProjectile";

    private AudioSource _audio;
    private float currentHealth;

    private void Awake()
    {
        InitializeCollider();
        InitializeAudio();
        InitializeGameOverUI();
        InitializeBleedingUI();
    }

    public void ResetPlayer()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        UpdateBleedingUI();
        gameOverPanel.SetActive(false);
    }
    public void PlayEnemyDeathSound()
    {
        if (enemyDeathSound != null)
        {
            _audio.PlayOneShot(enemyDeathSound);
        }
    }

    private void InitializeCollider()
    {
        if (!bodyCollider)
        {
            bodyCollider = GetComponentInChildren<Collider>();
            Debug.LogWarning("Auto-assigned body collider: " + bodyCollider?.name);
        }

        if (bodyCollider)
        {
            bodyCollider.isTrigger = true;
            bodyCollider.gameObject.layer = LayerMask.NameToLayer(PLAYER_LAYER);
            
            // Ensure kinematic rigidbody
            if (!bodyCollider.TryGetComponent<Rigidbody>(out var rb))
            {
                rb = bodyCollider.gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
        else
        {
            Debug.LogError("Missing player collider!");
        }
    }

    private void InitializeAudio() => _audio = GetComponent<AudioSource>();
    private void InitializeGameOverUI() => gameOverPanel?.SetActive(false);

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Collided with: {other.name} (Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)})");

        if (other.CompareTag("EnemyBullet") && 
            other.gameObject.layer == LayerMask.NameToLayer(ENEMY_BULLET_LAYER))
        {
            ProcessBulletHit(other);
        }
    }

    private void ProcessBulletHit(Collider bulletCollider)
    {
        var projectile = bulletCollider.GetComponent<Projectile>();
        if (projectile == null)
        {
            Debug.LogWarning("Enemy bullet missing Projectile component!");
            return;
        }

        // Get damage from weapon system
        float damage = projectile.GetFiringWeapon()?.GetDamage() ?? 10f;
        Vector3 hitPoint = bulletCollider.ClosestPoint(GetHeadPosition());
        
        TakeDamage(damage, hitPoint);
        Destroy(bulletCollider.gameObject);
    }


    private void InitializeBleedingUI()
    {
        if (bleedingPanel != null)
        {
            bleedingPanel.SetActive(false);
            foreach (Image img in bleedImages)
            {
                img.gameObject.SetActive(false);
            }
        }
    }

    // Implement ITakeDamage interface
    public void TakeDamage(Weapon weapon, Projectile projectile, Vector3 contactPoint)
    {
        // Convert to simple damage call to maintain existing system
        TakeDamage(weapon.GetDamage(), contactPoint);
    }

    public void TakeDamage(float damage, Vector3 hitPoint)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        UpdateHealthUI();
        UpdateBleedingUI(); // Add this line
        PlayHitEffects(hitPoint);

        if (currentHealth <= 0) HandleGameOver();
    }

    private void UpdateBleedingUI()
    {
        if (bleedingPanel == null || bleedImages.Length < 4) return;

        float healthPercentage = currentHealth / maxHealth;
        int imagesToShow = Mathf.Clamp(4 - Mathf.FloorToInt(healthPercentage * 5), 0, 4);

        // Activate panel if any bleeding should be visible
        bleedingPanel.SetActive(imagesToShow > 0 && currentHealth > 0);

        for (int i = 0; i < bleedImages.Length; i++)
        {
            if (bleedImages[i] != null)
            {
                bleedImages[i].gameObject.SetActive(i < imagesToShow);
            }
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth/maxHealth:P0}";
            healthText.color = Color.Lerp(Color.red, Color.white, currentHealth/maxHealth);
        }
    }

    private void PlayHitEffects(Vector3 position)
    {
        if (hitEffect != null)
        {
            // Add position offset for VR visibility
            Vector3 effectPos = position + Vector3.up * 0.5f; // Adjust based on your character height
            var effect = Instantiate(hitEffect, effectPos, Quaternion.identity);
            effect.Play();

            Debug.Log($"Spawned hit effect at {effectPos}");
            Destroy(effect.gameObject, effect.main.duration);
        }
        
        if (hitSound && _audio) _audio.PlayOneShot(hitSound);
    }


    private void HandleGameOver()
    {
        if (bleedingPanel != null)
        {
            bleedingPanel.SetActive(false);
        }

        // Cleanup enemies
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies) Destroy(enemy);
        
        // Show game over UI
        gameOverPanel?.SetActive(true);
        gameManager.GameOver(); // Notify GameManager instead of handling UI directly
    }


    public Vector3 GetHeadPosition()
    {
        return vrCamera != null ? 
            vrCamera.position : 
            transform.position + Vector3.up * 1.6f;
    }
}