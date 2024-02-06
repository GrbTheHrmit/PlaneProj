using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneScript : MonoBehaviour
{
    GameObject m_body;
    GameObject m_LeftAileron;
    GameObject m_RightAileron;
    GameObject m_LeftElevator;
    GameObject m_RightElevator;
    GameObject m_Rudder;

    private Rigidbody m_RigidBody;
    
    // Rotational forces around center of mass
    private Vector3 calculatedTorque;
    private Vector3 calculatedForce;

    // Start is called before the first frame update
    void Start()
    {
        m_RigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        sendInputs();

        //////
        /// TODO: CALCULATE INTEGRATED VELOCITY ESTIMATE???
        /// //

        if(m_RigidBody != null)
        {
            calculateCombinedForces();
        }
    }

    void sendInputs()
    {

    }

    void calculateCombinedForces()
    {

    }
}
