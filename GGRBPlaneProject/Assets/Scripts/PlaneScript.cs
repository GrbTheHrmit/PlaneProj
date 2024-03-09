using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct SteeringInput
{
    public SteeringInput(float accel, float lA, float rA, float lE, float rE, float rud)
    {
        acceleration = accel;
        leftAileron = lA;
        rightAileron = rA; 
        leftElevator = lE;
        rightElevator = rE;
        rudder = rud;
    }

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
    private bool usePlayerInputs = false;
    private AIPilot aiPilot;
    
    // Rotational forces around center of mass
    private Vector3 calculatedTorque;
    private Vector3 calculatedForce;

    private float maxSpeed = 1000;
    private float maxAngSpeed = 12;
    private float maxTorque = 1000;
    private float scaleFactor = 10.0f;
    private float maxThrust = 50.0f;

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
        if(m_LeftAileron != null )
        {
            m_LeftAileron.setSurfaceArea(50);
            m_LeftAileron.setUpperRadius(10);
            m_LeftAileron.setLowerRadius(10);
            m_LeftAileron.setWingspan(8);
            m_LeftAileron.setTorqueScale(0.2f);
        }

        m_RightAileron = gameObject.transform.Find("RightAileron")?.GetComponent<PlaneComponent>();
        if (m_RightAileron != null)
        {
            m_RightAileron.setSurfaceArea(50);
            m_RightAileron.setUpperRadius(10);
            m_RightAileron.setLowerRadius(10);
            m_RightAileron.setWingspan(8);
            m_RightAileron.setTorqueScale(0.2f);
        }
        m_LeftElevator = gameObject.transform.Find("LeftElevator")?.GetComponent<PlaneComponent>();
        if (m_LeftElevator != null)
        {
            m_LeftElevator.setSurfaceArea(10);
            m_LeftElevator.setUpperRadius(10);
            m_LeftElevator.setLowerRadius(10);
            m_LeftElevator.setWingspan(2);
        }

        m_RightElevator = gameObject.transform.Find("RightElevator")?.GetComponent<PlaneComponent>();
        if (m_RightElevator != null)
        {
            m_RightElevator.setSurfaceArea(10);
            m_RightElevator.setUpperRadius(10);
            m_RightElevator.setLowerRadius(10);
            m_RightElevator.setWingspan(2);
        }

        m_Rudder = gameObject.transform.Find("Rudder")?.GetComponent<PlaneComponent>();
        if (m_Rudder != null)
        {
            m_Rudder.setRudder();
            m_Rudder.setSurfaceArea(1.5f);
            m_Rudder.setUpperRadius(10);
            m_Rudder.setLowerRadius(10);
            m_Rudder.setWingspan(1);
            m_Rudder.setTorqueScale(5);
        }

        calculatedForce = Vector3.zero;
        calculatedTorque = Vector3.zero;

        aiPilot = new AIPilot();
        aiPilot.setPlane(this);
        aiPilot.setRigidBody(m_RigidBody);
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
        //Debug.Log(Time.deltaTime);

        if(m_RigidBody != null)
        {
            calculatedForce += gameObject.transform.forward * m_SteeringInput.acceleration * maxThrust;
            calculateCombinedForces();

            calculatedForce *= 0.1f;
            calculatedTorque *= 1.0f;

            if (m_RigidBody.velocity.magnitude < maxSpeed * scaleFactor || Vector3.Dot(gameObject.transform.InverseTransformDirection(m_RigidBody.velocity), calculatedForce) < 0)
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

                m_RigidBody.AddRelativeTorque(calculatedTorque * scaleFactor * 0.05f, ForceMode.Force);
                //Debug.Log("Magnitude: " + m_RigidBody.angularVelocity + "\nTorque: " + calculatedTorque);
            }

        }

        //Debug.Log(m_RigidBody.velocity);
        if (m_RigidBody != null && gameObject.transform.InverseTransformDirection(m_RigidBody.velocity).z < 0) 
        {
            m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, m_RigidBody.velocity.y, 0);
        }

        calculatedForce = Vector3.zero;
        calculatedTorque = Vector3.zero;

        
        
        //Debug.Log(m_RigidBody.velocity);
    }

    public void setPilot(AIPilot pilot)
    {
        aiPilot = pilot;
        aiPilot.setPlane(this);
    }

    // TODO update this for AI input and more intuitive controls 
    void updateInputs()
    {
        if(usePlayerInputs)
        {
            m_SteeringInput.acceleration = Input.GetAxis("Thrust");// - Input.GetAxis("ReverseThrust");
            m_SteeringInput.rightAileron = -Input.GetAxis("RightAileron");
            m_SteeringInput.leftAileron = -Input.GetAxis("LeftAileron");
            m_SteeringInput.rightElevator = Input.GetAxis("RightElevator");//(Input.GetButton("RightElevatorUp") ? 1 : 0) + (Input.GetButton("RightElevatorDown") ? -1 : 0);
            m_SteeringInput.leftElevator = Input.GetAxis("LeftElevator");//(Input.GetButton("LeftElevatorUp") ? 1 : 0) + (Input.GetButton("LeftElevatorDown") ? -1 : 0);
            m_SteeringInput.rudder = -Input.GetAxis("Rudder");//(Input.GetButton("RightRudder") ? 1 : 0) + (Input.GetButton("LeftRudder") ? -1 : 0);
        }
        else if(aiPilot != null)
        {
            m_SteeringInput = aiPilot.getSteering(Time.deltaTime);
            //Debug.Log(m_SteeringInput.acceleration);
        }
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
            //pos.y = 0;
            pos.z = 0;
            //Vector3 torqueComp = Vector3.Cross(force, pos);
            //torqueComp.x = 0;
            //torqueComp.y = 0;
            calculatedTorque += Vector3.Cross(force, pos);
        }

        if (m_RightAileron != null)
        {
            m_RightAileron.getForces(ref force, ref pos);
            calculatedForce += gameObject.transform.rotation * force;
            //pos.y = 0;
            pos.z = 0;
            //Vector3 torqueComp = Vector3.Cross(force, pos);
            //torqueComp.x = 0;
            //torqueComp.y = 0;
            calculatedTorque += Vector3.Cross(force, pos);
        }

        if (m_LeftElevator != null)
        {
            m_LeftElevator.getForces(ref force, ref pos);
            calculatedForce += gameObject.transform.rotation * force;
            //pos.y = 0;
            //Vector3 torqueComp = Vector3.Cross(force, pos);
            //torqueComp.x = 0;
            calculatedTorque += Vector3.Cross(force, pos);
        }

        if (m_RightElevator != null)
        {
            m_RightElevator.getForces(ref force, ref pos);
            calculatedForce += gameObject.transform.rotation * force;
            //pos.y = 0;
            //Vector3 torqueComp = Vector3.Cross(force, pos);
            //torqueComp.x = 0;
            calculatedTorque += Vector3.Cross(force, pos);
        }

        if (m_Rudder != null)
        {
            m_Rudder.getForces(ref force, ref pos);
            calculatedForce += gameObject.transform.rotation * force;
            //pos.x = 0;
            pos.y = 0;
            //Vector3 torqueComp = Vector3.Cross(force, pos) * 100;
            //torqueComp.z = 0;
            calculatedTorque += Vector3.Cross(force, pos);
        }
    }
}
