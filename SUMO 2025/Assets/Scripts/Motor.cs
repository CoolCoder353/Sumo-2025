using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : MonoBehaviour
{
    public float MaxRPM = 300;
    public WheelCollider Wheel;

    public float _currentRPM = 0;
    public float _steerAngle = 0;

    // Start is called before the first frame update
    void Start()
    {
        Wheel.ConfigureVehicleSubsteps(5, 12, 15);
    }

    public void SetRPM(float rpm)
    {
        _currentRPM = Mathf.Clamp(rpm, -MaxRPM, MaxRPM);
    }

    public void SetSpeed(float percentage)
    {
        SetRPM(percentage * MaxRPM);
    }

    public void SetSteerAngle(float angle)
    {
        _steerAngle = angle;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Apply the RPM to the wheel collider
        Wheel.motorTorque = _currentRPM;

        // Apply the steer angle to the wheel collider
        Wheel.steerAngle = _steerAngle;
    }
}