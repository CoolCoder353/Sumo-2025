using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class OSCARATRON : TrainingAgent
{
    private Agent Enemy;

    // Constants for behavior
    private const float MaxDistanceToCharge = 10f;
    private const float MaxSpeed = 1f; // Adjusted to ML-Agents normalized range
    private const float StopSpeed = 0.5f; // Stop value for motors

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

        // Get the enemy
        Enemy = null;
        var agents = FindObjectsOfType<Agent>();
        foreach (var a in agents)
        {
            if (a.transform.parent == transform.parent && a != this)
            {
                Enemy = a;
                break;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var s in Sensors)
        {
            sensor.AddObservation(s.GetDistance());
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Debug.Log(actionBuffers.ContinuousActions[0]);
        Debug.Log(actionBuffers.ContinuousActions[1]);

        float x = actionBuffers.ContinuousActions[0];
        float y = actionBuffers.ContinuousActions[1];

        //Normalize the values between (-1) - (+1)
        x = (x - 0.5f) * 2;
        y = (y - 0.5f) * 2;
        Motors[0].SetSpeed(x);
        Motors[1].SetSpeed(y);


        // Rewards
        // If the agent has fallen, end the episode
        if (transform.position.y < -1)
        {
            Environment.Reset(team);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log("Heuristic called");
        var continuousActionsOut = actionsOut.ContinuousActions;


        // Sensors:
        // 0: front center IR
        // 1: front left IR
        // 2: front right IR
        // 3: bottom left color
        // 4: bottom right color

        bool WhiteLeft = Sensors[3].GetDistance() == 1;
        bool WhiteRight = Sensors[4].GetDistance() == 1;

        float foundSomethingLeft = Sensors[1].GetDistance();
        float foundSomethingRight = Sensors[2].GetDistance();
        float foundSomethingCenter = Sensors[0].GetDistance();

        Debug.Log($"WhiteLeft: {WhiteLeft}, WhiteRight: {WhiteRight}, foundSomethingLeft: {foundSomethingLeft}, foundSomethingRight: {foundSomethingRight}, foundSomethingCenter: {foundSomethingCenter}");

        // If the robot is on a white line, it should turn around
        if (WhiteLeft)
        {
            continuousActionsOut[0] = StopSpeed; // Stop
            continuousActionsOut[1] = 0f;       // Move backward
            return;
        }
        if (WhiteRight)
        {
            continuousActionsOut[0] = 0f;       // Move backward
            continuousActionsOut[1] = StopSpeed; // Stop
            return;
        }

        // If no enemy is detected, move forward
        if (Enemy == null)
        {
            continuousActionsOut[0] = MaxSpeed;
            continuousActionsOut[1] = MaxSpeed;
            return;
        }

        // Chase the enemy
        Vector3 enemyPos = Enemy.transform.position;
        Vector3 myPos = transform.position;

        Vector3 direction = enemyPos - myPos;
        direction.Normalize();

        // Map direction to motor values (0 is backward, 0.5 is stop, 1 is forward)
        float mappedX = direction.x / 2 + 0.5f;
        float mappedY = direction.z / 2 + 0.5f;

        continuousActionsOut[0] = Mathf.Clamp(mappedX, 0f, 1f);
        continuousActionsOut[1] = Mathf.Clamp(mappedY, 0f, 1f);

        Debug.Log($"Chasing enemy at {enemyPos} with direction {direction} and actionsOut values: {continuousActionsOut[0]}, {continuousActionsOut[1]}");

    }
}