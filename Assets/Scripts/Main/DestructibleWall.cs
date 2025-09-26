using UnityEngine;

public class DestructibleWall : MonoBehaviour, ICrabImpactHandler
{
    [SerializeField] private float requiredEnergy = 0.4f; 
    [SerializeField] private int hitPoints = 1;
    [SerializeField] private GameObject breakVfx;

    public bool OnCrabImpact(CrabImpactContext ctx)
    {
        if (ctx.energy < requiredEnergy) return false; 

        hitPoints--;
        if (hitPoints <= 0)
        {
            if (breakVfx) Instantiate(breakVfx, ctx.point, Quaternion.identity);
            Destroy(gameObject);
            return true; 
        }
        return true; 
    }
}

