using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PhysicsProjectile : Projectile
{
    [SerializeField] private float lifeTime      = 5f;
    [SerializeField] private ParticleSystem impactVFX;
    private Rigidbody rb;
    private Collider  col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Physics-driven movement
        rb.isKinematic         = false;              // must be non-kinematic :contentReference[oaicite:1]{index=1}
        rb.useGravity          = false;              // no drop unless desired
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // avoid tunneling :contentReference[oaicite:2]{index=2}
        rb.linearDamping          = 0f;
        rb.angularDamping         = 0f;

        col.isTrigger          = false;               // so OnTriggerEnter always fires
        Destroy(gameObject, lifeTime);
    }

    public override void Init(Weapon firingWeapon, Vector3 direction)
    {
        base.Init(firingWeapon, direction);

        // Prevent self-collision with the shooter :contentReference[oaicite:3]{index=3}
        foreach (var shooterCol in firingWeapon.GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(col, shooterCol, true);
    }

    public override void Launch()
    {
        base.Launch();

        // Zero out any old motion
        rb.linearVelocity  = Vector3.zero;            // Unity 6+ uses linearVelocity :contentReference[oaicite:4]{index=4}
        rb.angularVelocity = Vector3.zero;

        // Instant velocity assignment
        rb.linearVelocity = fireDirection * weapon.GetShootingForce();  
        Debug.Log($"[Projectile] Launched dir={fireDirection}, speed={weapon.GetShootingForce()}");
    }

    private void OnTriggerEnter(Collider other) => HandleHit(other.gameObject, transform.position);

    private void OnCollisionEnter(Collision collision) 
        => HandleHit(collision.gameObject, collision.contacts[0].point);

private void HandleHit(GameObject hitObject, Vector3 contactPoint)
{
    // Impact VFX
    if (impactVFX != null)
    {
        var vfx = Instantiate(impactVFX, contactPoint, Quaternion.identity);
        vfx.Play();
        Destroy(vfx.gameObject, vfx.main.duration);
    }

    // Damageable lookup in parents and children
    var takers = new List<ITakeDamage>();
    takers.AddRange(hitObject.GetComponentsInParent<ITakeDamage>());
    takers.AddRange(hitObject.GetComponentsInChildren<ITakeDamage>());

    foreach (var taker in takers)
        taker.TakeDamage(weapon, this, contactPoint);

    // Determine if we hit an enemy anywhere in the parent chain
    bool hitEnemy = false;
    Transform t = hitObject.transform;
    while (t != null)
    {
        if (t.CompareTag("Enemy"))
        {
            hitEnemy = true;
            break;
        }
        t = t.parent;
    }

     // Add player check
    var player = hitObject.GetComponentInParent<Player>();
    if (player != null)
    {
        player.TakeDamage(weapon, this, contactPoint);
    }

    // Keep existing ITakeDamage logic
    foreach (var taker in takers)
        taker.TakeDamage(weapon, this, contactPoint);

    // Safely play the correct hit sound via the weapon's helper
    var w = weapon as Weapon;
    if (w)  // Unity’s “null” check also handles destroyed objects
    {
        w.PlayHitSound(hitEnemy);
    }

    Destroy(gameObject);
}

}
