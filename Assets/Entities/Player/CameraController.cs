using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 1.0f;

    public Vector2 minBounds = new(-10.0f, -10.0f);
    public Vector2 maxBounds = new(10.0f, 10.0f);

    private Vector3 moveDir;




    public void OnCameraMovement(InputAction.CallbackContext value)
    {
        Vector2 inputDir = value.ReadValue<Vector2>();
        moveDir = new(inputDir.x, inputDir.y, 0.0f);
    }



    public void Update()
    {
        Vector3 motion = moveSpeed * Time.deltaTime * moveDir;

        transform.position += motion;


        transform.position = new(
            Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y),
            transform.position.z
            );
    }
}
