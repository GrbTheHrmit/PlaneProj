using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    PlaneScript[] planes;

    public GameObject planeObj;

    int numAI = 1;

    // Start is called before the first frame update
    void Start()
    {
        planes = new PlaneScript[numAI];

        GameObject p = Instantiate(planeObj);

        planes[0] = p.GetComponent<PlaneScript>() ;
        planes[0].gameObject.transform.position = new Vector3 (0, 100, 0);
        //pilot.setPlane(planes[0]);
        //planes[0].setPilot();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
