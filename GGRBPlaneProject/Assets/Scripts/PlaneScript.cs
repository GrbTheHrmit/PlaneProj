using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

struct SteeringInput
{
    public float acceleration;
    public float leftAileron;
    public float rightAileron;
    public float leftElevator;
    public float rightElevator;
    public float rudder;
}

public class PlaneScript : MonoBehaviour
{
    GameObject m_body;
    PlaneComponent m_LeftAileron;
    PlaneComponent m_RightAileron;
    PlaneComponent m_LeftElevator;
    PlaneComponent m_RightElevator;
    PlaneComponent m_Rudder;

    private Rigidbody m_RigidBody;

    private SteeringInput m_SteeringInput;
    
    // Rotational forces around center of mass
    private Vector3 calculatedTorque;
    private Vector3 calculatedForce;

    private float maxSpeed = 1000;
    private float maxAngSpeed = 12;
    private float maxTorque = 1000;
    private float scaleFactor = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_RigidBody = GetComponent<Rigidbody>();
        if(m_RigidBody != null)
        {
            m_RigidBody.excludeLayers = LayerMask.NameToLayer("PlaneLayer");
        }
        gameObject.layer = LayerMask.NameToLayer("PlaneLayer");

        m_LeftAileron = gameObject.transform.Find("LeftAileron")?.GetComponent<PlaneComponent>();
        m_RightAileron = gameObject.transform.Find("RightAileron")?.GetComponent<PlaneComponent>();
        m_LeftElevator = gameObject.transform.Find("LeftElevator")?.GetComponent<PlaneComponent>();
        m_RightElevator = gameObject.transform.Find("RightElevator")?.GetComponent<PlaneComponent>();
        m_Rudder = gameObject.transform.Find("Rudder")?.GetComponent<PlaneComponent>();
        if (m_Rudder != null) m_Rudder.setRudder();

        calculatedForce = Vector3.zero;
        calculatedTorque = Vector3.zero;

    }

    // Update is called once per frame
    void Update()
    {
        //foreach(Button b : this.gameObject.in)
        updateInputs();
        sendInputs();

        //////
        /// TODO: CALCULATE INTEGRATED VELOCITY ESTIMATE???
        /// //

        if(m_RigidBody != null)
        {
            calculatedForce += gameObject.transform.forward * m_SteeringInput.acceleration;
            calculateCombinedForces();

            if (m_RigidBody.velocity.magnitude < maxSpeed * scaleFactor || Vector3.Dot(m_RigidBody.velocity, calculatedForce) < 0)
            {
                m_RigidBody.AddForce(calculatedForce * scaleFactor, ForceMode.Force);
                //Debug.Log("Magnitude: " + m_RigidBody.velocity);
                //Debug.Log("Force: " + calculatedForce);
            }
            if (m_RigidBody.angularVelocity.magnitude < maxAngSpeed * scaleFactor * 0.1f || Vector3.Dot(m_RigidBody.angularVelocity, calculatedTorque) < 0)
            {
                if(calculatedTorque.magnitude >= maxTorque)
                {
                    calculatedTorque = calculatedTorque.normalized * maxTorque;
                }

                m_RigidBody.AddTorque(calculatedTorque * scaleFactor * 0.05f, ForceMode.Force);
                //Debug.Log("Magnitude: " + m_RigidBody.angularVelocity + "\nTorque: " + calculatedTorque);
            }

        }
        calculatedForce = Vector3.zero;
        calculatedTorque = Vector3.zero;
    }

    // TODO update this for AI input and more intuitive controls 
    void updateInputs()
    {
        m_SteeringInput.acceleration = Input.GetAxis("Thrust");// - Input.GetAxis("ReverseThrust");
        m_SteeringInput.rightAileron = -Input.GetAxis("RightAileron");
        m_SteeringInput.leftAileron = -Input.GetAxis("LeftAileron");
        m_SteeringInput.rightElevator = Input.GetAxis("RightElevator");//(Input.GetButton("RightElevatorUp") ? 1 : 0) + (Input.GetButton("RightElevatorDown") ? -1 : 0);
        m_SteeringInput.leftElevator = Input.GetAxis("LeftElevator");//(Input.GetButton("LeftElevatorUp") ? 1 : 0) + (Input.GetButton("LeftElevatorDown") ? -1 : 0);
        m_SteeringInput.rudder = -Input.GetAxis("Rudder");//(Input.GetButton("RightRudder") ? 1 : 0) + (Input.GetButton("LeftRudder") ? -1 : 0);

        /*
        if (m_SteeringInput.acceleration > 0.5f) Debug.Log("Thrust\n");
        else if (m_SteeringInput.acceleration < -0.5f) Debug.Log("RevThrust\n");

        if (m_SteeringInput.rightAileron > 0.5f) Debug.Log("rightAileron\n");
        else if (m_SteeringInput.rightAileron < -0.5f) Debug.Log("RevrightAileron\n");

        if (m_SteeringInput.leftAileron > 0.5f) Debug.Log("leftAileron\n");
        else if (m_SteeringInput.leftAileron < -0.5f) Debug.Log("RevleftAileron\n");

        if (m_SteeringInput.rightElevator > 0.5f) Debug.Log("rightElevator\n");
        else if (m_SteeringInput.rightElevator < -0.5f) Debug.Log("RevrightElevator\n");

        if (m_SteeringInput.leftElevator > 0.5f) Debug.Log("leftElevator\n");
        else if (m_SteeringInput.leftElevator < -0.5f) Debug.Log("RevleftElevator\n");

        if (m_SteeringInput.rudder > 0.5f) Debug.Log("Rudder\n");
        else if (m_SteeringInput.rudder < -0.5f) Debug.Log("RevRudder\n");
        */
        
    }

    void sendInputs()
    {
        if (m_LeftAileron != null)  m_LeftAileron.handleInput(m_SteeringInput.leftAileron);
        if (m_RightAileron != null) m_RightAileron.handleInput(m_SteeringInput.rightAileron);
        if (m_LeftElevator != null) m_LeftElevator.handleInput(m_SteeringInput.leftElevator);
        if (m_RightElevator != null) m_RightElevator.handleInput(m_SteeringInput.rightElevator);
        if (m_Rudder != null) m_Rudder.handleInput(m_SteeringInput.rudder);

    }

    void calculateCombinedForces()
    {
        Vector3 force = Vector3.zero;
        Vector3 pos = Vector3.zero;

        if (m_LeftAileron != null)
        {
            m_LeftAileron.getForces(ref force, ref pos);
            calculatedForce += gameObject.transform.rotation * force;
            Vector3 torqueComp = Vector3.Cross(force, pos);
            torqueComp.x = 0;
            torqueComp.y = 0;
            calculatedTorque += gameObject.transform.rotation * torqueComp;
        }

        if (m_RightAileron != null)
        {
            m_RightAileron.getForces(ref force, ref pos);
            calculatedForce += gameObject.transform.rotation * force;
            Vector3 torqueComp = Vector3.Cross(force, pos);
            torqueComp.x = 0;
            torqueComp.y = 0;
            calculatedTorque += gameObject.transform.rotation * Vector3.Cross(force, pos);
        }

        if (m_LeftElevator != null)
        {
            m_LeftElevator.getForces(ref force, ref pos);
            calculatedForce += gameObject.transform.rotation * force;
            Vector3 torqueComp = Vector3.Cross(force, pos);
            torqueComp.x = 0;
            calculatedTorque += gameObject.transform.rotation * Vector3.Cross(force, pos);
        }

        if (m_RightElevator != null)
        {
            m_RightElevator.getForces(ref force, ref pos);
            calculatedForce += gameObject.transform.rotation * force;
            Vector3 torqueComp = Vector3.Cross(force, pos);
            torqueComp.x = 0;
            calculatedTorque += gameObject.transform.rotation * Vector3.Cross(force, pos);
        }

        if (m_Rudder != null)
        {
            m_Rudder.getForces(ref force, ref pos);
            calculatedForce += gameObject.transform.rotation * force;
            pos.y = 0;
            Vector3 torqueComp = Vector3.Cross(force, pos) * 100;
            //torqueComp.z = 0;
            calculatedTorque += gameObject.transform.rotation * Vector3.Cross(force, pos);
        }
    }
}
