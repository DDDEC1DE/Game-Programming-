using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

public class DebugUtils
{
    public static void LogError(string format, params object[] args)
    {
        string message = string.Format(format, args);

        UnityEngine.Debug.LogError(message);
    }
}