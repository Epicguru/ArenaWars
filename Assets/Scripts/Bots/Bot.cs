using UnityEngine;
using UnityEngine.Networking;

public class Bot : NetworkBehaviour
{
    public Agent Agent;
    public string StartItemName;

    public int TargetPlayerIndex = 0;
    public float MaxFiringAngle = 15f;

    public override void OnStartServer()
    {
        Item.NetSpawnAndEquip(Item.Get(StartItemName), this.Agent);
    }

    public void Update()
    {
        if (!isServer)
            return;

        float angle = GetTargetAngle();

        Agent.Rotation.TargetRotation = angle;
    }

    public float GetTargetAngle()
    {
        Vector2 playerPos = Vector2.zero;
        if (Player.All != null && TargetPlayerIndex >= 0 && Player.All.Count > TargetPlayerIndex)
        {
            if (Player.All[TargetPlayerIndex].Agent != null)
            {
                playerPos = Player.All[TargetPlayerIndex].Agent.transform.position;
            }
        }

        return (playerPos - (Vector2)transform.position).ToAngle();
    }

    public bool InFiringAngle()
    {
        return InFiringAngle(GetTargetAngle());
    }

    public bool InFiringAngle(float targetAngle)
    {
        float angle = transform.localEulerAngles.z;
        float dst = Mathf.DeltaAngle(angle, targetAngle);

        return Mathf.Abs(dst) <= MaxFiringAngle;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = InFiringAngle() ? Color.red : Color.yellow;

        Gizmos.DrawRay(transform.position, transform.right * 5f);
    }
}