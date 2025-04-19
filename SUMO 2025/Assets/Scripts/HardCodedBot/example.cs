using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ExampleBot : Agent
{
    public List<Motor> Motors;
    public List<IRSensor> Sensors;

    public SumoEnvironment Environment { get; set; }
    public int team { get; set; }

    public Vector3 startingPos { get; set; } = default;
    public int startingDirection { get; set; }

    private Vector3 lastPosition;
    private float lastTimeChanged;

    private const float MovementThreshold = 0.01f; // Minimum movement to consider the bot as moving
    private const float IdleTimeout = 5f;          // Time in seconds before considering the bot idle

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

        // Example: Move forward
        continuousActionsOut[0] = 1f;
        continuousActionsOut[1] = 1f;
    }
}