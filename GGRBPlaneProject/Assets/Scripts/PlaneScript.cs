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
            m_RigidBody.AddForce(gameObject.transform.forward * m_SteeringInput.acceleration, ForceMode.Force);
            calculateCombinedForces();
        }
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

    }
}
