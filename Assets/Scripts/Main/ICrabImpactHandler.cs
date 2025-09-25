// ICrabImpactHandler.cs
public interface ICrabImpactHandler
{
    /// Called when a detached crab part impacts or detonates near this object.
    /// Return true if you "consumed" the event (optional; used if you want to stop propagation).
    bool OnCrabImpact(CrabImpactContext ctx);
}
