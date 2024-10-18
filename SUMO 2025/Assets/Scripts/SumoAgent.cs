using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SumoAgent : Agent
{
    public List<Motor> Motors;
    public List<IRSensor> Sensors;



    public SumoEnvironment Environment { get; set; }
    public int team { get; set; }

    public Vector3 startingPos { get; set; } = default;
    public int startingDirection { get; set; }
    private Transform transform;

    private Vector3 lastPosition;
    private float lastTimeChanged;

    // Start is called before the first frame update
    void Start()
    {
        transform = gameObject.transform;
    }
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

    private IEnumerator LiveingPoints()
    {
        while (true)
        {
            Debug.Log("Added 0.3 points to " + team);
            AddReward(0.3f);
            yield return new WaitForSeconds(1);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var s in Sensors)
        {
            sensor.AddObservation(s.GetDistance());
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        float x = Input.GetAxis("Vertical");
        float y = Input.GetAxis("Horizontal");

        //Normalize the values between 0 - 1
        x = (x + 1) / 2;
        y = (y + 1) / 2;

        continuousActionsOut[0] = x;
        continuousActionsOut[1] = y;
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


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out SumoAgent agent))
        {
            if (agent.team != team)
            {
                Debug.Log("Added 30 points to " + team);
                AddReward(30f);
            }
            else
            {
                Debug.Log("Removed 30 points from " + team);
                AddReward(-30f);
            }
        }

    }


}
