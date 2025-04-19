using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FlankAndSpankBot : TrainingAgent
{


    private int attackMode = 0; // Default attack mode
    private float maxDistanceToCharge = 5.0f; // Example value
    private float maxSpeed = 1.0f; // Maximum speed
    private float turnSpeed = 0.5f; // Turning speed
    private int framesLeft = 0;
    private int framesRight = 0;
    private const int FrameDelay = 100;

    // Start is called before the first frame update
    void Start()
    {
        // Initialization logic
    }

    /// <summary>
    /// DO NOT CHANGE UNDER ANY CIRCUMSTANCES
    /// </summary>
    public override void OnEpisodeBegin()
    {
        if (startingPos != default) { transform.position = startingPos; }
        foreach (var m in Motors)
        {
            m.SetSpeed(0);
        }
        transform.rotation = Quaternion.Euler(0, startingDirection * Random.Range(0, 180), 0);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float x = actionBuffers.ContinuousActions[0];
        float y = actionBuffers.ContinuousActions[1];

        //Normalize the values between (-1) - (+1)
        x = (x - 0.5f) * 2;
        y = (y - 0.5f) * 2;
        Motors[0].SetSpeed(x);
        Motors[1].SetSpeed(y);


        // Rewards
        if (transform.position.y < -1)
        {
            Environment.Reset(team);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        bool WhiteLeft = Sensors[3].GetDistance() == 1;
        bool WhiteRight = Sensors[4].GetDistance() == 1;

        float foundSomethingLeft = Sensors[1].GetDistance();
        float foundSomethingRight = Sensors[2].GetDistance();
        float foundSomethingCenter = Sensors[0].GetDistance();

        // Handle white line detection
        if (WhiteLeft || framesLeft > 0)
        {
            if (framesLeft <= 0)
            {
                framesLeft = FrameDelay;
            }
            framesLeft--;
            continuousActionsOut[0] = 0.5f; // Stop
            continuousActionsOut[1] = 0f;  // Move backward
            return;
        }
        else if (WhiteRight || framesRight > 0)
        {
            if (framesRight <= 0)
            {
                framesRight = FrameDelay;
            }
            framesRight--;
            continuousActionsOut[0] = 0.5f; // Stop
            continuousActionsOut[1] = 0f;  // Move backward
            return;
        }

        // Attack modes
        switch (attackMode)
        {
            case 0: // Aggressive Charge
                if (foundSomethingCenter > 0 && foundSomethingCenter < maxDistanceToCharge)
                {
                    continuousActionsOut[0] = maxSpeed;
                    continuousActionsOut[1] = maxSpeed;
                }
                else
                {
                    continuousActionsOut[0] = 0.75f;
                    continuousActionsOut[1] = 0.75f;
                }
                break;

            case 1: // Flank & Attack
                if (foundSomethingLeft > 0 && foundSomethingLeft < maxDistanceToCharge)
                {
                    continuousActionsOut[0] = maxSpeed;
                    continuousActionsOut[1] = turnSpeed;
                }
                else if (foundSomethingRight > 0 && foundSomethingRight < maxDistanceToCharge)
                {
                    continuousActionsOut[0] = turnSpeed;
                    continuousActionsOut[1] = maxSpeed;
                }
                else
                {
                    continuousActionsOut[0] = 0.75f;
                    continuousActionsOut[1] = 0.75f;
                }
                break;

            case 2: // Evasive Counter
                if (foundSomethingCenter > 0 && foundSomethingCenter < maxDistanceToCharge)
                {
                    continuousActionsOut[0] = 0.5f;
                    continuousActionsOut[1] = 0f;
                }
                else if (foundSomethingLeft > 0 && foundSomethingLeft < maxDistanceToCharge)
                {
                    continuousActionsOut[0] = 0f;
                    continuousActionsOut[1] = 1f;
                }
                else if (foundSomethingRight > 0 && foundSomethingRight < maxDistanceToCharge)
                {
                    continuousActionsOut[0] = 1f;
                    continuousActionsOut[1] = 0f;
                }
                else
                {
                    continuousActionsOut[0] = 0.75f;
                    continuousActionsOut[1] = 0.75f;
                }
                break;
        }
    }
}