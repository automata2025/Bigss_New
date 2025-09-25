using UnityEngine;
using System.Collections;

public class LaserTrap : MonoBehaviour
{
    public GameObject laserBeam;   // assign your red cube or beam here
    public float onDuration = 3f;
    public float offDuration = 3f;

    void Start()
    {
        StartCoroutine(LaserRoutine());
    }

    IEnumerator LaserRoutine()
    {
        while (true)
        {
            // Laser ON
            laserBeam.SetActive(true);
            yield return new WaitForSeconds(onDuration);

            // Laser OFF
            laserBeam.SetActive(false);
            yield return new WaitForSeconds(offDuration);
        }
    }
}
