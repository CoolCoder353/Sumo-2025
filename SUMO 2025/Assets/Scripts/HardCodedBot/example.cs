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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        //This is where your robot logic will be

        //Example: Move forward
        continuousActionsOut[0] = 1;
        continuousActionsOut[1] = 1;
    }
}