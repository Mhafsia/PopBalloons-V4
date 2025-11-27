using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace PopBalloons.Network
{
    /// <summary>
    /// MJPEG streaming server for real-time camera feed.
    /// Creates an HTTP server that streams continuous JPEG frames.
    /// </summary>
    public class MJPEGStreamServer : MonoBehaviour
    {
        [Header("Stream Configuration")]
        [SerializeField] private int port = 8081;
        [SerializeField] private bool autoStart = true;
        [SerializeField] private int targetFPS = 15;
        [SerializeField] private int streamWidth = 640;
        [SerializeField] private int streamHeight = 480;
        [SerializeField] private int jpegQuality = 75;

        private HttpListener httpListener;
        private Thread listenerThread;
        private bool isRunning = false;
        private List<StreamWriter> activeStreams = new List<StreamWriter>();
        private byte[] currentFrame;
        private readonly object frameLock = new object();
        private Coroutine captureCoroutine;

        private void Start()
        {
            if (autoStart)
            {
                StartStream();
            }
        }

        private void OnDestroy()
        {
            StopStream();
        }

        /// <summary>
        /// Start the MJPEG streaming server
        /// </summary>
        public void StartStream()
        {
            if (isRunning) return;

            try
            {
                // Start HTTP listener
                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"http://*:{port}/stream/");
                httpListener.Start();
                isRunning = true;

                // Start listener thread
                listenerThread = new Thread(ListenForConnections);
                listenerThread.IsBackground = true;
                listenerThread.Start();

                // Start camera capture coroutine
                captureCoroutine = StartCoroutine(CaptureFrames());

                // Debug.Log($"MJPEG Stream Server started on port {port}");
            
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to start MJPEG server: {e.Message}");
                isRunning = false;
            }
        }

        /// <summary>
        /// Stop the streaming server
        /// </summary>
        public void StopStream()
        {
            if (!isRunning) return;

            isRunning = false;

            // Stop capture coroutine
            if (captureCoroutine != null)
            {
                StopCoroutine(captureCoroutine);
                captureCoroutine = null;
            }

            // Close all active streams
            lock (activeStreams)
            {
                foreach (var stream in activeStreams)
                {
                    try { stream.Close(); } catch { }
                }
                activeStreams.Clear();
            }

            // Stop HTTP listener
            if (httpListener != null)
            {
                httpListener.Stop();
                httpListener.Close();
            }

            // Stop listener thread
            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Join(1000);
            }

            Debug.Log("MJPEG Stream Server stopped");
        }

        /// <summary>
        /// Listen for incoming HTTP connections
        /// </summary>
        private void ListenForConnections()
        {
            while (isRunning)
            {
                try
                {
                    var context = httpListener.GetContext();
                    ThreadPool.QueueUserWorkItem(HandleClient, context);
                }
                catch (Exception e)
                {
                    if (isRunning)
                    {
                        Debug.LogError($"Error accepting connection: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Handle a client connection and stream MJPEG
        /// </summary>
        private void HandleClient(object obj)
        {
            var context = obj as HttpListenerContext;
            if (context == null) return;

            StreamWriter writer = null;

            try
            {
                var response = context.Response;
                
                // Set MJPEG headers
                response.ContentType = "multipart/x-mixed-replace; boundary=--boundary";
                response.StatusCode = 200;
                response.KeepAlive = true;

                writer = new StreamWriter(response.OutputStream, Encoding.ASCII);
                writer.AutoFlush = true;

                // Add to active streams
                lock (activeStreams)
                {
                    activeStreams.Add(writer);
                }

                Debug.Log($"Client connected to MJPEG stream. Active clients: {activeStreams.Count}");

                // Keep connection alive and send frames
                while (isRunning)
                {
                    byte[] frame;
                    lock (frameLock)
                    {
                        frame = currentFrame;
                    }

                    if (frame != null && frame.Length > 0)
                    {
                        try
                        {
                            // Write MJPEG frame
                            writer.WriteLine("--boundary");
                            writer.WriteLine("Content-Type: image/jpeg");
                            writer.WriteLine($"Content-Length: {frame.Length}");
                            writer.WriteLine();
                            writer.Flush();

                            // Write binary JPEG data
                            response.OutputStream.Write(frame, 0, frame.Length);
                            response.OutputStream.Flush();

                            writer.WriteLine();
                            writer.Flush();
                        }
                        catch
                        {
                            break; // Client disconnected
                        }
                    }

                    Thread.Sleep(1000 / targetFPS);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Client disconnected: {e.Message}");
            }
            finally
            {
                // Remove from active streams
                if (writer != null)
                {
                    lock (activeStreams)
                    {
                        activeStreams.Remove(writer);
                    }
                    try { writer.Close(); } catch { }
                }

                Debug.Log($"Client disconnected. Active clients: {activeStreams.Count}");
            }
        }

        /// <summary>
        /// Capture camera frames continuously
        /// </summary>
        private IEnumerator CaptureFrames()
        {
            while (isRunning)
            {
                // Only capture if we have active viewers
                bool hasViewers = false;
                lock (activeStreams)
                {
                    hasViewers = activeStreams.Count > 0;
                }

                if (hasViewers)
                {
                    yield return new WaitForEndOfFrame();

                    Camera cam = Camera.main;
                    if (cam != null)
                    {
                        // Create render texture
                        RenderTexture rt = new RenderTexture(streamWidth, streamHeight, 24);
                        RenderTexture currentRT = cam.targetTexture;
                        
                        cam.targetTexture = rt;
                        cam.Render();
                        
                        RenderTexture.active = rt;
                        
                        // Read pixels
                        Texture2D screenshot = new Texture2D(streamWidth, streamHeight, TextureFormat.RGB24, false);
                        screenshot.ReadPixels(new Rect(0, 0, streamWidth, streamHeight), 0, 0);
                        screenshot.Apply();
                        
                        // Restore camera
                        cam.targetTexture = currentRT;
                        RenderTexture.active = null;
                        
                        // Encode to JPEG
                        byte[] bytes = screenshot.EncodeToJPG(jpegQuality);
                        
                        // Update current frame
                        lock (frameLock)
                        {
                            currentFrame = bytes;
                        }
                        
                        // Cleanup
                        Destroy(rt);
                        Destroy(screenshot);
                    }
                }

                yield return new WaitForSeconds(1f / targetFPS);
            }
        }

        /// <summary>
        /// Get the stream URL
        /// </summary>
        public string GetStreamURL()
        {
            string localIP = GetLocalIPAddress();
            return $"http://{localIP}:{port}/stream/";
        }

        /// <summary>
        /// Get local IP address
        /// </summary>
        private string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "localhost";
        }
    }
}
