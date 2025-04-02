using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FlankAndSpankBot : Agent
{

    public List<Motor> Motors;
    public List<IRSensor> Sensors;4



    public SumoEnvironment Environment { get; set; }
    public int team { get; set; }

    public Vector3 startingPos { get; set; } = default;
    public int startingDirection { get; set; }
    private Transform transform;

    // Start is called before the first frame update
    void Start()
    {

        transform = gameObject.transform;
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
        StartCoroutine(LiveingPoints());
    }

    public void OnActionReceived(ActionBuffers actionBuffers)
    {
        float x = actionBuffers.ContinuousActions[0];
        float y = actionBuffers.ContinuousActions[1];

        //Normalize the values between (-1) - (+1)
        x = (x - 0.5f) * 2;
        y = (y - 0.5f) * 2;
        Motors[0].SetSpeed(x);
        Motors[1].SetSpeed(y);

        //Check if we have changed pos (plus or minus a little bit)
        if (Vector3.Distance(lastPosition, transform.position) > 0.01f)
        {
            lastPosition = transform.position;
            lastTimeChanged = Time.time;
        }



        //Rewards
        //If the agent has fallen, end the episode
        if (transform.position.y < -1)
        {
            Environment.Reset(team);
        }

        //If the agent is not moving for 5 seconds, end the episode on a tie (-1)
        if (Time.time >= lastTimeChanged + 5)
        {
            Environment.Reset(-1);
        }

    }

       int framesLeft = 0;
        int framesRight = 0;
        int FrameDelay = 100;

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        //This is where your robot logic will be

        // Left Motor: continuousActionsOut[0]
        // Right Motor: continuousActionsOut[1]

        //0 is full reverse, 0.5 is stop, 1 is full forward

        //Example: Move forward


        //Sensors :
        // 0: front center IR
        // 1: front left IR
        // 2: front right IR
        // 3: bottom left colour
        // 4: bottom right colour

        bool WhiteLeft = Sensors[3].GetDistance();
        bool WhiteRight = Sensors[4].GetDistance();

        float foundSomethingLeft = Sensors[1].GetDistance();
        float foundSomethingRight = Sensors[2].GetDistance();
        float foundSomethingCenter = Sensors[0].GetDistance();

     

        // If the robot is on a white line, it should turn around
        if (WhiteLeft || framesLeft > 0)
        {
            if(framesLeft <= 0)
            {
                framesLeft = FrameDelay;
            }
            framesLeft--; // Set frames to FrameDelay only when it's the first detection
            continuousActionsOut[0] = 0.5f; // stops
            continuousActionsOut[1] = 0f; // move backwards
            return;
        }
        else if (WhiteRight || framesRight > 0) 
        {
            if(framesRight <= 0)
            {
                framesRight = FrameDelay;
            }
            framesRight--;
            continuousActionsOut[0] = 0.5f; // stops
            continuousActionsOut[1] = 0f; // move backwards
            return;
        }
        

        switch (attackMode)
{
    case 0: // Aggressive Charge
        if (foundSomethingCenter > 0 && foundSomethingCenter < maxDistanceToCharge)
        {
            // Charge at the enemy with max speed
            continuousActionsOut[0] = maxSpeed;
            continuousActionsOut[1] = maxSpeed;
        }
        else
        {
            // If no enemy is detected, move forward at 75% speed
            continuousActionsOut[0] = 0.75f;
            continuousActionsOut[1] = 0.75f;
        }
        break;

    case 1: // Flank & Attack
        if (foundSomethingLeft > 0 && foundSomethingLeft < maxDistanceToCharge)
        {
            // Enemy detected on the left, move to the left while attacking
            continuousActionsOut[0] = maxSpeed;
            continuousActionsOut[1] = turnSpeed;
        }
        else if (foundSomethingRight > 0 && foundSomethingRight < maxDistanceToCharge)
        {
            // Enemy detected on the right, move to the right while attacking
            continuousActionsOut[0] = turnSpeed;
            continuousActionsOut[1] = maxSpeed;
        }
        else
        {
            // If no enemy is detected, move forward at 75% speed
            continuousActionsOut[0] = 0.75f;
            continuousActionsOut[1] = 0.75f;
        }
        break;

    case 2: // Evasive Counter
        if (foundSomethingCenter > 0 && foundSomethingCenter < maxDistanceToCharge)
        {
            // Evasive maneuver when close to the enemy
            continuousActionsOut[0] = 0.5f; // slow down and reverse
            continuousActionsOut[1] = 0f;
        }
        else if (foundSomethingLeft > 0 && foundSomethingLeft < maxDistanceToCharge)
        {
            // Move around to avoid left-side attack
            continuousActionsOut[0] = 0f;
            continuousActionsOut[1] = 1f;
        }
        else if (foundSomethingRight > 0 && foundSomethingRight < maxDistanceToCharge)
        {
            // Move around to avoid right-side attack
            continuousActionsOut[0] = 1f;
            continuousActionsOut[1] = 0f;
        }
        else
        {
            // No threat, move forward at 75% speed
            continuousActionsOut[0] = 0.75f;
            continuousActionsOut[1] = 0.75f;
        }
        break;
}
}