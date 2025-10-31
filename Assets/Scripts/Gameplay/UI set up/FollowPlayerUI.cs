using UnityEngine;

public class FollowPlayerUI : MonoBehaviour
{

    public Transform target3D;            // The single 3D object
    public RectTransform[] uiElements;    // All UI elements that should follow it

    void Update()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target3D.position);

        for (int i = 0; i < uiElements.Length; i++)
        {
            uiElements[i].position = screenPos;
        }
    }
}