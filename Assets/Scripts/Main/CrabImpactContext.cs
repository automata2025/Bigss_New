// CrabImpactContext.cs
using UnityEngine;

public readonly struct CrabImpactContext
{
    public readonly Vector3 point;        // world contact/detonation point
    public readonly Vector3 normal;       // surface normal (if available)
    public readonly Vector3 incomingVel;  // projectile velocity at impact
    public readonly float energy;       // a normalized 0..1 "strength" (based on speed/profile)
    public readonly float radius;       // effect radius (for AOE-style hazards)
    public readonly GameObject instigator; // the player / root that launched it
    public readonly GameObject projectile; // the detached crab segment

    public CrabImpactContext(Vector3 point, Vector3 normal, Vector3 incomingVel, float energy, float radius, GameObject instigator, GameObject projectile)
    {
        this.point = point;
        this.normal = normal;
        this.incomingVel = incomingVel;
        this.energy = energy;
        this.radius = radius;
        this.instigator = instigator;
        this.projectile = projectile;
    }
}

