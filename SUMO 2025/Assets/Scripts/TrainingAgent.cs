using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;


public class TrainingAgent : Agent
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
}