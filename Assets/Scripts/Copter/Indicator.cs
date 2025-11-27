using UnityEngine;
using System.Collections;

public class Indicator : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkDuration = 4f;   // How long blinking lasts
    [SerializeField] private float waitDuration = 2f;    // Time the light stays fully off
    [SerializeField] private float blinkSpeed = 0.2f;    // How fast it toggles on/off

    // Public properties (optional but useful)
    public float BlinkDuration
    {
        get => blinkDuration;
        set => blinkDuration = Mathf.Max(0f, value);
    }

    public float WaitDuration
    {
        get => waitDuration;
        set => waitDuration = Mathf.Max(0f, value);
    }

    public float BlinkSpeed
    {
        get => blinkSpeed;
        set => blinkSpeed = Mathf.Max(0.01f, value);
    }

    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        StartCoroutine(BlinkLoop());
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            // BLINK for blinkDuration seconds
            float timer = 0f;
            while (timer < blinkDuration)
            {
                _renderer.enabled = !_renderer.enabled;
                yield return new WaitForSeconds(blinkSpeed);
                timer += blinkSpeed;
            }

            // Ensure OFF after blink phase
            _renderer.enabled = false;

            // WAIT fully off
            yield return new WaitForSeconds(waitDuration);
        }
    }
}
