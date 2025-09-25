using UnityEngine;

public class DestructibleWall : MonoBehaviour, ICrabImpactHandler
{
    [SerializeField] private float requiredEnergy = 0.4f; // how strong the hit must be
    [SerializeField] private int hitPoints = 1;
    [SerializeField] private GameObject breakVfx;

    public bool OnCrabImpact(CrabImpactContext ctx)
    {
        if (ctx.energy < requiredEnergy) return false; // not enough; let others handle

        hitPoints--;
        if (hitPoints <= 0)
        {
            if (breakVfx) Instantiate(breakVfx, ctx.point, Quaternion.identity);
            Destroy(gameObject);
            return true; // consumed
        }
        return true; // took damage, stop further handling
    }
}

