using System;

using UnityEngine;

public class MyUtils
{
    public static bool XOR(bool a, bool b)
    {
        return ((!a && b) || (a && !b));
    }

    public static float Normalice(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
    
    public static float Clamp0360(float eulerAngles)
    {
        float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;
        if (result < 0)
            result += 360f;
        if (Math.Abs(result - 360) < 0.01f)
            result = 0;

        return result;
    }
    public static string GetCountdownTimeString(double timeMiliseconds)
    {
        int minutes = (int)(timeMiliseconds / 1000) / 60;
        int seconds = (int)(timeMiliseconds / 1000) % 60;
        int miliseconds = (int)timeMiliseconds % 1000;

        return String.Format(String.Format("{0:00}:{1:00}.{2:00}",
            minutes, seconds,
            miliseconds / 10));
    }
}