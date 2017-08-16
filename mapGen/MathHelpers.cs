using UnityEngine;

public static class MathHelpers
{

    /// <summary>
    /// Gets a standard normal distribution. Mean is 0 and deviation is 1.
    /// </summary>
    /// <returns></returns>
    public static float GaussianFloat()
    {
        float u, v, S;

        do
        {
            u = 2f * Random.value - 1f;
            v = 2f * Random.value - 1f;
            S = u * u + v * v;
        } while (S >= 1f);

        float fac = Mathf.Sqrt(-2f * Mathf.Log(S) / S);
        return u * fac;
    }

    private static float NormalizedGaussianFloat(float mean, float stdDev)
    {
        return GaussianFloat() * stdDev + mean;
    }

    /// <summary>
    /// Finds a value within a range.
    /// </summary>
    /// <param name="mean"></param>
    /// <param name="standardDeviation"></param>
    /// <param name="maxValue">Upper bound of return value.</param>
    /// <param name="minValue">Lower bound of return value.</param>
    /// <returns></returns>
    public static int FindValueInDistributionRange(int mean, int standardDeviation, int maxValue, int minValue)
    {
        int valueInRange;
        while (true)
        {
            valueInRange = Mathf.RoundToInt(NormalizedGaussianFloat(mean, standardDeviation));

            if (valueInRange <= maxValue && valueInRange >= minValue)
                return valueInRange;
        }
    }

    /// <summary>
    /// Gets a point within an elipsis.
    /// </summary>
    public static Vector2 RandomPointInElipsis(int width, int height)
    {
        float t = 2 * Mathf.PI * Random.value;
        float u = Random.value + Random.value;
        float r;
        if (u > 1)
            r = 2 - u;
        else
            r = u;

        Vector2 pointInArea = new Vector2(width * r * Mathf.Cos(t) / 2,
                                          height * r * Mathf.Sin(t) / 2);
        return pointInArea;
    }
}