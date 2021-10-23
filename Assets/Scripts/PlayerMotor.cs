using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]


public class PlayerMotor : MonoBehaviour
{
    [SerializeField]
    private Camera cam;



    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float cameraRotationX = 0f;
    private float currentCameraRotationX = 0f;
    private Vector3 thrusterForce = Vector3.zero;

    [SerializeField]
    private float cameraRotationLimit = 85f;


    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    // Gets a movement vector 
    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }
    
    
    // Gets a Rotational vector 
    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    // Gets a Rotational vector for the camera 
    public void cameraRotate(float _cameraRotationX)
    {
        cameraRotationX = _cameraRotationX;
    }

    // Get a force vector for our thrusters
    public void ApplyThruster(Vector3 _thrusterForce)
    {
        thrusterForce = _thrusterForce;
    }

    // Run every physics iteration
    private void FixedUpdate ()
    {
        PerformMovement();
        PreformRotation();

    }


    //Preform movement based on velocity variable 
    void PerformMovement()
    {

        if(velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
        if(thrusterForce != Vector3.zero)
        {
            rb.AddForce(thrusterForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }


    //Preform rotation

    void PreformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if(cam != null)
        {
            // Set our roation and clamp it
            currentCameraRotationX -= cameraRotationX;
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);
            
            // Apply our roation to the transform of the camera
            cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }

    }


}   // class
