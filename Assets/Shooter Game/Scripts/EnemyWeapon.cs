using UnityEngine;

public class EnemyWeapon : Weapon   // <— inherit here
{
    [Header("Ballistics Overrides (optional)")]
    [SerializeField] private Transform muzzlePoint;     
    [SerializeField] private PhysicsProjectile bulletPrefab;
    [SerializeField] private float fireRate = 1f;

    [Header("Audio Overrides (optional)")]


    private float _nextFireTime;

    private void Awake()
    {
        // weapon base Awake will wire up audio, recoil, etc.
        base.Awake();  

        // now configure 3D sound specifics
        _audio.spatialBlend = 1f;
        _audio.rolloffMode  = AudioRolloffMode.Logarithmic;
        _audio.maxDistance  = 50f;
    }

    public void TryFire(Vector3 direction)
    {
        if (Time.time < _nextFireTime || muzzlePoint == null || bulletPrefab == null)
            return;

        _nextFireTime = Time.time + (1f / fireRate);
        Fire(direction);
    }

    private void Fire(Vector3 direction)
    {
        Vector3 spawnPos = muzzlePoint.position + muzzlePoint.forward * 0.1f;
        var proj = Instantiate(bulletPrefab, spawnPos, muzzlePoint.rotation);

        // **This is where you Init and Launch the projectile:**
        proj.Init(this, direction.normalized);
        proj.Launch();

        // Add tag and layer setup
        proj.gameObject.tag = "EnemyBullet";
        proj.gameObject.layer = LayerMask.NameToLayer("EnemyProjectile");

        // Play the inherited shootClip at inherited volume
        PlayShootSound();
    }
}
