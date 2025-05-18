using UnityEngine;

public interface ITakeDamage
{
    void TakeDamage(Weapon weapon, Projectile projectile, Vector3 contactPoint);
}
