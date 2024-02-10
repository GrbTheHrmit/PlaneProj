using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlaneComponent : MonoBehaviour
{
    // Controlling plane object
    private GameObject parentPlane;

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

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("PlaneLayer");

        angleSpeed = 100;
        maxAngle = 45;
        currentAngle = 0;
    }

    void Update()
    {
        gameObject.transform.rotation = isRudder ? Quaternion.Euler(0, currentAngle, 0) : Quaternion.Euler(currentAngle, 0, 0);
    }

    public void setRudder()
    { isRudder = true; }

    public Vector3 getForces()
    {
        ////////
        /// TODO: CALCULATE FORCES ON A WING BASED ON WORLD ORIENTATION AND velocity and physical attributes
        /// /////////

        return new Vector3();
    }

    public void handleInput(float val)
    {
        float scaledVal = val * maxAngle;
        float incVal = Mathf.Min(Mathf.Abs(scaledVal - currentAngle), angleSpeed);
        currentAngle += incVal * Mathf.Sign(scaledVal - currentAngle);

        ///////
        /// TODO: CHANGE VISUAL ORIENTATION OF WING
        ///////
    }
}