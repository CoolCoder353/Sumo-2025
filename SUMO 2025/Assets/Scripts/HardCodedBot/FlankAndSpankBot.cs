using System.Collections.Generic;
using UnityEngine;

public class FlankAndSpankBot : MonoBehaviour
{
    public List<Motor> Motors; // Motors controlling the bot
    public List<IRSensor> Sensors; // Sensors for detecting surroundings

    private int attackMode = 0; // Default attack mode
    private float maxDistanceToCharge = 5.0f; // Example value
    private float maxSpeed = 1.0f; // Maximum speed
    private float turnSpeed = 0.5f; // Turning speed
    private int framesLeft = 0;
    private int framesRight = 0;
    private const int FrameDelay = 100;

    private Vector3 startingPos;
    private float startingDirection;

    void Start()
    {
        // Initialize starting position and rotation
        startingPos = transform.position;
        startingDirection = transform.eulerAngles.y;

        ResetBot();
    }

    void Update()
    {
        // Main logic for controlling the bot
        HandleWhiteLineDetection();

        switch (attackMode)
        {
            case 0:
                AggressiveCharge();
                break;
            case 1:
                FlankAndAttack();
                break;
            case 2:
                EvasiveCounter();
                break;
        }
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
            Motors[0].SetSpeed(0.5f); // Stop
            Motors[1].SetSpeed(0f);  // Move backward
            return;
        }
        else if (WhiteRight || framesRight > 0)
        {
            if (framesRight <= 0)
            {
                framesRight = FrameDelay;
            }
            framesRight--;
            Motors[0].SetSpeed(0.5f); // Stop
            Motors[1].SetSpeed(0f);  // Move backward
            return;
        }
    }

    private void AggressiveCharge()
    {
        float foundSomethingCenter = Sensors[0].GetDistance();

        if (foundSomethingCenter > 0 && foundSomethingCenter < maxDistanceToCharge)
        {
            Motors[0].SetSpeed(maxSpeed);
            Motors[1].SetSpeed(maxSpeed);
        }
        else
        {
            Motors[0].SetSpeed(0.75f);
            Motors[1].SetSpeed(0.75f);
        }
    }

    private void FlankAndAttack()
    {
        float foundSomethingLeft = Sensors[1].GetDistance();
        float foundSomethingRight = Sensors[2].GetDistance();

        if (foundSomethingLeft > 0 && foundSomethingLeft < maxDistanceToCharge)
        {
            Motors[0].SetSpeed(maxSpeed);
            Motors[1].SetSpeed(turnSpeed);
        }
        else if (foundSomethingRight > 0 && foundSomethingRight < maxDistanceToCharge)
        {
            Motors[0].SetSpeed(turnSpeed);
            Motors[1].SetSpeed(maxSpeed);
        }
        else
        {
            Motors[0].SetSpeed(0.75f);
            Motors[1].SetSpeed(0.75f);
        }
    }

    private void EvasiveCounter()
    {
        float foundSomethingCenter = Sensors[0].GetDistance();
        float foundSomethingLeft = Sensors[1].GetDistance();
        float foundSomethingRight = Sensors[2].GetDistance();

        if (foundSomethingCenter > 0 && foundSomethingCenter < maxDistanceToCharge)
        {
            Motors[0].SetSpeed(0.5f);
            Motors[1].SetSpeed(0f);
        }
        else if (foundSomethingLeft > 0 && foundSomethingLeft < maxDistanceToCharge)
        {
            Motors[0].SetSpeed(0f);
            Motors[1].SetSpeed(1f);
        }
        else if (foundSomethingRight > 0 && foundSomethingRight < maxDistanceToCharge)
        {
            Motors[0].SetSpeed(1f);
            Motors[1].SetSpeed(0f);
        }
        else
        {
            Motors[0].SetSpeed(0.75f);
            Motors[1].SetSpeed(0.75f);
        }
    }
}