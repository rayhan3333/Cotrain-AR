using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class collisionDetection : MonoBehaviour
{
    // Start is called before the first frame update

    public IKSolverURDF ikScript;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("Collision Entered");
        if (collision.transform == transform.parent)
        {
            return;
        }
        else if (collision.transform == transform.GetChild(2))
        {
            return;
        }
        else if (collision.gameObject.name.Contains("Target"))
        {
            return;
        }
        else
        {
            Debug.Log("Collision detected:" + gameObject.name + " and " + collision.gameObject.name);
            ikScript.collision = true;
        }
    }

    void OnTriggerStay(Collider collision)
    {
        if (collision.transform == transform.parent)
        {
            return;
        }
        else if (collision.transform == transform.GetChild(2))
        {
            return;
        }
        else if (collision.gameObject.name.Contains("Target"))
        {
            return;
        }
        else
        {
            ikScript.collision = true;
        }
    }
}
