using UnityEngine;
using System.Collections;

public class FlameWaveController : MonoBehaviour
{
    [Header("Assign your turrets' flame particle systems")]
    public ParticleSystem[] turrets;   // drag your 6 turret particle systems here

    [Header("Wave settings")]
    public float delayBetweenTurrets = 0.3f; // time between each turret firing
    public float flameDuration = 1.0f;       // how long each flame stays on
    public float wavePause = 0.5f;           // pause after turret 6 before restarting

    void Start()
    {
        StartCoroutine(FlameWaveLoop());
    }

    IEnumerator FlameWaveLoop()
    {
        while (true) // loop forever
        {
            for (int i = 0; i < turrets.Length; i++)
            {
                if (turrets[i] != null)
                {
                    turrets[i].Play();
                    StartCoroutine(StopAfterTime(turrets[i], flameDuration));
                }
                yield return new WaitForSeconds(delayBetweenTurrets);
            }

            yield return new WaitForSeconds(wavePause);
        }
    }

    IEnumerator StopAfterTime(ParticleSystem ps, float time)
    {
        yield return new WaitForSeconds(time);
        ps.Stop();
    }
}
