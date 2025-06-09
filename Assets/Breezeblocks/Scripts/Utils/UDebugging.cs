using System.Diagnostics;
using UnityEngine;

public static class UDebugging
{
    /// <summary>
    /// Logs the name of the method + class that called this.
    /// </summary>
    public static void LogCaller()
    {
        // Skip 1 frame (LogCaller itself), so Frame 1 is the immediate caller
        var trace = new StackTrace();
        var frame = trace.GetFrame(1);
        var method = frame.GetMethod();
        string className = method.DeclaringType.FullName;
        string methodName = method.Name;
        UnityEngine.Debug.Log($"Called from {className}.{methodName}()");
    }
}
