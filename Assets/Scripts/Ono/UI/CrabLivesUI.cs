using UnityEngine;
using TMPro;

public class CrabLivesUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player movement / chain script.")]
    public P_Control_Physics playerControl;

    [Tooltip("TMP text that will display the lives.")]
    public TextMeshProUGUI livesText;

    [Header("Text Formats")]
    [Tooltip("Shown when there are spare crabs (FollowerCount > 0). {0} = number of spare crabs.")]
    public string normalFormat = "x{0}";

    [Tooltip("Shown when there are no spare crabs left (FollowerCount == 0).")]
    public string lastLifeText = "LAST ONE!";

    private int _lastCount = int.MinValue;

    private void Update()
    {
        if (playerControl == null || livesText == null)
            return;

        // How many follower crabs we currently have = spare lives
        int spareCrabs = playerControl.FollowerCount;

        // Only update UI when value changes
        if (spareCrabs == _lastCount)
            return;

        _lastCount = spareCrabs;

        if (spareCrabs > 0)
        {
            // e.g. "x3"
            livesText.text = string.Format(normalFormat, spareCrabs);
        }
        else
        {
            // No more followers – we're on the final crab
            livesText.text = lastLifeText;
        }
    }
}
