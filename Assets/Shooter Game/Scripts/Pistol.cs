using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pistol : Weapon
{
    [Header("Player Ballistics")]
    [SerializeField] private PhysicsProjectile bulletPrefab;  // your projectile prefab

    /// <summary>
    /// Called by Weapon.OnActivated when trigger is pressed.
    /// </summary>
    protected override void StartShooting(IXRInteractor interactor)
    {
        base.StartShooting(interactor);     // applies recoil if you want
        FireBullet();                       // actually spawn the bullet
    }

    private void FireBullet()
    {
        if (bulletPrefab == null || bulletSpawn == null) return;

        // 1) spawn slightly ahead of the muzzle to avoid self‐collision
        Vector3 spawnPos = bulletSpawn.position + bulletSpawn.forward * 0.1f;
        var proj = Instantiate(bulletPrefab, spawnPos, bulletSpawn.rotation);

        // 2) initialize it with your Weapon & forward direction
        proj.Init(this, bulletSpawn.forward);

        // 3) launch it
        proj.Launch();

        // 4) play your firing sound
        PlayShootSound();
    }
}
