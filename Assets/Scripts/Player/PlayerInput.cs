using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
public class PlayerInput : NetworkBehaviour
{
    public Player Player;

    public Vector2 InputDirection;
    public float PlayerInputFalloff = 10f;
    public float SendRate = 30f;

    private float timer = 0f;

    private Vector2 lastSentDir;
    private float lastSentRot;
    private bool lastSentAttack;

    public void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Player.Agent == null)
            return;

        int x = 0;
        int y = 0;

        if (InputManager.IsPressed("Right"))
            x++;
        if (InputManager.IsPressed("Left"))
            x--;
        if (InputManager.IsPressed("Up"))
            y++;
        if (InputManager.IsPressed("Down"))
            y--;

        InputDirection.x = x;
        InputDirection.y = y;

        Vector2 mousePos = InputManager.MousePos;
        Vector2 agentPos = Player.Agent.transform.position;
        float rotation = Mathf.Atan2(mousePos.y - agentPos.y, mousePos.x - agentPos.x) * Mathf.Rad2Deg;

        bool attack = InputManager.IsPressed("Shoot");

        // Apply to active agent!
        if (isServer)
        {
            Player.Agent.Movement.InputDirection = InputDirection;
            Player.Agent.Rotation.TargetRotation = rotation;

            var held = Player.Agent.Hands.GetHeldItem();
            if (held != null)
            {
                if (held.IsWeapon)
                {
                    if (attack)
                    {
                        held.Weapon.StartAttack();
                    }
                    else
                    {
                        held.Weapon.EndAttack();
                    }
                }
            }            
        }
        else
        {
            // Send movement to server.
            timer += Time.unscaledDeltaTime;
            float interval = 1f / SendRate;
            if (timer >= interval)
            {
                timer -= interval;
                if(InputDirection != lastSentDir || lastSentRot != rotation || attack != lastSentAttack)
                {
                    CmdSendData(InputDirection, rotation, attack);
                    lastSentDir = InputDirection;
                    lastSentRot = rotation;
                    lastSentAttack = attack;
                }
            }
        }
    }

    [Command]
    private void CmdSendData(Vector2 inputDir, float rotation, bool attack)
    {
        InputDirection = inputDir;

        if (Player.Agent == null)
            return;

        Player.Agent.Movement.InputDirection = InputDirection;
        Player.Agent.Rotation.TargetRotation = rotation;

        var held = Player.Agent.Hands.GetHeldItem();
        if (held != null)
        {
            if (held.IsWeapon)
            {
                if (attack)
                {
                    held.Weapon.StartAttack();
                }
                else
                {
                    held.Weapon.EndAttack();
                }
            }
        }
    }
}
