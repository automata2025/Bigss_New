using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovingText: MonoBehaviour
{

    public TextMeshProUGUI textComponent;

   
    public string baseMessage = "<b><color=#FFD447>⚠ OBJECTIVE:</color></b> Escape the dungeon <size=75%><color=#AAAAAA>(optional) with your children</color></size>";


    public int repeatCount = 4;


    public string separator = "    •    ";


    public float speed = 100f;

    public bool ignoreTimeScale = true;

    private RectTransform _containerRect;
    private RectTransform _textRect;

    private float _containerWidth;
    private float _textWidth;

    private void Awake()
    {
        _containerRect = GetComponent<RectTransform>();

        if (textComponent != null)
        {
            _textRect = textComponent.GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        if (_containerRect == null || _textRect == null || textComponent == null)
        {
            Debug.LogError("[TMPMarqueeText] Missing RectTransforms or TextMeshProUGUI.", this);
            enabled = false;
            return;
        }

        if (!string.IsNullOrEmpty(baseMessage) && repeatCount > 0)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < repeatCount; i++)
            {
                if (i > 0)
                    sb.Append(separator);
                sb.Append(baseMessage);
            }

            textComponent.text = sb.ToString();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_textRect);

        _containerWidth = _containerRect.rect.width;
        _textWidth = _textRect.rect.width;


        Vector2 pos = _textRect.anchoredPosition;
        pos.x = -_textWidth;
        _textRect.anchoredPosition = pos;
    }

    private void Update()
    {
        if (_textRect == null) return;

        float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

        Vector2 pos = _textRect.anchoredPosition;
        pos.x += speed * dt;

        if (pos.x > _containerWidth)
        {
            pos.x = -_textWidth;
        }

        _textRect.anchoredPosition = pos;
    }
}
