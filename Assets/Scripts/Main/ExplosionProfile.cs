// ExplosionProfile.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Crab/Explosion Profile", fileName = "ExplosionProfile")]
public class ExplosionProfile : ScriptableObject
{
    [Header("Strength")]
    [SerializeField] private float baseRadius = 2.5f;
    [SerializeField] private float baseEnergy = 1.0f; // drives ctx.energy

    [Header("Physics Impulse (optional)")]
    [SerializeField] private bool applyPhysicsImpulse = true;
    [SerializeField] private float force = 12f;
    [SerializeField] private float upwardModifier = 0.0f;

    [Header("FX (optional)")]
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private AudioClip sfx;

    public float BaseRadius => baseRadius;
    public float BaseEnergy => baseEnergy;
    public bool ApplyPhysicsImpulse => applyPhysicsImpulse;
    public float Force => force;
    public float UpwardModifier => upwardModifier;
    public GameObject VfxPrefab => vfxPrefab;
    public AudioClip Sfx => sfx;
}
