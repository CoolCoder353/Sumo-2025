using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class IRSensor : MonoBehaviour
{
    public LayerMask layerMask;

    public float MaxDistance = 30;
    [Tooltip("If true, the sensor will return the distance to the object hit. Else, it will return 1 if hit, 0 otherwise.")]
    public bool ReturnDistance = true;

    public bool reverseOutput = false;

    public bool DrawRay = true;
    public float drawInterval { get; internal set; } = 0.1f; //Draw every x seconds

    private float lastTimeDrawn = 0;

    public float lastResult = 0;
    // Update is called once per frame
    void Update()
    {
        Draw();
    }



    public void Draw()
    {
        if (DrawRay)
        {
            if (Time.time - lastTimeDrawn > drawInterval)
            {
                //Update the ray
                lastTimeDrawn = Time.time;
                lastResult = GetDistance();
            }
            //Draw white if no hit, red if hit
            Debug.DrawRay(transform.position, transform.forward * lastResult, lastResult < MaxDistance ? Color.white : Color.red);
        }
    }

    public float GetDistance()
    {
        if (ReturnDistance)
        {
            return InternalGetDistance();
        }
        else
        {
            if (reverseOutput)
                return InternalGetDistance() < MaxDistance ? 0 : 1;
            else
            {
                return InternalGetDistance() < MaxDistance ? 1 : 0;
            }

        }
    }

    private float InternalGetDistance()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, MaxDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.distance;
        }
        return MaxDistance;
    }


}
