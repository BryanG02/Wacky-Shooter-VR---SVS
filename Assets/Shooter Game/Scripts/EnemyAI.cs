using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour, ITakeDamage
{
    const string RUN_TRIGGER = "Run";
    const string CROUCH_TRIGGER = "Crouch";
    const string SHOOT_TRIGGER = "Shoot";

    [Header("Stats")]
    [SerializeField] private float startingHealth;
    [SerializeField] private float damage;
    [SerializeField, Range(0,100)] private float shootingAccuracy;
    [SerializeField] private ParticleSystem bloodSplatterFX;

    [Header("Movement & Engagement")]
    [SerializeField] private float engageDistance = 10f;
    [SerializeField] private float rotationSpeed  = 120f;

    [Header("Shooting Settings")]
    [SerializeField] private Transform shootingPosition;
    [SerializeField] private EnemyWeapon enemyWeapon;
    [SerializeField] private PhysicsProjectile enemyBulletPrefab;
    [SerializeField] private float enemyFireRate     = 1f;
    [SerializeField] private float enemyBulletOffset = 0.2f;

    [Header("Crouch on Hit")]
    [SerializeField] private float crouchDuration = 1f;
    [SerializeField] private float crouchCooldown = 5f;

    private float _health;
    private bool  _initialized;
    private bool  isShooting;
    private bool  _canCrouch = true;
    private NavMeshAgent agent;
    private Animator     animator;
    private Player       player;
    private Coroutine    _shootRoutine;

    public void Init(Player player, Transform coverPoint)
    {
        this.player = player;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        _health = startingHealth;

        agent.updateRotation  = false;
        agent.stoppingDistance = engageDistance;

        _initialized = true;

        // Immediately face player and start running
        FacePlayerInstantly();
        animator.SetTrigger(RUN_TRIGGER);

        MoveTo(coverPoint.position);
    }

    private void Update()
    {
        if (!_initialized) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        RotateTowardsPlayer();

        if (dist > engageDistance)
        {
            // Chase player
            if (isShooting) StopShooting();
            animator.SetTrigger(RUN_TRIGGER);
            MoveTo(player.transform.position);
        }
        else
        {
            // In range: stop moving
            if (agent.hasPath)
                agent.ResetPath();

            if (!isShooting)
                StartShooting();
        }
    }

    private void MoveTo(Vector3 dest)
    {
        agent.isStopped = false;
        agent.SetDestination(dest);
    }

    private void StartShooting()
    {
        isShooting = true;
        animator.SetTrigger(SHOOT_TRIGGER);
        _shootRoutine = StartCoroutine(ShootingLoop());
    }

    private IEnumerator ShootingLoop()
    {
        while (isShooting)
        {
            // AnimationEvent OnShootFrame will call FireAtPlayer
            yield return new WaitForSeconds(1f / enemyFireRate);
        }
    }

    public void OnShootFrame()
    {
        if (!isShooting) return;
        FireAtPlayer();
    }

    private void StopShooting()
    {
        isShooting = false;
        if (_shootRoutine != null) StopCoroutine(_shootRoutine);
    }

    private void FireAtPlayer()
    {
        Vector3 origin  = shootingPosition.position + shootingPosition.forward * enemyBulletOffset;
        Vector3 target  = player.GetHeadPosition();
        Vector3 dir     = (target - origin).normalized;

        // Add inaccuracy
        float missPct = 1f - shootingAccuracy/100f;
        float angle   = missPct * 30f;
        dir = Quaternion.Euler(
            UnityEngine.Random.Range(-angle, angle),
            UnityEngine.Random.Range(-angle, angle),
            0f) * dir;

        enemyWeapon.TryFire(dir);
    }

    private void RotateTowardsPlayer()
    {
        Vector3 dir = player.GetHeadPosition() - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private void FacePlayerInstantly()
    {
        Vector3 dir = player.GetHeadPosition() - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    public void TakeDamage(Weapon weapon, Projectile projectile, Vector3 contactPoint)
    {
        _health -= weapon.GetDamage();
        SpawnBlood(contactPoint);

        if (_health <= 0f)
        {
            // Notify both GameManager and Player
            GameManager manager = FindFirstObjectByType<GameManager>();
            Player player = FindAnyObjectByType<Player>();
            
            if (manager != null) manager.RegisterEnemyKill();
            if (player != null) player.PlayEnemyDeathSound();
            
            Destroy(gameObject);
        }

        if (_canCrouch)
            StartCoroutine(CrouchRoutine());
    }

    private void SpawnBlood(Vector3 point)
    {
        var rot = Quaternion.LookRotation((transform.position - point).normalized);
        var splat = Instantiate(bloodSplatterFX, point, rot);
        splat.Play();
        Destroy(splat.gameObject, splat.main.duration);
    }

    private IEnumerator CrouchRoutine()
    {
        _canCrouch = false;
        animator.SetTrigger(CROUCH_TRIGGER);
        StopShooting();

        yield return new WaitForSeconds(crouchDuration);
        animator.SetTrigger(RUN_TRIGGER);

        yield return new WaitForSeconds(crouchCooldown);
        _canCrouch = true;
    }
}
