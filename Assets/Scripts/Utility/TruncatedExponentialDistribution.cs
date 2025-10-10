using UnityEngine;

[System.Serializable]
public class TruncatedExponentialDistribution
{
    [Min(0)]
    public float min;
    [Min(0)]
    public float mean;
    [Min(0)]
    public float max;
    [SerializeField]
    private AnimationCurve _pdf;

    public TruncatedExponentialDistribution(float min, float mean, float max)
    {
        this.min = min;
        this.mean = mean;
        this.max = max;
        this.UpdatePDF();
    }

    public float Sample()
    {
        if (mean - min == 0)
        {
            return min;
        }

        float lambda = 1f / (mean - min);
        float u = Random.value;
        float expTerm = Mathf.Exp(-lambda * max);
        float sample = -Mathf.Log(1f - u * (1f - expTerm)) / lambda + min;
        return sample;
    }

    public void UpdatePDF()
    {
        _pdf = new AnimationCurve();
        float lambda = 1f / (mean - min);
        int sampleCount = 100;
        for (int i = 0; i <= sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float x = Mathf.Lerp(0, max, t);
            float y = x < min ? 0 : lambda * Mathf.Exp(-lambda * x);
            _pdf.AddKey(x, y);
        }
    }
}
