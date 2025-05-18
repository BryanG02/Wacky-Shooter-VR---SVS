using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour 
{
    protected Weapon weapon;
    protected Vector3 fireDirection;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic            = false;
        _rb.useGravity           = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    // Now accepts ANY IWeapon (player or enemy)
    public virtual void Init(Weapon firingWeapon, Vector3 direction)
    {
        weapon        = firingWeapon;
        fireDirection = direction.normalized;
    }

    public virtual void Launch()
    {
        if (_rb == null || weapon == null) return;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.linearVelocity        = fireDirection * weapon.GetShootingForce();
    }

    public Weapon GetFiringWeapon() => weapon;
    public float   GetDamage()        => weapon?.GetDamage() ?? 0f;
}

