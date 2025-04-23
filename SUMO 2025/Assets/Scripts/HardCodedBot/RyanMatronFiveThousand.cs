using System.Collections.Generic;
using UnityEngine;

public class RyanMatronFiveThousand : MonoBehaviour
{
    public List<Motor> Motors; // Motors controlling the bot
    public List<IRSensor> Sensors; // Sensors for detecting surroundings

    private Vector3 lastPosition;
    private float lastTimeChanged;

    private const float MovementThreshold = 0.01f; // Minimum movement to consider the bot as moving
    private const float IdleTimeout = 5f;          // Time in seconds before considering the bot idle
    private const float MaxDistanceToCharge = 10f; // Maximum distance to charge
    private const float MaxSpeed = 1f;            // Maximum speed
    private int framesLeft = 0;
    private int framesRight = 0;
    private int framesturn = 0:
    private const int FrameDelay = 100;


    private Vector3 startingPos;
    private float startingDirection;

    void Start()
    {
        // Initialize starting position and rotation
        startingPos = transform.position;
        startingDirection = transform.eulerAngles.y;

        lastPosition = transform.position;
        lastTimeChanged = Time.time;

        ResetBot();
    }

    void Update()
    {
        // Main logic for controlling the bot
        HandleWhiteLineDetection();

        // Check if the bot has moved
        if (Vector3.Distance(lastPosition, transform.position) > MovementThreshold)
        {
            lastPosition = transform.position;
            lastTimeChanged = Time.time;
        }

        // If the bot is idle for too long, reset it
        if (Time.time >= lastTimeChanged + IdleTimeout)
        {
            ResetBot();
        }

        // Behavior logic
        ChaseOrMove();
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

        lastPosition = transform.position;
        lastTimeChanged = Time.time;
    }

    private void HandleWhiteLineDetection()
    {
        bool WhiteLeft = Sensors[3].GetDistance() == 1;
        bool WhiteRight = Sensors[4].GetDistance() == 1;

        if (WhiteLeft || framesLeft > 0)
        {
            if (framesLeft <= 0)
            {
                framesLeft = FrameDelay;
            }
            framesLeft--;
            Motors[0].SetSpeed(0f); // Move backward
            Motors[1].SetSpeed(0f);  // Move backward
        
        }
        else if (WhiteRight || framesRight > 0)
        {
            if (framesRight <= 0)
            {
                framesRight = FrameDelay;
            }
            framesRight--;
            Motors[0].SetSpeed(0f); // Move backward
            Motors[1].SetSpeed(0f);  // Move backward
            
            //180 turn angle 
        
        }
    if(WhiteLeft || WhiteRight)
    {
        if(framesturn <= 0)
                {
                    framesturn = FrameDelay;
                }
                framesturn--;
            Motors[0].SetSpeed(0f); // 180 turn 
            Motors[1].SetSpeed(1f);  // 180 turn
    }
    }

    private void ChaseOrMove()
    {
        float foundSomethingCenter = Sensors[0].GetDistance();
        float foundSomethingLeft = Sensors[1].GetDistance();
        float foundSomethingRight = Sensors[2].GetDistance();

        // If we see something ahead, CHASE!
        if (foundSomethingCenter > 0 && foundSomethingCenter < MaxDistanceToCharge)
        {
            Motors[0].SetSpeed(MaxSpeed); // Full speed
            Motors[1].SetSpeed(MaxSpeed); // Full speed
            return;
        }

        // If we see something on the left, turn right to follow it
        if (foundSomethingLeft > 0 && foundSomethingLeft < MaxDistanceToCharge)
        {
            Motors[0].SetSpeed(MaxSpeed); // Full speed
            Motors[1].SetSpeed(0.5f);    // Slow down
            return;
        }

        // If we see something on the right, turn left to follow it
        if (foundSomethingRight > 0 && foundSomethingRight < MaxDistanceToCharge)
        {
            Motors[0].SetSpeed(0.5f);    // Slow down
            Motors[1].SetSpeed(MaxSpeed); // Full speed
            return;
        }

        // At this point, we see nothing and are not on a white line, so move forward
        Motors[0].SetSpeed(0.75f); // Moderate speed
        Motors[1].SetSpeed(0.75f); // Moderate speed
    }
}