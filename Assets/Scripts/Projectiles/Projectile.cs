using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour
{
    public const int MAX_RAY_HITS = 20;
    private static RaycastHit2D[] hits = new RaycastHit2D[MAX_RAY_HITS];

    public ProjectileData Data;

    [Header("Collision")]
    public LayerMask CollisionMask;

    [Header("Info")]
    [ReadOnly]
    public Vector2 Bearing;
    [ReadOnly]
    public float Speed;
    [SyncVar]
    [ReadOnly]
    public string DataID;

    private float angle;
    private int bounceCount;
    private float lifeTimer;

    public void Init(Vector2 position, float angle, ProjectileData data)
    {
        this.Data = data;
        transform.position = position;
        SetAngle(angle);

        // Apply all data to this instance.
        this.Speed = data.Speed;
        this.bounceCount = 0;
        lifeTimer = 0f;
    }

    public override void OnStartClient()
    {
        if (isServer)
            return;

        Init(transform.position, transform.eulerAngles.z, ProjectileData.Get(DataID));
    }

    public void Update()
    {
        // Move forward and detect all collision.
        Vector2 currentPos = transform.position;
        Vector2 predictedPos = currentPos + (Bearing * Speed * Time.deltaTime);
        float currentAngle = GetAngle();

        // Detect and resolve collision, penetration, bouncing...
        Vector2 finalPos;
        float finalAngle;
        ResolveCollisions(currentPos, predictedPos, currentAngle, out finalPos, out finalAngle);

        // Apply the resolved position and angle.
        if(GetAngle() != finalAngle)
            SetAngle(finalAngle);
        transform.position = finalPos;

        // Check lifetime
        lifeTimer += Time.deltaTime;
        if(lifeTimer >= Data.MaxLifetime)
        {
            StopAll();
        }
    }

    public float GetAngle()
    {
        return angle;
    }

    private void SetAngle(float angle)
    {
        if (this.angle == angle)
            return;

        this.angle = angle;
        var angles = transform.localEulerAngles;
        angles.z = angle;
        transform.localEulerAngles = angles;

        // Calculate bearing using some simple trigonometry.
        float x = Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = Mathf.Sin(angle * Mathf.Deg2Rad);
        Bearing.x = x;
        Bearing.y = y;
        Bearing.Normalize();
    }

    public void ResolveCollisions(Vector2 currentPos, Vector2 predictedPos, float currentAngle, out Vector2 finalPos, out float finalAngle)
    {
        // Detect and solve collisions between current and future pos.
        Physics2D.queriesStartInColliders = false;
        int totalHits = Physics2D.LinecastNonAlloc(currentPos, predictedPos, hits, CollisionMask);
        if (totalHits > MAX_RAY_HITS)
        {
            Debug.LogWarning("Number of ray hits exceeded the processing capacity! Capacity is {0}, hit {1} colliders. Consider upping the MAX_RAY_HITS value in Projectile.cs.".Form(MAX_RAY_HITS, totalHits));
            totalHits = MAX_RAY_HITS;
        }

        for (int i = 0; i < totalHits; i++)
        {
            var hit = hits[i];

            // If it is a trigger, ignore.
            if (hit.collider.isTrigger)
                continue;

            // We hit something!
            ServerHit(hit);

            // For now, assume nothing can be penetrated through...
            // Check of we can bounce of this...
            if (bounceCount < Data.MaxBounces)
            {
                bounceCount++;
                Vector2 n = hit.normal;
                Vector2 b = Bearing;

                Vector2 newBearing = b - (2 * (Vector2.Dot(b, n)) * n);
                float angle = newBearing.ToAngle();

                bool pos = Random.value >= 0.5f;
                float add = Random.Range(Mathf.Abs(Data.BounceAngleChange.x), Mathf.Abs(Data.BounceAngleChange.y));
                angle += add * (pos ? 1f : -1f);

                finalAngle = angle;
                finalPos = hit.point;

                // Spawn a hit effect
                if (isServer)
                {
                    TempEffect.NetSpawn(TempEffects.HIT_SPARKS_SMALL, hit.point + hit.normal * 0.15f, hit.normal.ToAngle());
                }

                return;
            }

            // Just stop as soon as we hit something: destroy this object if on server.
            StopAll();

            // Spawn a hit effect.
            // Spawn a hit effect, on the server so it is authorative: if a client sees a hit effect, it is 'real'.
            if (isServer)
            {
                TempEffect.NetSpawn(TempEffects.HIT_SPARKS, hit.point + hit.normal * 0.15f, hit.normal.ToAngle());
            }

            // Break out of the method.
            finalPos = hit.point;
            finalAngle = currentAngle;
            return;
        }

        // Looks like no final collisions.
        finalPos = predictedPos;
        finalAngle = currentAngle;        
    }

    [Server]
    private void ServerHit(RaycastHit2D hit)
    {
        var agent = hit.collider.GetComponentInParent<Agent>();
        if(agent != null)
        {
            agent.Movement.WorldVelocity += Bearing * 3f;
        }
    }

    public void StopAll()
    {
        if (isServer)
        {
            // Destroy on server and all clients.
            Destroy(this.gameObject);
        }
        else
        {
            // Temporarily disable, until the server destroys it.
            gameObject.SetActive(false);
        }
    }

    [Server]
    public static void SpawnNew(Vector2 position, float angle, ProjectileData data)
    {
        if (data == null)
            return;

        var prefab = Spawnables.Instance.Projectile;

        var spawned = Instantiate(prefab);
        spawned.Init(position, angle, data);

        spawned.DataID = data.name;

        // Spawn on server.
        NetworkServer.Spawn(spawned.gameObject);
    }
}