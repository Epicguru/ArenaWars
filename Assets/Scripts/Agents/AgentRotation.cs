using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Agent))]
public class AgentRotation : NetworkBehaviour
{
    // :D
    [Header("References")]
    public Agent Agent;

    [Header("Controls")]
    public float TargetRotation;

    public void Update()
    {
        if (!isServer)
            return;

        var angle = transform.localEulerAngles;
        angle.z = TargetRotation;
        transform.localEulerAngles = angle;
    }
}
