using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class PlaneComponent
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

    Vector3 getForces()
    {
        ////////
        /// TODO: CALCULATE FORCES ON A WING BASED ON WORLD ORIENTATION AND velocity and physical attributes
        /// /////////

        return new Vector3();
    }

    void handleInput(float val)
    {
        float incVal = Mathf.Min(Mathf.Abs(val - currentAngle), angleSpeed);
        currentAngle += incVal * Mathf.Sign(val - currentAngle);

        ///////
        /// TODO: CHANGE VISUAL ORIENTATION OF WING
        ///////
    }
}