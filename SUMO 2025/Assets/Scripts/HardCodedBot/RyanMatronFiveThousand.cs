
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using UnityEngine;


public class RyanMatronFiveThousand : TrainingAgent
{

    private Vector3 lastPosition;
    private float lastTimeChanged;

    private const float MovementThreshold = 0.01f; // Minimum movement to consider the bot as moving
    private const float IdleTimeout = 5f;          // Time in seconds before considering the bot idle
    private const float MaxDistanceToCharge = 10f; // Maximum distance to charge
    private const float MaxSpeed = 1f;            // Maximum speed

    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;
        lastTimeChanged = Time.time;
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

        lastPosition = transform.position;
        lastTimeChanged = Time.time;
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


        // Check if the bot has moved
        if (Vector3.Distance(lastPosition, transform.position) > MovementThreshold)
        {
            lastPosition = transform.position;
            lastTimeChanged = Time.time;
        }

        // Rewards
        // If the agent has fallen, end the episode
        if (transform.position.y < -1)
        {
            Environment.Reset(team);
        }

        // If the agent is idle for too long, end the episode on a tie (-1)
        if (Time.time >= lastTimeChanged + IdleTimeout)
        {
            Environment.Reset(-1);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        // Sensors:
        // 0: front center IR
        // 1: front left IR
        // 2: front right IR
        // 3: bottom left color
        // 4: bottom right color

        bool WhiteLeft = Sensors[3].GetDistance() == 1; // Assuming 1 indicates white
        bool WhiteRight = Sensors[4].GetDistance() == 1;

        float foundSomethingLeft = Sensors[1].GetDistance();
        float foundSomethingRight = Sensors[2].GetDistance();
        float foundSomethingCenter = Sensors[0].GetDistance();

        // If the robot is on a white line, it should turn around
        if (WhiteLeft)
        {
            continuousActionsOut[0] = 0.5f; // Stop
            continuousActionsOut[1] = 0f;  // Move backward
            return;
        }
        if (WhiteRight)
        {
            continuousActionsOut[0] = 0f;  // Move backward
            continuousActionsOut[1] = 0.5f; // Stop
            return;
        }

        // If we see something ahead, CHASE!
        if (foundSomethingCenter > 0 && foundSomethingCenter < MaxDistanceToCharge)
        {
            continuousActionsOut[0] = MaxSpeed; // Full speed
            continuousActionsOut[1] = MaxSpeed; // Full speed
            return;
        }

        // If we see something on the left, turn right to follow it
        if (foundSomethingLeft > 0 && foundSomethingLeft < MaxDistanceToCharge)
        {
            continuousActionsOut[0] = MaxSpeed; // Full speed
            continuousActionsOut[1] = 0.5f;    // Slow down
            return;
        }

        // If we see something on the right, turn left to follow it
        if (foundSomethingRight > 0 && foundSomethingRight < MaxDistanceToCharge)
        {
            continuousActionsOut[0] = 0.5f;    // Slow down
            continuousActionsOut[1] = MaxSpeed; // Full speed
            return;
        }

        // At this point, we see nothing and are not on a white line, so move forward
        continuousActionsOut[0] = 0.75f; // Moderate speed
        continuousActionsOut[1] = 0.75f; // Moderate speed
    }
}