using UnityEngine;

public class Indicator : MonoBehaviour
{
    // === Properties ===
    public float BlinkDuration { get; set; } = 4f;   // blink time
    public float WaitDuration { get; set; } = 2f;   // off time
    public float BlinkSpeed { get; set; } = 0.2f; // toggle speed

    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        StartCoroutine(BlinkLoop());
    }

    private System.Collections.IEnumerator BlinkLoop()
    {
        while (true)
        {
            // --- BLINK for BlinkDuration seconds ---
            float timer = 0f;
            while (timer < BlinkDuration)
            {
                _renderer.enabled = !_renderer.enabled;
                yield return new WaitForSeconds(BlinkSpeed);
                timer += BlinkSpeed;
            }

            // fully OFF after blinking
            _renderer.enabled = false;

            // --- WAIT ---
            yield return new WaitForSeconds(WaitDuration);
        }
    }
}
