using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AIPilot : ScriptableObject
{
    private PlaneScript m_plane;
    private Rigidbody m_rigidbody;

    private SteeringInput lastInput;
    private Vector3 lastVel;
    private Vector3 lastAngVel;

    private float altDeadzone = 1;
    private float velDeadzone = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        lastInput = new SteeringInput();
        lastVel = new Vector3(0,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setPlane(PlaneScript plane)
    { m_plane = plane; }

    public void setRigidBody(Rigidbody rb)
    { m_rigidbody = rb; }

    private SteeringInput holdSpeed()
    {
        SteeringInput input = lastInput;

        float lastAccel = m_rigidbody.velocity.z - lastVel.z;

        if (Mathf.Abs(lastAccel) < velDeadzone)
        {
            input.acceleration -= 0.002f * lastAccel / velDeadzone;
        }

        return input;
    }

    private SteeringInput holdAltitude()
    {
        SteeringInput input = lastInput;
        input.acceleration = 0.75f;
        
        if(m_rigidbody != null)
        {

            if(Mathf.Abs(m_rigidbody.velocity.z) < 3)
            {
                input.acceleration += 0.002f;
            }
            else if(Mathf.Abs(m_rigidbody.velocity.z) > 4)
            {
                input.acceleration -= 0.002f;
            }
            else
            {
                input = holdSpeed();
            }

            float yVel = m_rigidbody.velocity.y;
            if (yVel > altDeadzone)
            {
                input.rightAileron += 0.001f * Mathf.Abs(yVel) / altDeadzone;
                input.leftAileron += 0.001f * Mathf.Abs(yVel) / altDeadzone;
            }
            else if(yVel < altDeadzone)
            {
                input.rightAileron -= 0.001f * Mathf.Abs(yVel) / altDeadzone;
                input.leftAileron -= 0.001f * Mathf.Abs(yVel) / altDeadzone;
            }
        }

        return input;
    }

    public SteeringInput getSteering(float dt)
    {
        SteeringInput input = new SteeringInput();
        if(m_plane != null)
        {
            input = holdAltitude();
        }
        
        lastInput = input;
        return input;
    }
}
