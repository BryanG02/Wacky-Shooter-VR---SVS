using UnityEngine;  // Add this line

public static class ParticleSystemExtensions
{
    public static ParticleSystem PlayAndDestroy(this ParticleSystem ps, float duration)
    {
        ps.Play();
        Object.Destroy(ps.gameObject, duration);  // Explicit namespace
        return ps;
    }
}