using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    
    public float smoothing = 5f;        // The speed with which the camera will be following.

    Transform target;            // The position that that camera will be following.
    Vector3 offset;                     // The initial offset from the target.

    Camera cam;

    bool activate;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    public void Seguir(bool seguir, Transform target = null) {
        activate = seguir;
        if (seguir)
        {
            this.target = target;
            offset = Vector3.zero;//transform.position - target.position;
            offset.z = transform.position.z - target.position.z;
        }
    }
    
    void FixedUpdate()
    {
        if (activate && target != null) {
            // Create a postion the camera is aiming for based on the offset from the target.
            Vector3 targetCamPos = target.position + offset;

            // Smoothly interpolate between the camera's current position and it's target position.
            transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
        }
        
    }
}
