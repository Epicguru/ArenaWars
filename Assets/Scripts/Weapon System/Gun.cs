using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Gun : Weapon
{
    // A ranged weapon which fires projectiles.

    [Header("References")]
    public Transform[] ProjectileSpawnPoints;
    public Animator Animator;

    [Header("Shooting")]
    [Tooltip("Data about the projectile to be fired.")]
    public ProjectileData ProjectileData;

    [Tooltip("The rate of fire, in shots per second.")]
    public float FireRate = 2f;

    [Tooltip("The amount of camera shake applied upon each shot.")]
    public float ShakeMagnitude = 0.05f;

    [Tooltip("Is it a dual-wield weapon? If true, set a bool called 'Flip' in the animator, allowing for alternating animations.")]
    public bool IsDual = false;

    [Header("Meele")]
    [Tooltip("The minimum time between melee attacks.")]
    public float MeeleCooldown = 0.5f;

    [Header("Animation")]
    public string ShootTrigger = "Shoot";
    private int ShootID = -1;
    public string CanShootBool = "CanShoot";
    private int CanShootID = -1;
    public string MeeleTrigger = "Meele";
    private int MeeleID = -1;

    [Header("Info")]
    [ReadOnly]
    [SyncVar]
    public bool HasEquipped = false;
    [ReadOnly]
    [SyncVar]
    public bool IsShooting;

    private bool flip;
    private float timer;
    private float meeleTimer;

    public void Awake()
    {
        if (Animator == null)
        {
            Animator = GetComponentInChildren<Animator>();
            if (Animator == null)
            {
                Debug.LogError("Gun '{0}' does not have an animator, which is required. Set the reference value or attach an Animator component in one of its children.".Form(name));
            }
        }
        var callback = GetComponentInChildren<GenericAnimationCallback>();
        if (callback != null)
        {
            callback.UponEvent.AddListener(AnimationEvent);
        }
        else
        {
            Debug.LogError("Did not find a GenericAnimationCallback component in gun '{0}'! It should be next to the Animator component.".Form(name));
        }

        if(ProjectileSpawnPoints == null || ProjectileSpawnPoints.Length == 0)
        {
            Debug.LogError("Projecitle spawn points array is null or empty! Invalid gun '{0}'! Where do you expect the bullets to come from?! Set the reference!".Form(name));
        }

        if(ProjectileData == null)
        {
            Debug.LogError("Projectile data is null, so no projectile can be fired! Gun: {0}".Form(name));
        }
    }

    public void Update()
    {
        if (Animator == null)
            return;

        if(ShootID == -1)            
            ShootID = Animator.StringToHash(ShootTrigger);
        if (CanShootID == -1)
            CanShootID = Animator.StringToHash(CanShootBool);

        bool canFire = HasEquipped && (meeleTimer == 0f);
        Animator.SetBool(CanShootID, canFire);

        timer += Time.deltaTime;
        if(IsShooting && timer >= GetFiringInterval())
        {
            timer = 0f;

            // Cause the animation to be played, if possible...
            // On the server, the animation playing will cause the projectile to be fired.
            // On everyone else, it will just do visual and audio effects.

            if (IsDual)
            {
                Animator.SetBool("Flip", flip);
                flip = !flip;
            }
            Animator.SetTrigger(ShootID);
        }

        meeleTimer -= Time.deltaTime;
        if (meeleTimer < 0f)
            meeleTimer = 0f;

        if (Input.GetKeyDown(KeyCode.E))
        {
            DoMeeleAttack();
        }
    }

    public void AnimationEvent(AnimationEvent e)
    {
        if(e.stringParameter == "Equip" && isServer)
        {
            HasEquipped = true;
        }
        if(e.stringParameter == "Shoot")
        {
            if (isServer)
            {
                // Fire the projectile.
                Fire(e.intParameter);
            }

            if(GameCamera.Instance.TargetTransform == Item.GetParentAgent().transform)
            {
                // Do any effects here. Caused on both client and server.
                GameCamera.Instance.Shake.Shake(ShakeMagnitude);
            }
        }
    }

    public float GetFiringInterval()
    {
        return 1f / FireRate;
    }

    public void DoMeeleAttack()
    {
        if (meeleTimer != 0f)
            return;
        if (!HasEquipped)
            return;

        if(MeeleID == -1)
        {
            MeeleID = Animator.StringToHash(MeeleTrigger);
        }

        Animator.SetTrigger(MeeleID);
        meeleTimer = MeeleCooldown;
    }

    [Server]
    private void Fire(int index)
    {
        // Fire projectile...
        if (index < 0 || index >= ProjectileSpawnPoints.Length)
        {
            Debug.LogError("Index is out of spawn point range! {0}".Form(index));
            return;
        }

        Projectile.SpawnNew(ProjectileSpawnPoints[index].position, ProjectileSpawnPoints[index].eulerAngles.z, ProjectileData);
    }

    [Server]
    public override void StartAttack()
    {
        if (IsShooting)
            return;

        IsShooting = true;
    }

    [Server]
    public override void EndAttack()
    {
        if (!IsShooting)
            return;

        IsShooting = false;
    }
}