using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField]
    Vector3 rotateObject = Vector3.one;

    [SerializeField]
    float timestep = 0.1f;

    [SerializeField]
    bool globalAxis = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (globalAxis)
        {
            gameObject.transform.eulerAngles += rotateObject * timestep;
        }
        else
        {
            gameObject.transform.localEulerAngles += rotateObject * timestep;
        }  
    }
}
