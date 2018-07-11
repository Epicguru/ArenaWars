using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Agent))]
public class AgentMovement : NetworkBehaviour
{
    [Header("References")]
    public Agent Agent;
    public Rigidbody2D Body;

    [Header("Speed")]
    public float BaseSpeed = 5f;
    public float SpeedMultiplier = 1f;
    public float FinalSpeed
    {
        get
        {
            return BaseSpeed * SpeedMultiplier;
        }
    }

    [Header("Velocity")]
    public Vector2 InputDirection;
    public Vector2 WorldVelocity;
    public float WorldVelocityReduction = 0f;

    public void Update()
    {
        if (Body == null)
            return;

        if (!isServer)
            return;

        WorldVelocity = Vector2.Lerp(WorldVelocity, Vector2.zero, WorldVelocityReduction * Time.deltaTime);
        Vector2 inputVel = InputDirection.normalized * FinalSpeed;

        Vector2 velocity = WorldVelocity + inputVel;

        Body.velocity = velocity;
    }
}