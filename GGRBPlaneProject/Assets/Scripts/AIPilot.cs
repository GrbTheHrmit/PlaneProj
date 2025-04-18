using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

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
        //target = new Vector3(0, 500, -500);
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

    private float distBetweenRads(float r1, float r2)
    {
        //Debug.Log("R1 " + r1 + "\nR2 " + r2);
        float diff = Mathf.Abs(r1 - r2);
        return diff <= Mathf.PI ? Mathf.Sign(r1 - r2) * diff : Mathf.Sign(r2 - r1) * (Mathf.PI * 2 - diff);
    }

    private float remapRad(float r1)
    {
        return Mathf.Abs(r1) <= Mathf.PI ? r1 : r1 - Mathf.PI * 2;
    }

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

    private SteeringInput holdAltitude(SteeringInput input)
    {
        //SteeringInput input = lastInput;
        input.acceleration = 1;
        
        if(m_rigidbody != null)
        {

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

    private SteeringInput holdIncline(SteeringInput input)
    {
        input.acceleration = 1;

        if (m_rigidbody != null)
        {

            float yVel = m_rigidbody.transform.InverseTransformDirection(m_rigidbody.velocity).y;
            //Debug.Log(yVel);
            if (yVel > 1.0f)
            {
                input.rightAileron += 0.001f * Mathf.Abs(yVel);
                input.leftAileron += 0.001f * Mathf.Abs(yVel);
            }
            else if (yVel < -1.0f)
            {
                input.rightAileron -= 0.001f * Mathf.Abs(yVel);
                input.leftAileron -= 0.001f * Mathf.Abs(yVel);
            }
        }

        return input;
    }

    private SteeringInput SeekTarget()
    {
        SteeringInput input = lastInput;

        if (Vector3.Dot(m_rigidbody.transform.forward, target - m_rigidbody.transform.position) > 0)
        {
            input = SeekElevators(input, target);
            input = SeekRudder(input, target);
            input = SeekAilerons(input, target);
            input = holdIncline(input);
            input = holdAltitude(input);
        }
        else
        {
            //input = LevelOut(input);
            input = TurnAround(input, target);
            //Debug.Log("turning around");
        }
        
        return input;
    }

    private SteeringInput alignAilerons(SteeringInput input, Vector3 tar)
    {
        Vector3 upVec = new Vector3(0, 1, 0);
        Vector3 targetVec = m_rigidbody.transform.InverseTransformDirection((target - m_rigidbody.transform.position).normalized);
        float pitchDelta = distBetweenRads(Mathf.Atan2(targetVec.y, targetVec.x), Mathf.Atan2(upVec.y, upVec.x));
        pitchDelta = pitchDelta / Mathf.PI;

        input.rightAileron += -pitchDelta;
        input.leftAileron += pitchDelta;
        return input;
    }

    private SteeringInput LevelOut(SteeringInput input)
    {
        Vector3 right = m_rigidbody.transform.right;
        Vector3 up = m_rigidbody.transform.up;
        if(Mathf.Abs(right.y) > 0.1f || up.y < 0)
        {
            float val = Mathf.Sign(right.y) * Mathf.Clamp(Mathf.Abs(right.y), 0.1f, 1);
            input.leftAileron = -val * 0.1f;
            input.rightAileron = val * 0.1f;
            //Debug.Log("Leveling " + val);
        }

        return input;
    }

    private SteeringInput SeekAilerons(SteeringInput input, Vector3 tar)
    {
        input.acceleration = 1;
        Vector3 upVec = new Vector3(0, 0,1);
        Vector3 targetVec = m_rigidbody.transform.InverseTransformDirection((tar - m_rigidbody.transform.position).normalized);

        //float pitchDelta = distBetweenRads(Mathf.Atan2(targetVec.y, targetVec.x), Mathf.Atan2(upVec.y, upVec.x));
        float pitchDelta = Mathf.PI * Quaternion.FromToRotation(targetVec, upVec).eulerAngles.y / 180;
        pitchDelta = remapRad(pitchDelta);

        //Mathf.Abs(targetVec.z) < 0.9f &&
        if (Mathf.Abs(targetVec.z) < 0.92f && Mathf.Abs(pitchDelta) > 0.1f)
        {
            pitchDelta = pitchDelta / Mathf.PI;

            input.rightAileron += -pitchDelta * 0.1f;
            input.leftAileron += pitchDelta * 0.1f;
            //Debug.Log("pitching");
        }
        else
        {
            //Debug.Log("leveling");
            input = LevelOut(input);

        }
        Vector4 diffVec = targetVec;
        if (Mathf.Abs(diffVec.y) > 0.01f)
        {
            input.rightAileron -= diffVec.y * 0.01f;
            input.leftAileron -= diffVec.y * 0.01f;
            //Debug.Log("upping " + diffVec.y);
        }


        //Debug.Log("vel " + m_rigidbody.angularVelocity.x);
        //Debug.Log(input.leftAileron);
        //Debug.Log(input.rightAileron);

        return input;
    }

    private SteeringInput SeekElevators(SteeringInput input, Vector3 tar)
    {
        input.acceleration = 1;
        Vector3 forwardVec = new Vector3(0, 0, 1);
        Vector3 targetVec = m_rigidbody.transform.InverseTransformDirection((tar - m_rigidbody.transform.position).normalized);
        //Debug.Log("forward: " + forwardVec);
        //Debug.Log("target: " + targetVec);

        float pitchDelta = Mathf.PI * Quaternion.FromToRotation(targetVec, forwardVec).eulerAngles.x / 180;
        pitchDelta = remapRad(pitchDelta);
        //float pitchDelta = distBetweenRads(Mathf.Atan2(targetVec.y, targetVec.z), Mathf.Atan2(forwardVec.y, forwardVec.z));
        //Debug.Log(pitchDelta);

        
        pitchDelta = pitchDelta / Mathf.PI;

        //if (targetVec.y < 0) pitchDelta = -pitchDelta;

        input.rightElevator = pitchDelta;// * 0.1f;
        input.leftElevator = pitchDelta;// * 0.1f;
        

        //Debug.Log("vel " + m_rigidbody.angularVelocity.x);
        

        return input;
    }

    private SteeringInput SeekRudder(SteeringInput input, Vector3 tar)
    {
        input.acceleration = 1;
        Vector3 forwardVec = new Vector3(0, 0, 1);
        Vector3 targetVec = m_rigidbody.transform.InverseTransformDirection((tar - m_rigidbody.transform.position).normalized);
        //Debug.Log("forward: " + forwardVec);
        //Debug.Log("target: " + targetVec);

        float pitchDelta = distBetweenRads(Mathf.Atan2(targetVec.z, targetVec.x), Mathf.Atan2(forwardVec.z, forwardVec.x));
        pitchDelta = pitchDelta / Mathf.PI;


        input.rudder = pitchDelta;

        //Debug.Log("rudding " + pitchDelta);

        return input;
    }



    private SteeringInput TurnAround(SteeringInput input, Vector3 tar)
    {
        input.acceleration = 1;
        Vector3 forwardVec = new Vector3(0, 0, 1);
        Vector3 targetVec = (tar - m_rigidbody.transform.position);
        //targetVec.y = Mathf.Abs(targetVec.y);
        targetVec.z = 5;
        targetVec = targetVec.normalized;
        Quaternion q = Quaternion.FromToRotation(forwardVec, targetVec);
        Debug.Log(targetVec);

        float deltaX = q.eulerAngles.x;
        deltaX = remapRad(deltaX);
        float deltaY = q.eulerAngles.y;
        deltaY = remapRad(deltaY);
        float deltaZ = q.eulerAngles.z;
        deltaZ = remapRad(deltaZ);

        //Debug.Log("deltax " + deltaX);
        //Debug.Log("deltay " + deltaY);
        //Debug.Log("deltaz " + deltaZ);

        Vector3 adjTarget = targetVec * 100 + m_rigidbody.position;
        //Debug.Log(adjTarget);
        //Debug.Log(targetVec * 100 + m_rigidbody.position);

        input = SeekElevators(lastInput, adjTarget);
        input = SeekRudder(input, adjTarget);
        input = SeekAilerons(input, adjTarget);
        input = holdIncline(input);
        input = holdAltitude(input);


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
