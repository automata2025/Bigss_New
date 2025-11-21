using UnityEngine;

public class DestructibleWall : MonoBehaviour, ICrabImpactHandler
{
    [Header("Gameplay")]
    [SerializeField] private float requiredEnergy = 0.4f;
    [SerializeField] private int hitPoints = 1;

    [Header("FX")]
    [SerializeField] private GameObject breakVfx;

    private bool _destroyed;

    public bool IsAlive => !_destroyed && hitPoints > 0;

    public bool OnCrabImpact(CrabImpactContext ctx)
    {
        if (!IsAlive)
            return false;

        if (ctx.Energy < requiredEnergy)
            return false;

        hitPoints--;
        if (hitPoints > 0)
            return true; // we consumed the hit but are still alive

        // Kill
        _destroyed = true;

        if (breakVfx != null)
        {
            Instantiate(breakVfx, ctx.Point, Quaternion.identity);
        }

        Destroy(gameObject);
        return true;
    }
}
