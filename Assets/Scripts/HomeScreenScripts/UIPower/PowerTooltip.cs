using UnityEngine;
using UnityEngine.UI;

public class PowerTooltip : MonoBehaviour
{
    public PowerManager powerManager;
    public Text infoText;

    public void OnPowerClicked()
    {
        infoText.text = powerManager.GetRechargeStatus();
    }
}
