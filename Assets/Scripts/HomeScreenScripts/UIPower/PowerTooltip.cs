using UnityEngine;
using TMPro;

public class PowerTooltip : MonoBehaviour
{
    public PowerManager powerManager;
    public TMP_Text infoText;

    public void OnPowerClicked()
    {
        infoText.text = powerManager.GetRechargeStatus();
    }
}