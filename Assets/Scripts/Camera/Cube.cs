using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 forwardVec = Camera.main.transform.forward;
                Vector3 crossProduct = Vector3.Cross(raycast.direction, forwardVec);
                gameObject.transform.Rotate(crossProduct,Time.deltaTime * 100, Space.World);
            }
        }
    }
}
