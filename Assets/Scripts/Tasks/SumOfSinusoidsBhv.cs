using System.Collections.Generic;
using UnityEngine;

public class SumOfSinusoidsBhv : CachedTransformBhv
{
    // Public fields
    [Range(0, 1)]
    public float globalAmplitudeModifier = 0.05f;
    [Range(0, 1)]
    public float globalFrequencyModifier = 0.75f;
    public List<Sinusoid> sinusoids;

    // Private fields
    private Vector3 _initialPosition;

    private void OnValidate()
    {
        if (sinusoids.Count == 0)
        {
            sinusoids = new List<Sinusoid>
            {
                // From Wang et al. 2021
                new Sinusoid(2.31f, 0.1f),
                new Sinusoid(2.31f, 0.25f),
                new Sinusoid(2.31f, 0.55f),
                new Sinusoid(1.76f, 0.85f),
                new Sinusoid(1.3f, 1.15f),
                new Sinusoid(0.97f, 1.55f),
                new Sinusoid(0.73f, 2.05f)
            };
        }
    }

    private void Start()
    {
        this.OnValidate();
        _initialPosition = this.Position;
    }

    private void Update()
    {
        this.Position = this.SumOfSinusoids() + _initialPosition;
    }

    private Vector3 SumOfSinusoids()
    {
        Vector3 r = Vector3.zero;

        for (int i = 0; i < sinusoids.Count; i++)
        {
            float amplitude = sinusoids[i].amplitude * globalAmplitudeModifier;
            float frequency = sinusoids[i].frequency * globalFrequencyModifier;
            Vector3 phase = sinusoids[i].phase;

            r.x += amplitude * Mathf.Cos(2 * Mathf.PI * frequency * Time.time + phase.x);
            r.y += amplitude * Mathf.Cos(2 * Mathf.PI * frequency * Time.time + phase.y);
            r.z += amplitude * Mathf.Cos(2 * Mathf.PI * frequency * Time.time + phase.z);
        }

        return r;
    }
}

[System.Serializable]
public class Sinusoid
{
    public float amplitude;
    public float frequency;
    public Vector3 phase;

    public Sinusoid(float amplitude, float frequency)
    {
        this.amplitude = amplitude;
        this.frequency = frequency;
        this.phase = this.RandomPhase(-Mathf.PI, Mathf.PI);
    }

    private Vector3 RandomPhase(float min, float max)
    {
        return new Vector3(
            Random.Range(min, max),
            Random.Range(min, max),
            Random.Range(min, max)
        );
    }
}
