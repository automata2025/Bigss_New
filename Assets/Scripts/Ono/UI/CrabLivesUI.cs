using UnityEngine;
using TMPro;

public class CrabLivesUI : MonoBehaviour
{
    public P_Control_Physics playerControl;

    public TextMeshProUGUI livesText;

    public string normalFormat = "x{0}";

    public string lastLifeText = "LAST ONE!";

    private int _lastCount = int.MinValue;

    private void Update()
    {
        if (playerControl == null || livesText == null)
            return;

        int spareCrabs = playerControl.FollowerCount;

        if (spareCrabs == _lastCount)
            return;

        _lastCount = spareCrabs;

        if (spareCrabs > 0)
        {
            livesText.text = string.Format(normalFormat, spareCrabs);
        }
        else
        {
            livesText.text = lastLifeText;
        }
    }
}
