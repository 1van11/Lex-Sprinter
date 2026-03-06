using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void ExitApplication()
    {
        Debug.Log("Game is exiting...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}