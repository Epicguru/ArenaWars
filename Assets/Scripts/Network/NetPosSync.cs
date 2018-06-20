using UnityEngine;
using UnityEngine.Networking;

public class NetPosSync : NetworkBehaviour
{
    public bool UseRigidbody2D = true;
    public Rigidbody2D Rigidbody
    {
        get
        {
            if(_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }

            return _rigidbody;
        }
    }
    private Rigidbody2D _rigidbody;

    [Range(0f, 60f)]
    public float UpdateRate = 15f;

    public float LerpSpeed = 10f;

    [SyncVar]
    private Vector2 Position;

    [SyncVar(hook = "NewVel")]
    private Vector2 Velocity;

    [SyncVar]
    private float Rotation;

    private Vector2 realTarget;

    public void Awake()
    {
        if (UseRigidbody2D)
        {
            if(Rigidbody == null)
            {
                Debug.LogWarning("Object '{0}' has a NetPosSync that uses a rigidbody to sync its position, but a rigidbody 2D could not be found!".Form(name));
            }
        }
    }

    public Vector2 GetLastVelocity()
    {
        if (!UseRigidbody2D)
        {
            Debug.LogWarning("This NetPosSync ({0}) does not use Rigidbody2D, so does not sync velocity!".Form(name));
            return Vector2.zero;
        }
        return Velocity;
    }

    public override void OnStartClient()
    {
        // Set initial state, unless on server.
        if (isServer)        
            return;        

        transform.localPosition = Position;
        var rot = transform.localEulerAngles;
        rot.z = Rotation;
        transform.localEulerAngles = rot;
        realTarget = Position;
    }

    public void Update()
    {
        if (isServer)
        {
            UpdateState();
        }
        else
        {
            MoveToTarget();
        }
    }

    [Server]
    private void UpdateState()
    {
        if(Position != (Vector2)transform.localPosition)
            Position = transform.localPosition;
        if(Rotation != transform.localEulerAngles.z)
            Rotation = transform.localEulerAngles.z;

        if (UseRigidbody2D && Rigidbody != null)
        {
            if(Velocity != Rigidbody.velocity)
                Velocity = Rigidbody.velocity;
        }
    }

    [Client]
    private void MoveToTarget()
    {
        if (!UseRigidbody2D)
        {
            transform.localPosition = Vector2.Lerp(transform.localPosition, Position, Time.deltaTime * LerpSpeed);
            // Rotation...
            Vector3 angles = transform.localEulerAngles;
            angles.z = Mathf.LerpAngle(angles.z, Rotation, Time.deltaTime * LerpSpeed);
            transform.localEulerAngles = angles;
        }
        else
        {
            if (Rigidbody == null)
            {
                Debug.LogWarning("Position and rotation of '{0}' not synced, Rigidbody2D missing!");
                return; 
            }

            // Rotation...
            Vector3 angles = transform.localEulerAngles;
            angles.z = Mathf.LerpAngle(angles.z, Rotation, Time.deltaTime * LerpSpeed);
            transform.localEulerAngles = angles;

            // Move real target by current velocity...
            realTarget += Velocity * Time.deltaTime;

            // Lerp from real position to target position, which should be very close by.
            float t = Time.deltaTime * 20f;
            transform.localPosition = Vector2.Lerp(transform.localPosition, realTarget, t);
        }
    }

    private void NewVel(Vector2 newVel)
    {
        Velocity = newVel;
        if (isServer)
            return;

        realTarget = Position;
    }

    public override float GetNetworkSendInterval()
    {
        if (UpdateRate == 0)
            return 0;

        return 1f / UpdateRate;
    }
}