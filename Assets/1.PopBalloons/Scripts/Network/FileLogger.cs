using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Simple file logger for debugging on HoloLens
/// Writes logs to a file that can be accessed via Device Portal
/// </summary>
public class FileLogger : MonoBehaviour
{
    private static FileLogger instance;
    private StreamWriter writer;
    private string logFilePath;

    public static FileLogger Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("FileLogger");
                instance = go.AddComponent<FileLogger>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Create log file in persistent data path (safe on all platforms)
        string folder = Application.persistentDataPath;
        logFilePath = Path.Combine(folder, "PopBalloons_Debug.log");

        try
        {
            // Delete old log file
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }

            writer = new StreamWriter(logFilePath, true);
            writer.AutoFlush = true;

            Log("=====================================");
            Log($"FileLogger started at {DateTime.Now}");
            Log($"Log file: {logFilePath}");
            Log("=====================================");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create log file: {e.Message}");
        }
    }

    public static void Log(string message)
    {
        string logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
        
        // Also write to Unity Debug.Log
        Debug.Log(logMessage);

        // Write to file
        if (Instance.writer != null)
        {
            try
            {
                Instance.writer.WriteLine(logMessage);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to log file: {e.Message}");
            }
        }
    }

    public static void LogError(string message)
    {
        string logMessage = $"[{DateTime.Now:HH:mm:ss}] ERROR: {message}";
        Debug.LogError(logMessage);

        if (Instance.writer != null)
        {
            try
            {
                Instance.writer.WriteLine(logMessage);
            }
            catch { }
        }
    }

    public static void LogWarning(string message)
    {
        string logMessage = $"[{DateTime.Now:HH:mm:ss}] WARNING: {message}";
        Debug.LogWarning(logMessage);

        if (Instance.writer != null)
        {
            try
            {
                Instance.writer.WriteLine(logMessage);
            }
            catch { }
        }
    }

    private void OnDestroy()
    {
        if (writer != null)
        {
            try
            {
                Log("FileLogger shutting down");
                writer.Close();
                writer.Dispose();
            }
            catch { }
        }
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
    }
}
