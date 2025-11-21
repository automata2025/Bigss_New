using UnityEngine;
public readonly struct CrabImpactContext
{
    public Vector3 Point { get; }       
    public Vector3 Normal { get; }      
    public Vector3 IncomingVel { get; }  
    public float Energy { get; }        
    public float Radius { get; }        
    public GameObject Instigator { get; } 
    public GameObject Projectile { get; } 

    public CrabImpactContext(
        Vector3 point,
        Vector3 normal,
        Vector3 incomingVel,
        float energy,
        float radius,
        GameObject instigator,
        GameObject projectile)
    {
        Point = point;
        Normal = normal;
        IncomingVel = incomingVel;
        Energy = energy;
        Radius = radius;
        Instigator = instigator;
        Projectile = projectile;
    }
}

