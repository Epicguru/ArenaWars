using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float CurrentMagnitude;
    public float MaxMagnitude = 20f;
    public float Frequency = 20f;
    public float LerpSpeed = 25f;

    public float MagnitudeDecay = 2f;

    private Vector2 targetOffset;
    private float timer;

    public void Update()
    {
        timer += Time.deltaTime;
        float interval = 1f / Frequency;
        if(timer >= interval)
        {
            timer -= interval;

            // Change shake target.
            targetOffset = Random.insideUnitCircle * CurrentMagnitude;
        }

        transform.localPosition = Vector2.Lerp(transform.localPosition, targetOffset, Time.deltaTime * LerpSpeed);
        CurrentMagnitude = Mathf.Lerp(CurrentMagnitude, 0f, Time.deltaTime * MagnitudeDecay);
    }

    public void Shake(float magnitude)
    {
        this.CurrentMagnitude += magnitude;
        if (CurrentMagnitude > MaxMagnitude)
            CurrentMagnitude = MaxMagnitude;
    }
}