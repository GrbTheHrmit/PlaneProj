using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlaneComponent : MonoBehaviour
{
    // Controlling plane object
    private Rigidbody parentPlane;

    // Physical Attributes
    private Vector3 COM;
    private float upperRadius = 1.0f;
    private float lowerRadius = 1.0f;
    private float surfaceArea = 1.0f;
    private float wingspan = 1.0f;
    private float dragSurfaceArea;
    private float forceCoeff = 0.05f;
    private float torqueScale = 1.0f;

    // Movement Attributes
    private float currentAngle;
    private float angleSpeed;
    private float maxAngle;

    private bool isRudder;

    private Quaternion startRotation;

    void Start()
    {
        parentPlane = GetComponentInParent<PlaneScript>()?.gameObject.GetComponent<Rigidbody>();

        gameObject.layer = LayerMask.NameToLayer("PlaneLayer");

        angleSpeed = 0.1f;
        maxAngle = 10;
        currentAngle = 0;
        //liftCoefficient = 0.005f;

        startRotation = gameObject.transform.localRotation;
    }

    void Update()
    {
        gameObject.transform.localRotation = startRotation * (isRudder ? Quaternion.Euler(0, currentAngle, 0) : Quaternion.Euler(currentAngle, 0, 0));
    }

    public void setSurfaceArea(float area)
    {
        surfaceArea = area;
    }

    public void setUpperRadius(float rad)
    {
        upperRadius = rad;
    }
    public void setLowerRadius(float rad)
    {
        lowerRadius = rad;
    }
    public void setWingspan(float len)
    {
        wingspan = len;
    }

    public void setTorqueScale(float s)
    {
        torqueScale = s;
    }

    public void setRudder()
    { isRudder = true; }

    public void getForces(ref Vector3 force, ref Vector3 relativePos)
    {
        if (parentPlane == null) return;
        ////////
        /// TODO: CALCULATE FORCES ON A WING BASED ON WORLD ORIENTATION AND velocity and physical attributes
        /// /////////

        float aspectRatio = (wingspan * wingspan) / surfaceArea;
        float liftCoeff = 2 * 3.142f * (aspectRatio / (aspectRatio + 2.0f)) * (currentAngle / maxAngle);

        float forwardVelocity = Mathf.Max(parentPlane.gameObject.transform.InverseTransformDirection(parentPlane.velocity).z, 0);
        Debug.Log(parentPlane.velocity);
        // Calculate lift force
        Vector3 lift = (isRudder ? new Vector3(1, 0, 0) : new Vector3(0, -1, 0)) * Mathf.Pow(forwardVelocity, 2) * liftCoeff * surfaceArea * forceCoeff;

        // Add drag force
        Vector3 drag = new Vector3(0, 0, -1) * ((lift.sqrMagnitude / Mathf.Max(forwardVelocity * forwardVelocity * 3.142f * wingspan * wingspan, 0.1f)) + (0.01f * forwardVelocity * forwardVelocity));
        //Debug.Log(drag);

        force = (lift + drag);

        relativePos = -this.gameObject.transform.localPosition * torqueScale;

    }

    public void handleInput(float val)
    {
        float scaledVal = val * maxAngle;
        float incVal = Mathf.Min(Mathf.Abs(scaledVal - currentAngle), angleSpeed);
        currentAngle += incVal * Mathf.Sign(scaledVal - currentAngle);
    }
}