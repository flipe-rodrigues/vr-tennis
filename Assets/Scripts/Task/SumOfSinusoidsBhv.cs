using System.Collections.Generic;
using UnityEngine;

public class SumOfSinusoidsBhv : CachedTransformBhv
{
    public List<Sinusoid> sinusoids;

    private void OnValidate()
    {
        if (sinusoids.Count == 0)
        {
            sinusoids = new List<Sinusoid>
            {
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
    }

    private void Update()
    {
        this.Transform.position = this.SumOfSinusoids();
    }

    private Vector3 SumOfSinusoids()
    {
        Vector3 r = Vector3.zero;

        for (int i = 0; i < sinusoids.Count; i++)
        {
            float amplitude = sinusoids[i].amplitude;
            float frequency = sinusoids[i].frequency;
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
