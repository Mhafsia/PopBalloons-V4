using UnityEngine;
using System.IO;
using System;

namespace PopBalloons.Utilities
{
    /// <summary>
    /// Logs all Unity console messages to a file for debugging
    /// Automatically creates logs in Assets/Logs/ directory
    /// </summary>
    public class ConsoleLogger : MonoBehaviour
    {
        private static string logFilePath;
        private static StreamWriter logWriter;
        private static bool isInitialized = false;

        void Awake()
        {
            if (!isInitialized)
            {
                InitializeLogger();
                isInitialized = true;
            }
        }

        private void InitializeLogger()
        {
            // Create Logs directory if it doesn't exist
            string logsDir = Path.Combine(Application.dataPath, "Logs");
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }

            // Create log file with timestamp
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            logFilePath = Path.Combine(logsDir, $"unity_console_{timestamp}.txt");

            // Initialize file writer
            logWriter = new StreamWriter(logFilePath, true);
            logWriter.AutoFlush = true;

            // Subscribe to Unity log messages
            Application.logMessageReceived += HandleLog;

            // Write header
            logWriter.WriteLine("=".PadRight(80, '='));
            logWriter.WriteLine($"Unity Console Log - Session started: {DateTime.Now}");
            logWriter.WriteLine($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            logWriter.WriteLine("=".PadRight(80, '='));
            logWriter.WriteLine();
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logType = type.ToString().ToUpper();
            
            // Format: [HH:mm:ss.fff] [TYPE] Message
            logWriter.WriteLine($"[{timestamp}] [{logType}] {logString}");
            
            // Include stack trace for errors and exceptions
            if (type == LogType.Error || type == LogType.Exception)
            {
                if (!string.IsNullOrEmpty(stackTrace))
                {
                    logWriter.WriteLine($"    Stack Trace: {stackTrace}");
                }
            }
        }

        void OnDestroy()
        {
            if (logWriter != null)
            {
                logWriter.WriteLine();
                logWriter.WriteLine("=".PadRight(80, '='));
                logWriter.WriteLine($"Session ended: {DateTime.Now}");
                logWriter.WriteLine("=".PadRight(80, '='));
                logWriter.Close();
                logWriter = null;
            }

            Application.logMessageReceived -= HandleLog;
            isInitialized = false;
        }

        void OnApplicationQuit()
        {
            OnDestroy();
        }
    }
}
