using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    PlaneScript[] planes;

    public GameObject planeObj;

    int numAI = 5;
    // Start is called before the first frame update
    void Awake()
    {
        planes = new PlaneScript[numAI];

        for(int i = 0; i < numAI; i++)
        {
            GameObject p = Instantiate(planeObj);

            planes[i] = p.GetComponent<PlaneScript>();
        }
        //pilot.setPlane(planes[0]);
        //planes[0].setPilot();
    }

    void Start()
    {
        //Debug.Log("enable");
        SetupPlanes();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 1; i < numAI; i++)
        {
            Vector3 tar = planes[0].gameObject.transform.position;
            tar += planes[0].gameObject.transform.right * 5 * i;
            tar += planes[0].gameObject.transform.up * -2 * i;
            planes[i].setTarget(tar);

        }
    }

    void SetupPlanes()
    {
        planes[0].gameObject.transform.localScale = new Vector3(3, 3, 3);
        for (int i = 0; i < numAI; i++)
        {
            planes[i].gameObject.transform.position = new Vector3(100 + 5 * i, 100 - 5 * i, 100 - 5 * i);
            planes[i].setPilot(ScriptableObject.CreateInstance<AIPilot>());
            if(i != 0)
            {
                Vector3 tar = planes[0].gameObject.transform.position;
                tar += planes[0].gameObject.transform.right * 20 * (i - numAI / 2);
                tar += planes[0].gameObject.transform.up * -10 * Mathf.Abs(i - numAI / 2);
                planes[i].setTarget(tar);
            }
            else
            {
                planes[0].setTarget(new Vector3(500, 500, 500));
            }

        }
    }


}
