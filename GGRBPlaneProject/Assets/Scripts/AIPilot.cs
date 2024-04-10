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
        
        
    }

    private void Awake()
    {
        lastInput = new SteeringInput();
        lastVel = new Vector3(0, 0, 0);
        target = new Vector3(500, 500, 500);
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

    public Vector3 getTarget() { return target; }

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
        if (Vector3.Dot(m_rigidbody.transform.forward, target - m_rigidbody.transform.position) > 0)
        {
            input = SeekElevators(lastInput);
            input = SeekRudder(input);
            input = SeekAilerons(input);
        }
        
        return input;
    }

    private SteeringInput LevelOut(SteeringInput input)
    {
        Vector3 right = m_rigidbody.transform.right;
        if(Mathf.Abs(right.y) > 0.01f)
        {
            float val = Mathf.Sign(right.y) * Mathf.Clamp(Mathf.Abs(right.y) * 0.1f, 0.1f, 1);
            input.leftAileron = -val;
            input.rightAileron = val;
            Debug.Log("Leveling " + val);
        }
        else
        {
            input.leftAileron = 0;
            input.rightAileron = 0;
        }

        return input;
    }

    private SteeringInput SeekAilerons(SteeringInput input)
    {
        input.acceleration = 1;
        Vector3 forwardVec = m_rigidbody.transform.forward;
        Vector3 targetVec = (target - m_rigidbody.transform.position).normalized;
        //Debug.Log("forward: " + forwardVec);
        //Debug.Log("target: " + targetVec);

        //float pitchDelta = Mathf.Atan2(forwardVec.y, forwardVec.x) - Mathf.Atan2(targetVec.y, targetVec.x);
        float pitchDelta = Mathf.Atan2(forwardVec.z, forwardVec.x) - Mathf.Atan2(targetVec.z, targetVec.x);

        if (Mathf.Abs(pitchDelta) > 0.01f && !((Mathf.Sign(pitchDelta) < 0 && m_rigidbody.transform.right.y > 0.9f) || (Mathf.Sign(pitchDelta) > 0 && m_rigidbody.transform.right.y < -0.9f)))
        {
            pitchDelta = 0.1f * pitchDelta / Mathf.PI;

            input.rightAileron = pitchDelta;
            input.leftAileron = -pitchDelta;
            //Debug.Log("dec");
        }
        else
        {
            input = LevelOut(input);

            Vector4 diffVec = Quaternion.Inverse(m_rigidbody.transform.rotation) * targetVec;
            if(Mathf.Abs(diffVec.y) > 0.01f)
            {
                input.rightAileron -= diffVec.y * 0.01f;
                input.leftAileron -= diffVec.y * 0.01f;
                //Debug.Log("incline change");
            }
        }
        

        //Debug.Log("vel " + m_rigidbody.angularVelocity.x);
       // Debug.Log(pitchDelta);

        return input;
    }

    private SteeringInput SeekElevators(SteeringInput input)
    {
        input.acceleration = 1;
        Vector3 forwardVec = m_rigidbody.transform.forward;
        Vector3 targetVec = (target - m_rigidbody.transform.position).normalized;
        //Debug.Log("forward: " + forwardVec);
        //Debug.Log("target: " + targetVec);

        float pitchDelta = Mathf.Atan2(forwardVec.z, forwardVec.y) - Mathf.Atan2(targetVec.z, targetVec.y);
        pitchDelta = pitchDelta / Mathf.PI;

        /*if (Mathf.Abs(pitchDelta) < Mathf.Abs(m_rigidbody.angularVelocity.x * 0.1f / Mathf.PI) && Mathf.Sign(pitchDelta) != Mathf.Sign(m_rigidbody.angularVelocity.x))
        {
            input.rightElevator = -pitchDelta;
            input.leftElevator = -pitchDelta;
            //Debug.Log("dec");
        }
        else
        {*/
            input.rightElevator = pitchDelta;
            input.leftElevator = pitchDelta;
        //}

        //Debug.Log("vel " + m_rigidbody.angularVelocity.x);
        //Debug.Log(pitchDelta);

        return input;
    }

    private SteeringInput SeekRudder(SteeringInput input)
    {
        input.acceleration = 1;
        Vector3 forwardVec = m_rigidbody.transform.forward;
        Vector3 targetVec = (target - m_rigidbody.transform.position).normalized;
        //Debug.Log("forward: " + forwardVec);
        //Debug.Log("target: " + targetVec);

        float pitchDelta = Mathf.Atan2(forwardVec.z, forwardVec.x) - Mathf.Atan2(targetVec.z, targetVec.x);
        pitchDelta = pitchDelta / Mathf.PI;


        input.rudder = -pitchDelta;

        //Debug.Log("rudding " + pitchDelta);

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
