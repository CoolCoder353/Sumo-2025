using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class OSCARATRON : Agent
{

    public List<Motor> Motors;
    public List<IRSensor> Sensors;



    public SumoEnvironment Environment { get; set; }
    public int team { get; set; }

    public Vector3 startingPos { get; set; } = default;
    public int startingDirection { get; set; }
    private Transform transform;

    private Agent Enemy;

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

        //Get the enemy
        //GET all the Agents in the scene that have the same parent as this agent
        var agents = FindObjectsOfType<Agent>();

        Enemy = null;

        foreach (var a in agents)
        {
            if (a.transform.parent == transform.parent && a != this)
            {
                enemy = a;
                break;
            }
        }

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

        //Constants -> needs to be calibrated
        float maxDistanceToCharge = 10f;
        float maxSpeed = 100f;


        //If the robot is on a white line, it should turn around
        if (WhiteLeft)
        {
            continuousActionsOut[0] = 0.5f; //stops
            continuousActionsOut[1] = 0f; //backwards
            return;
        }
        if (WhiteRight)
        {
            continuousActionsOut[0] = 0f; //backwards
            continuousActionsOut[1] = 0.5f; //stops
            return;
        }

        if (Enemy == null)
        {
            //Just move forward
            continuousActionsOut[0] = 1;
            continuousActionsOut[1] = 1;
            return
        }

        //At this point we know who the enemy is so lets chase it
        Vector3 enemyPos = Enemy.transform.position;
        Vector3 myPos = transform.position;

        Vector3 direction = enemyPos - myPos;

        //Normalise
        direction.Normalize();

        //Go to it (remember that 0 is backwards, 0.5 is stop, 1 is forwards)
        float mappedX = direction.x / 2 + 0.5f;
        float mappedY = direction.z / 2 + 0.5f;

        continuousActionsOut[0] = mappedX;
        continuousActionsOut[1] = mappedY;






    }
}