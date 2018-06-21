using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public static GameCamera Instance;

    [Header("References")]
    public Camera Cam;
    public CameraShake Shake;

    [Header("Controls")]
    public float Z;
    public Transform TargetTransform;
    public bool Lerp = true;
    public float LerpSpeed = 5f;

    public void Awake()
    {
        Instance = this;
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    public void Update()
    {
        if (TargetTransform != null)
        {
            if (Lerp)
            {
                transform.position = Vector2.Lerp(transform.position, TargetTransform.position, Time.deltaTime * LerpSpeed);
            }
            else
            {
                transform.position = new Vector3(TargetTransform.position.x, TargetTransform.position.y, transform.position.z);
            }
        }

        var pos = transform.position;
        pos.z = Z;
        transform.position = pos;
    }
}
