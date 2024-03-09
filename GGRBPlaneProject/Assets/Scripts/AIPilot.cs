using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AIPilot : ScriptableObject
{
    private PlaneScript m_plane;
    private Rigidbody m_rigidbody;

    private Vector3 target;

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
        target = new Vector3(100, 500, -500);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setPlane(PlaneScript plane)
    { m_plane = plane; }

    public void setRigidBody(Rigidbody rb)
    { m_rigidbody = rb; }

    public void setTarget(Vector3 t)
    { target = t; }

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

    private SteeringInput SeekTarget()
    {
        SteeringInput input = lastInput;
        input.acceleration = 1;
        Vector3 forwardVec = m_rigidbody.transform.forward;
        Vector3 targetVec = (target - m_rigidbody.transform.position).normalized;
        //Debug.Log("forward: " + forwardVec);
        //Debug.Log("target: " + targetVec);

        float pitchDelta = Mathf.Acos(Vector2.Dot(new Vector2(forwardVec.z, forwardVec.y).normalized, new Vector2(targetVec.z, targetVec.y).normalized));
        pitchDelta -= Mathf.PI * 0.5f;
        //Debug.Log(pitchDelta);

        //if (pitchDelta < m_rigidbody.angularVelocity.x)
        //{
            input.rightElevator += pitchDelta * 0.001f;
            input.leftElevator += pitchDelta * 0.001f;
            //Debug.Log(pitchDelta);
        //}

        return input;

    }

    public SteeringInput getSteering(float dt)
    {
        SteeringInput input = new SteeringInput();
        if(m_plane != null)
        {
            //input = holdAltitude();
            input = SeekTarget();
        }

        input.leftAileron = Mathf.Clamp(input.leftAileron, -1, 1);
        input.rightAileron = Mathf.Clamp(input.rightAileron, -1, 1);
        input.leftElevator = Mathf.Clamp(input.leftElevator, -1, 1);
        input.rightElevator = Mathf.Clamp(input.rightElevator, -1, 1);

        lastInput = input;
        return input;
    }
}
