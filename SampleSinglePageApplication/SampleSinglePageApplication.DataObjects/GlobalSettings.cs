﻿namespace SampleSinglePageApplication;

/// <summary>
/// This static class is used to persist a few settings about the startup state of the application.
/// </summary>
public static class GlobalSettings
{
    public static long RunningSince { get; set; } = 0;
    public static bool StartupError { get; set; }
    public static string StartupErrorCode { get; set; } = "";
    public static bool StartupRun { get; set; }
}