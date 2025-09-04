using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;  
    private Vector3 offset;

    void Start()
    {
        // Record the starting distance between camera and player
        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        // Keep the same starting offset while copying player's movement
           transform.position = new Vector3(
            transform.position.x,                    // keep original X
            transform.position.y,                    // keep original Y
            player.position.z + offset.z             // follow only Z forward
        );
    }
}
