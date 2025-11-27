using UnityEngine;
using System.IO;
using System;

namespace PopBalloons.Utilities
{
    /// <summary>
    /// Simple logger that writes Unity console logs to a file.
    /// </summary>
    public class FileLogger : MonoBehaviour
    {
        private string logPath;
        private StreamWriter writer;

        private void Awake()
        {
            // Create a unique log file name with timestamp
            string fileName = $"GameLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            
            string folderPath;
#if UNITY_EDITOR
            folderPath = Path.Combine(Application.dataPath, "Logs");
#else
            folderPath = Application.persistentDataPath;
#endif

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            logPath = Path.Combine(folderPath, fileName);

            Debug.LogWarning($"[FileLogger] Writing logs to: {logPath}");

            try
            {
                writer = new StreamWriter(logPath, true);
                writer.AutoFlush = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileLogger] Failed to create log file: {e.Message}");
            }

            Application.logMessageReceived += HandleLog;
        }

        [ContextMenu("Open Log Folder")]
        public void OpenLogFolder()
        {
            string folderPath;
#if UNITY_EDITOR
            folderPath = Path.Combine(Application.dataPath, "Logs");
#else
            folderPath = Application.persistentDataPath;
#endif
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            System.Diagnostics.Process.Start(folderPath);
            Debug.Log($"[FileLogger] Opened folder: {folderPath}");
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (writer == null) return;

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] [{type}] {logString}";

            // Add stack trace for errors and exceptions
            if (type == LogType.Error || type == LogType.Exception)
            {
                logEntry += $"\n{stackTrace}";
            }

            try
            {
                writer.WriteLine(logEntry);
            }
            catch (Exception e)
            {
                // Avoid infinite loop if logging fails
                // Debug.LogError($"[FileLogger] Failed to write log: {e.Message}"); 
            }
        }
    }
}
