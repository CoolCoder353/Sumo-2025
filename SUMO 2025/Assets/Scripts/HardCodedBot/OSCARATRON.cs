using System.Collections.Generic;
using UnityEngine;

public class OSCARATRON : MonoBehaviour
{
    public List<Motor> Motors; // Motors controlling the bot
    public List<IRSensor> Sensors; // Sensors for detecting surroundings
    public Transform Enemy; // Reference to the enemy bot

    // Constants for behavior
    private const float MaxDistanceToCharge = 10f;
    private const float MaxSpeed = 0.75f; // Maximum speed
    private const float StopSpeed = 0.5f; // Stop value for motors

    private Vector3 startingPos;
    private float startingDirection;
    public int team;

    public SumoEnvironment environment;


    bool flightmode = false;

    void Start()
    {
        // Initialize starting position and rotation
        startingPos = transform.position;
        startingDirection = transform.eulerAngles.y;

        flightmode = true;


        ResetBot();
    }

    void Update()
    {

        if (transform.position.y < -10)
        {
            Debug.Log("OscarTron has fallen, resetting environment for team: " + team);
            ResetBot();
            environment.Reset(team);

        }

        // Main logic for controlling the bot
        if (Enemy == null)
        {
            FindEnemy();
        }

        if (Enemy != null)
        {
            ChaseEnemy();
        }
        else
        {
            MoveForward();
        }

        CheckForWhiteLine();

    }



    private void ResetBot()
    {
        // Reset position and rotation
        transform.position = startingPos;
        transform.rotation = Quaternion.Euler(0, startingDirection * Random.Range(0, 180), 0);

        // Stop all motors
        foreach (var motor in Motors)
        {
            motor.SetSpeed(0);
        }
    }

    private void FindEnemy()
    {
        // Find the closest enemy in the scene
        var agents = FindObjectsOfType<SumoAgent>();
        foreach (var agent in agents)
        {
            if (agent != transform)
            {
                Enemy = agent.gameObject.transform;
                break;
            }
        }
    }

    private void ChaseEnemy()
    {
        // Chase the enemy
        Vector3 enemyPos = Enemy.position;
        Vector3 myPos = transform.position;

        Vector3 direction = enemyPos - myPos;
        direction.Normalize();

        // Map direction to motor values (0 is backward, 0.5 is stop, 1 is forward)
        float mappedX = direction.x / 2 + 0.5f;
        float mappedY = direction.z / 2 + 0.5f;

        if (flightmode) //If flight mode, dont use the motors and just use the rigidbody
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody not found on the bot.");
                return;
            }

            direction.y = 0;


            //rotate 
            Quaternion targetRotation = Quaternion.LookRotation(direction);


            rb.Move(direction * MaxSpeed * Time.deltaTime + rb.position, targetRotation);



            return;
        }

        Motors[0].SetSpeed(Mathf.Clamp(mappedX, 0f, MaxSpeed));
        Motors[1].SetSpeed(Mathf.Clamp(mappedY, 0f, MaxSpeed));
    }

    private void MoveForward()
    {
        // Move forward at a constant speed
        Motors[0].SetSpeed(MaxSpeed);
        Motors[1].SetSpeed(MaxSpeed);
    }

    private void CheckForWhiteLine()
    {
        // Sensors:
        // 0: front center IR
        // 1: front left IR
        // 2: front right IR
        // 3: bottom left color
        // 4: bottom right color

        bool WhiteLeft = Sensors[3].GetDistance() == 1;
        bool WhiteRight = Sensors[4].GetDistance() == 1;

        if (WhiteLeft)
        {
            Motors[0].SetSpeed(StopSpeed); // Stop
            Motors[1].SetSpeed(0f);       // Move backward
        }
        else if (WhiteRight)
        {
            Motors[0].SetSpeed(0f);       // Move backward
            Motors[1].SetSpeed(StopSpeed); // Stop
        }
    }
}