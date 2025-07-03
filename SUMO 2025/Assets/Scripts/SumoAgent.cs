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
    private Quaternion lastRotation;
    private float lastTimeChanged;

    private float startTime;

    // Start is called before the first frame update
    void Start()
    {
        transform = gameObject.transform;
    }
    public override void OnEpisodeBegin()
    {
        if (transform == null)
        {
            return;
        }

        if (startingPos != default) { transform.position = startingPos; }
        foreach (var m in Motors)
        {
            m.SetSpeed(0);
        }
        transform.rotation = Quaternion.Euler(0, startingDirection * Random.Range(0, 180), 0);
        startTime = Time.time;
        StartCoroutine(LiveingPoints());
    }

    private IEnumerator LiveingPoints()
    {
        while (true)
        {
            // Add a small rewards for being alive within the first 15 seconds
            if (Time.time < startTime + 15)
            {
                AddReward(0.01f);
            }
            else if (Time.time >= startTime + 3 * 60)
            {
                Debug.Log("Timeout, resetting environment for team: " + team);
                Environment.Reset(team);
            }
            else //Punish the bot for not winning yet, exponentially
            {
                // Exponential decay of reward over time
                float timeAlive = Time.time - startTime;
                float reward = -1 / 100 * timeAlive * timeAlive + 10;
                AddReward(reward);
            }
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
        float nx = (x - 0.5f) * 2;
        float ny = (y - 0.5f) * 2;

        Debug.LogWarning($"Action received x = {x}, y = {y}, normalized to nx = {nx}, ny = {ny}");

        Motors[0].SetSpeed(nx);
        Motors[1].SetSpeed(ny);

        //Check if we have changed pos (plus or minus a little bit)
        if (Vector3.Distance(lastPosition, transform.position) > 0.1f)
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;
            lastTimeChanged = Time.time;
        }



        //Rewards
        //If the agent has fallen, end the episode
        if (transform.position.y < -1) //|| transform.position.y > 0.1)
        {
            Debug.Log("Agent has fallen, resetting environment for team: " + team);
            Environment.Reset(team);
        }

        //If the agent is not moving for 5 seconds, end the episode on a tie (-1)
        if (Time.time >= lastTimeChanged + 5)
        {
            Debug.Log("Agent has not moved for 5 seconds, resetting environment for team: " + team);
            Environment.Reset(team);
        }

        //If the agent is spinning around in circles, punish it LOTS
        if (Mathf.Abs(transform.rotation.eulerAngles.y - lastRotation.eulerAngles.y) > 180)
        {
            Debug.Log("Spinning around in circles, punishing agent");
            AddReward(-50f);
        }



    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out SumoAgent agent))
        {
            if (agent.team != team)
            {
                ////Debug.Log("Added 30 points to " + team);
                AddReward(250);
            }
        }
        else if (collision.gameObject.TryGetComponent(out OSCARATRON bot))
        {

            //Debug.Log("Added 30 points to " + team);
            AddReward(250);

        }

    }


}
