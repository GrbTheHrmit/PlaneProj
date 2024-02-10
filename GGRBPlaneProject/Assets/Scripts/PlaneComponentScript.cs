using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlaneComponent : MonoBehaviour
{
    // Controlling plane object
    private Rigidbody parentPlane;

    // Physical Attributes
    private Vector3 COM;
    private float upperRadius;
    private float lowerRadius;
    private float surfaceArea;
    private float dragSurfaceArea;

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

        angleSpeed = 100;
        maxAngle = 45;
        currentAngle = 0;

        startRotation = gameObject.transform.localRotation;
    }

    void Update()
    {
        gameObject.transform.localRotation = startRotation * (isRudder ? Quaternion.Euler(0, currentAngle, 0) : Quaternion.Euler(currentAngle, 0, 0));
    }


    public void setRudder()
    { isRudder = true; }

    public void getForces(ref Vector3 force, ref Vector3 relativePos)
    {
        ////////
        /// TODO: CALCULATE FORCES ON A WING BASED ON WORLD ORIENTATION AND velocity and physical attributes
        /// /////////

        force = parentPlane != null ? ((isRudder ? new Vector3(-1, 0, 0) : new Vector3(0, 1, 0)) * parentPlane.velocity.magnitude * (currentAngle / maxAngle)) : new Vector3();
        relativePos = this.gameObject.transform.localPosition;

    }

    public void handleInput(float val)
    {
        float scaledVal = val * maxAngle;
        float incVal = Mathf.Min(Mathf.Abs(scaledVal - currentAngle), angleSpeed);
        currentAngle += incVal * Mathf.Sign(scaledVal - currentAngle);
    }
}