/* Kaustabh Dutta
 * Reference:
 * StackOverflow
 * GitHub*/


using System;
using System.Linq;
using UnityEngine;

public static class Perlin
{
    private static System.Random _random = new System.Random();
    private static int[] _permutation;

    private static Vector2[] _gradients;

    static Perlin()
    {
        CalculatePermutation(out _permutation);
        CalculateGradients(out _gradients);
    }

    private static void CalculatePermutation(out int[] p)
    {
        p = Enumerable.Range(0, 256).ToArray();

        //shuffle the array
        for (var i = 0; i < p.Length; i++)
        {
            var source = _random.Next(p.Length);

            var t = p[i];
            p[i] = p[source];
            p[source] = t;
        }
    }

   
    // generate a new permutation.
    
    public static void Reseed()
    {
        CalculatePermutation(out _permutation);
    }

    private static void CalculateGradients(out Vector2[] grad)
    {
        grad = new Vector2[256];

        for (var i = 0; i < grad.Length; i++)
        {
            Vector2 gradient;

            do
            {
                gradient = new Vector2((float)(_random.NextDouble() * 2 - 1), (float)(_random.NextDouble() * 2 - 1));
            }
            while (gradient.sqrMagnitude >= 1);

            gradient.Normalize();

            grad[i] = gradient;
        }

    }

    private static float Drop(float t)
    {
        t = Math.Abs(t);
        return 1f - t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Q(float u, float v)
    {
        return Drop(u) * Drop(v);
    }

    public static float Noise(float x, float y)
    {
        var cell = new Vector2((float)Math.Floor(x), (float)Math.Floor(y));

        var total = 0f;

        var corners = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

        foreach (var n in corners)
        {
            var ij = cell + n;
            var uv = new Vector2(x - ij.x, y - ij.y);

            var index = _permutation[(int)ij.x % _permutation.Length];
            index = _permutation[(index + (int)ij.y) % _permutation.Length];

            var grad = _gradients[index % _gradients.Length];

            total += Q(uv.x, uv.y) * Vector2.Dot(grad, uv);
        }

        return Math.Max(Math.Min(total, 1f), -1f);
    }

}