using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Weapon : MonoBehaviour
{
    [Header("Ballistics")]
    [SerializeField] protected float shootingForce;
    [SerializeField] protected Transform bulletSpawn;
    [SerializeField] protected float recoilForce;
    [SerializeField] protected float damage;

    [Header("Audio")]
    [SerializeField] public AudioClip shootClip;
    [SerializeField] public AudioClip enemyHitClip;
    [SerializeField] public AudioClip worldHitClip;

    // new volume controls
    [Range(0f,1f)]
    [SerializeField] private float shootVolume = 1f;
    [Range(0f,1f)]
    [SerializeField] private float hitVolume   = 1f;

    protected XRGrabInteractable grabInteractable;
    protected new Rigidbody rb;
    public AudioSource _audio;

    protected virtual void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb               = GetComponent<Rigidbody>();

        // Set up audio :contentReference[oaicite:9]{index=9}
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;

        // XR event hooks :contentReference[oaicite:10]{index=10}
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnPickUp);
            grabInteractable.selectExited .AddListener(OnDrop);
            grabInteractable.activated     .AddListener(OnActivated);
            grabInteractable.deactivated   .AddListener(OnDeactivated);
        }
    }

    protected virtual void OnPickUp(SelectEnterEventArgs args) => rb.isKinematic = true;
    protected virtual void OnDrop  (SelectExitEventArgs  args) => rb.isKinematic = false;

    protected virtual void OnActivated(ActivateEventArgs args)   
    {
        PlayShootSound();
        StartShooting(args.interactorObject);
    }

    protected virtual void OnDeactivated(DeactivateEventArgs args)
        => StopShooting(args.interactorObject);

    protected virtual void StartShooting(IXRInteractor interactor)
    {
        Shoot();  // default: one shot per press
    }

    protected virtual void StopShooting(IXRInteractor interactor) { }

    /// <summary>Applies recoil; subclasses override Shoot for full behavior.</summary>
    protected virtual void Shoot() => ApplyRecoil(bulletSpawn.forward);

    protected void ApplyRecoil(Vector3 dir) 
        => rb.AddForce(-dir.normalized * recoilForce, ForceMode.Impulse);

    public float GetShootingForce() => shootingForce;
    public float GetDamage()        => damage;

    public void PlayShootSound()
    {
        if (shootClip != null && _audio != null)
        {
            _audio.PlayOneShot(shootClip, shootVolume);
        }
    }

    public void PlayHitSound(bool isEnemyHit)
    {
        var clip = isEnemyHit ? enemyHitClip : worldHitClip;
        if (clip != null && _audio != null)
        {
            _audio.PlayOneShot(clip, hitVolume);
        }
    }

}
