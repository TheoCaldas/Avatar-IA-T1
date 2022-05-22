using UnityEngine;
using System.Collections;
 
public class ThirdPersonCamera : MonoBehaviour 
{
    public float turnSpeed = 10.0f;     
    public float zoomSpeed = 10.0f;
    public Transform player;
     
    void Update()
    {
        transform.RotateAround(player.position, Vector3.up, Input.GetAxis("Mouse X") * turnSpeed);
        transform.RotateAround(player.position, -transform.right, Input.GetAxis("Mouse Y") * turnSpeed);
        gameObject.GetComponent<Camera>().fieldOfView += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
    }
}
