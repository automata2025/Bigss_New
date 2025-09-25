using UnityEngine;
using System.Collections;

public class FlameWaveController : MonoBehaviour
{
    [Header("Assign your turrets' flame particle systems")]
    [SerializeField] private ParticleSystem[] turrets;   // private backing field

    [Header("Wave settings")]
    [SerializeField] private float delayBetweenTurrets = 0.3f;
    [SerializeField] private float flameDuration = 1.0f;
    [SerializeField] private float wavePause = 0.5f;

    public ParticleSystem[] Turrets => turrets;   // expression-bodied property
    public float DelayBetweenTurrets { get => delayBetweenTurrets; set => delayBetweenTurrets = value; }
    public float FlameDuration { get => flameDuration; set => flameDuration = value; }
    public float WavePause { get => wavePause; set => wavePause = value; }

    void Start()
    {
        StartCoroutine(FlameWaveLoop());
    }

    IEnumerator FlameWaveLoop()
    {
        while (true) // loop forever
        {
            for (int i = 0; i < Turrets.Length; i++)
            {
                if (Turrets[i] != null)
                {
                    Turrets[i].Play();
                    StartCoroutine(StopAfterTime(Turrets[i], FlameDuration));
                }
                yield return new WaitForSeconds(DelayBetweenTurrets);
            }

            yield return new WaitForSeconds(WavePause);
        }
    }

    IEnumerator StopAfterTime(ParticleSystem ps, float time)
    {
        yield return new WaitForSeconds(time);
        ps.Stop();
    }
}
