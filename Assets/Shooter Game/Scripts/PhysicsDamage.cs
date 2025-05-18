using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsDamage : MonoBehaviour, ITakeDamage
{
    private Rigidbody _rb;

    private void Awake()
    {
       _rb = GetComponent<Rigidbody>(); 
    }

    public void TakeDamage(Weapon weapon, Projectile projectile, Vector3 contactPoint)
    {
        _rb.AddForce(projectile.transform.forward * weapon.GetShootingForce(), ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1) Try to get the Projectile component (the “other” collider is the projectile)
        var projectile = other.GetComponent<Projectile>();
        if (projectile == null) return;  // not a projectile → ignore

        // 2) Pull the Weapon reference out of that projectile
        var weapon = projectile.GetFiringWeapon();
        if (weapon == null)
        {
            Debug.LogError("Projectile has no firing weapon set!");
            return;
        }

        // 3) Now destroy self and notify any ITakeDamage handlers
        Destroy(gameObject);
        ITakeDamage[] damageTakers = other.GetComponentsInChildren<ITakeDamage>();
        foreach (var taker in damageTakers)
        {
            taker.TakeDamage(weapon, projectile, transform.position);
        }
    }
}
