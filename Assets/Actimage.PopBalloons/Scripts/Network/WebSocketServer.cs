using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using PopBalloons;
using PopBalloons.Utilities;

/// <summary>
/// Simple WebSocket server to receive commands from the companion web app.
/// Allows remote control of the game from any device on the network.
/// </summary>
public class WebSocketServer : MonoBehaviour
    {
        [Header("Server Configuration")]
        [SerializeField] private int port = 8080;
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool sendStatsUpdates = true;
        [SerializeField] private float statsUpdateInterval = 1f;

        [Header("Hand Tracking")]
        [SerializeField] private bool sendHandTrackingData = false;
        [SerializeField] private float handTrackingInterval = 0.1f; // 10 Hz by default

        private TcpListener server;
        private Thread listenerThread;
        private List<TcpClient> clients = new List<TcpClient>();
        private bool isRunning = false;
        private float lastStatsUpdate = 0f;
        private float lastHandTrackingUpdate = 0f;
        private UnityMainThreadDispatcher dispatcher;
        private PopBalloons.Network.HandTrackingDataCollector handTrackingCollector;

        private void Start()
        {
            FileLogger.Log("===========================================");
            FileLogger.Log("🎮 POPBALLOONS APP - STARTING");
            FileLogger.Log("🔧 WebSocketServer.Start() called");
#if UNITY_EDITOR
            FileLogger.Log("🎮 Running in Unity Editor");
#else
            FileLogger.Log("🎮 Running on Device");
#endif
            FileLogger.Log("===========================================");

            // Initialize the main thread dispatcher FIRST on the main thread
            dispatcher = UnityMainThreadDispatcher.Instance();
            FileLogger.Log("✅ Main thread dispatcher initialized");
            
            // Initialize hand tracking collector
            handTrackingCollector = gameObject.AddComponent<PopBalloons.Network.HandTrackingDataCollector>();
            FileLogger.Log("✅ Hand tracking collector initialized");
            
            // Subscribe to profile change events
            PopBalloons.Data.ProfilesManager.OnProfileChanged += HandleProfileChanged;
            FileLogger.Log("✅ Profile change events subscribed");
            
            if (autoStart)
            {
                FileLogger.Log($"⚙️ AutoStart is enabled, starting server...");
                StartServer();
            }
            else
            {
                FileLogger.LogWarning("⚠️ AutoStart is DISABLED - Server will NOT start automatically!");
            }
        }

        private void Update()
        {
            // Send stats updates periodically
            if (sendStatsUpdates && isRunning && Time.time - lastStatsUpdate > statsUpdateInterval)
            {
                SendStats();
                lastStatsUpdate = Time.time;
            }

            // Send hand tracking data periodically
            if (sendHandTrackingData && isRunning && Time.time - lastHandTrackingUpdate > handTrackingInterval)
            {
                SendHandTrackingData();
                lastHandTrackingUpdate = Time.time;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            PopBalloons.Data.ProfilesManager.OnProfileChanged -= HandleProfileChanged;
            StopServer();
        }

        /// <summary>
        /// Handle profile change event and broadcast to all clients
        /// </summary>
        private void HandleProfileChanged(PopBalloons.Data.PlayerProfile profile)
        {
            // Make sure we execute on main thread
            dispatcher.Enqueue(() =>
            {
                BroadcastProfileUpdate();
            });
        }

        /// <summary>
        /// Start the WebSocket server
        /// </summary>
        public void StartServer()
        {
            if (isRunning) return;

            FileLogger.Log("===========================================");
            FileLogger.Log("🌐 WEBSOCKET SERVER - STARTING...");
#if UNITY_EDITOR
            FileLogger.Log("🎮 Running in Unity Editor - WebSocket enabled!");
#else
            FileLogger.Log("📱 Running on HoloLens Device");
#endif
            FileLogger.Log("===========================================");

            try
            {
                // Get local IP for display purposes
                string localIP = GetLocalIPAddress();
                FileLogger.Log($"🔍 Local IP: {localIP}");
                
                // On UWP/HoloLens, try different binding strategies
                IPAddress bindAddress = IPAddress.Any;
                FileLogger.Log($"📍 Attempting to bind to: IPAddress.Any (0.0.0.0)");
                
                server = new TcpListener(bindAddress, port);
                server.Start();
                isRunning = true;

                listenerThread = new Thread(ListenForClients);
                listenerThread.IsBackground = true;
                listenerThread.Start();

                FileLogger.Log("===========================================");
                FileLogger.Log($"✅ WEBSOCKET SERVER STARTED SUCCESSFULLY!");
                FileLogger.Log($"📡 Port: {port}");
                FileLogger.Log($"🌐 Local IPv4: {localIP}");
                FileLogger.Log($"🔗 Connect from web app using: ws://{localIP}:{port}");
                FileLogger.Log("===========================================");
            }
            catch (Exception e)
            {
                FileLogger.LogError("===========================================");
                FileLogger.LogError($"❌ FAILED TO START WEBSOCKET SERVER!");
                FileLogger.LogError($"💥 Error: {e.Message}");
                FileLogger.LogError($"📚 Stack: {e.StackTrace}");
                FileLogger.LogError("===========================================");
            }
        }

        /// <summary>
        /// Stop the WebSocket server
        /// </summary>
        public void StopServer()
        {
            isRunning = false;

            // Close all client connections
            lock (clients)
            {
                foreach (var client in clients)
                {
                    client?.Close();
                }
                clients.Clear();
            }

            // Stop the server
            server?.Stop();
            listenerThread?.Abort();}

        /// <summary>
        /// Listen for incoming client connections
        /// </summary>
        private void ListenForClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch (Exception e)
                {
                    if (isRunning)
                    {
                        Debug.LogError($"Error accepting client: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Handle individual client connection
        /// </summary>
        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            try
            {
                // Perform WebSocket handshake
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (request.Contains("Upgrade: websocket"))
                {
                    string response = PerformHandshake(request);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);

                    lock (clients)
                    {
                        clients.Add(client);
                    }

                    Debug.Log("Client connected to WebSocket");

                    // Send welcome message
                    SendMessageToClient(client, new ResponseMessage
                    {
                        type = "response",
                        data = new ResponseData { message = "Connected to PopBalloons HoloLens" }
                    });

                    // Send player profile
                    SendPlayerProfile(client);

                    // Listen for messages
                    while (isRunning && client.Connected)
                    {
                        if (stream.DataAvailable)
                        {
                            string message = ReceiveMessage(stream);
                            if (!string.IsNullOrEmpty(message))
                            {
                                ProcessMessage(message, client);
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Client handler error: {e.Message}");
            }
            finally
            {
                lock (clients)
                {
                    clients.Remove(client);
                }
                client.Close();
                Debug.Log("Client disconnected");
            }
        }

        /// <summary>
        /// Perform WebSocket handshake
        /// </summary>
        private string PerformHandshake(string request)
        {
            string key = "";
            string[] lines = request.Split('\n');
            foreach (string line in lines)
            {
                if (line.StartsWith("Sec-WebSocket-Key:"))
                {
                    key = line.Substring(19).Trim();
                    break;
                }
            }

            string acceptKey = Convert.ToBase64String(
                System.Security.Cryptography.SHA1.Create().ComputeHash(
                    Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")
                )
            );

            return "HTTP/1.1 101 Switching Protocols\r\n" +
                   "Upgrade: websocket\r\n" +
                   "Connection: Upgrade\r\n" +
                   $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";
        }

        /// <summary>
        /// Receive WebSocket message
        /// </summary>
        private string ReceiveMessage(NetworkStream stream)
        {
            try
            {
                byte[] buffer = new byte[2];
                int bytesRead = stream.Read(buffer, 0, 2);
                
                if (bytesRead < 2)
                {
                    Debug.LogWarning($"Not enough data in WebSocket frame header. Read {bytesRead} bytes.");
                    return null;
                }

                bool fin = (buffer[0] & 0b10000000) != 0;
                bool mask = (buffer[1] & 0b10000000) != 0;
                int payloadLen = buffer[1] & 0b01111111;

                if (payloadLen == 126)
                {
                    byte[] len = new byte[2];
                    bytesRead = stream.Read(len, 0, 2);
                    if (bytesRead < 2)
                    {
                        Debug.LogWarning("Not enough data for extended payload length.");
                        return null;
                    }
                    payloadLen = (len[0] << 8) | len[1];
                }

                byte[] maskKey = new byte[4];
                if (mask)
                {
                    bytesRead = stream.Read(maskKey, 0, 4);
                    if (bytesRead < 4)
                    {
                        Debug.LogWarning("Not enough data for mask key.");
                        return null;
                    }
                }

                if (payloadLen == 0)
                {
                    return string.Empty;
                }

                byte[] payload = new byte[payloadLen];
                bytesRead = stream.Read(payload, 0, payloadLen);
                
                if (bytesRead < payloadLen)
                {
                    Debug.LogWarning($"Incomplete payload. Expected {payloadLen}, got {bytesRead}");
                    return null;
                }

                if (mask)
                {
                    for (int i = 0; i < payload.Length; i++)
                    {
                        payload[i] = (byte)(payload[i] ^ maskKey[i % 4]);
                    }
                }

                string message = Encoding.UTF8.GetString(payload);
                
                // Ignore empty or invalid messages
                if (string.IsNullOrWhiteSpace(message) || message.Length < 2)
                {
                    return null;
                }
                
                return message;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error receiving WebSocket message: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Send WebSocket message to client
        /// </summary>
        private void SendMessageToClient(TcpClient client, object data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                
                byte[] payload = Encoding.UTF8.GetBytes(json);
                byte[] frame;

                if (payload.Length < 126)
                {
                    frame = new byte[payload.Length + 2];
                    frame[0] = 0b10000001; // FIN + Text frame
                    frame[1] = (byte)payload.Length;
                    Array.Copy(payload, 0, frame, 2, payload.Length);
                }
                else if (payload.Length < 65536)
                {
                    frame = new byte[payload.Length + 4];
                    frame[0] = 0b10000001; // FIN + Text frame
                    frame[1] = 126;
                    frame[2] = (byte)(payload.Length >> 8);
                    frame[3] = (byte)(payload.Length & 0xFF);
                    Array.Copy(payload, 0, frame, 4, payload.Length);
                }
                else
                {
                    frame = new byte[payload.Length + 10];
                    frame[0] = 0b10000001; // FIN + Text frame
                    frame[1] = 127;
                    for (int i = 0; i < 8; i++)
                    {
                        frame[9 - i] = (byte)((payload.Length >> (8 * i)) & 0xFF);
                    }
                    Array.Copy(payload, 0, frame, 10, payload.Length);
                }

                NetworkStream stream = client.GetStream();
                stream.Write(frame, 0, frame.Length);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Client disconnected or error sending message: {e.Message}");
                // Remove disconnected client
                lock (clients)
                {
                    clients.Remove(client);
                }
                try { client.Close(); } catch { }
            }
        }

        /// <summary>
        /// Broadcast message to all connected clients
        /// </summary>
        private void BroadcastMessage(object data)
        {
            lock (clients)
            {
                var disconnectedClients = new List<TcpClient>();
                
                foreach (var client in clients.ToArray())
                {
                    if (client.Connected)
                    {
                        try
                        {
                            SendMessageToClient(client, data);
                        }
                        catch
                        {
                            disconnectedClients.Add(client);
                        }
                    }
                    else
                    {
                        disconnectedClients.Add(client);
                    }
                }
                
                // Clean up disconnected clients
                foreach (var client in disconnectedClients)
                {
                    clients.Remove(client);
                    try { client.Close(); } catch { }
                }
            }
        }

        /// <summary>
        /// Send raw JSON string to client
        /// </summary>
        private void SendRawJsonToClient(TcpClient client, string json)
        {
            try
            {
                if (client == null || !client.Connected)
                {
                    return; // Client already disconnected, ignore silently
                }
                
                byte[] payload = Encoding.UTF8.GetBytes(json);
                byte[] frame;

                if (payload.Length < 126)
                {
                    frame = new byte[payload.Length + 2];
                    frame[0] = 0b10000001; // FIN + Text frame
                    frame[1] = (byte)payload.Length;
                    Array.Copy(payload, 0, frame, 2, payload.Length);
                }
                else if (payload.Length < 65536)
                {
                    frame = new byte[payload.Length + 4];
                    frame[0] = 0b10000001; // FIN + Text frame
                    frame[1] = 126;
                    frame[2] = (byte)(payload.Length >> 8);
                    frame[3] = (byte)(payload.Length & 0xFF);
                    Array.Copy(payload, 0, frame, 4, payload.Length);
                }
                else
                {
                    frame = new byte[payload.Length + 10];
                    frame[0] = 0b10000001; // FIN + Text frame
                    frame[1] = 127;
                    for (int i = 0; i < 8; i++)
                    {
                        frame[9 - i] = (byte)((payload.Length >> (8 * i)) & 0xFF);
                    }
                    Array.Copy(payload, 0, frame, 10, payload.Length);
                }

                NetworkStream stream = client.GetStream();
                stream.Write(frame, 0, frame.Length);
            }
            catch (Exception)
            {
                // Ignore connection errors silently - client likely disconnected
            }
        }

        /// <summary>
        /// Broadcast raw JSON to all connected clients
        /// </summary>
        private void BroadcastRawJson(string json)
        {
            lock (clients)
            {
                var disconnectedClients = new List<TcpClient>();
                
                foreach (var client in clients.ToArray())
                {
                    if (client != null && client.Connected)
                    {
                        try
                        {
                            SendRawJsonToClient(client, json);
                        }
                        catch
                        {
                            // Client disconnected, mark for removal
                            disconnectedClients.Add(client);
                        }
                    }
                    else
                    {
                        disconnectedClients.Add(client);
                    }
                }
                
                // Clean up disconnected clients silently
                foreach (var client in disconnectedClients)
                {
                    clients.Remove(client);
                    try { client?.Close(); } catch { }
                }
            }
        }

        /// <summary>
        /// Process incoming message from web app
        /// </summary>
        private void ProcessMessage(string message, TcpClient client)
        {
            try
            {
                // Ignore null, empty, or very short messages
                if (string.IsNullOrWhiteSpace(message) || message.Length < 2)
                {
                    return;
                }
                
                // Ignore messages with invalid Unicode characters (replacement character �)
                if (message.Contains("\uFFFD") || message.Contains("�"))
                {
                    // Just ignore silently - this happens when messages overlap
                    return;
                }
                
                // Ignore messages that don't look like JSON (should start with { or [)
                message = message.Trim();
                if (!message.StartsWith("{") && !message.StartsWith("["))
                {
                    Debug.LogWarning($"Ignoring non-JSON message: {message}");
                    return;
                }
                
                // For simple commands without data, parse differently
                if (message.Contains("\"goHome\"") || message.Contains("\"quitGame\"") || message.Contains("\"getProfile\"") || 
                    message.Contains("\"toggleCameraFeed\"") || message.Contains("\"startHandTracking\"") || 
                    message.Contains("\"stopHandTracking\"") || message.Contains("\"getHandTrackingFrame\""))
                {
                    var simpleMsg = JsonUtility.FromJson<SimpleMessage>(message);
                    
                    switch (simpleMsg.type)
                    {
                        case "goHome":
                            HandleGoHome(client);
                            break;
                        case "quitGame":
                            HandleQuitGame(client);
                            break;
                        case "getProfile":
                            SendPlayerProfile(client);
                            break;
                        case "toggleCameraFeed":
                            // Must run on main thread to use FindObjectOfType
                            dispatcher.Enqueue(() =>
                            {
                                var mjpegServer = FindObjectOfType<PopBalloons.Network.MJPEGStreamServer>();
                                if (mjpegServer != null)
                                {
                                    string streamURL = mjpegServer.GetStreamURL();
                                    string jsonResponse = $"{{\"type\":\"cameraFeedURL\",\"data\":{{\"url\":\"{streamURL}\"}}}}";
                                    SendRawJsonToClient(client, jsonResponse);
                                }
                                else
                                {
                                    SendMessageToClient(client, new ResponseMessage
                                    {
                                        type = "response",
                                        data = new ResponseData { message = "Serveur MJPEG non trouvé. Ajoutez le composant MJPEGStreamServer dans Unity." }
                                    });
                                }
                            });
                            break;
                        case "startHandTracking":
                            sendHandTrackingData = true;
                            Debug.Log("🤲 Hand tracking STARTED - will send updates every " + handTrackingInterval + "s");
                            SendMessageToClient(client, new ResponseMessage
                            {
                                type = "response",
                                data = new ResponseData { message = "Hand tracking started" }
                            });
                            break;
                        case "stopHandTracking":
                            sendHandTrackingData = false;
                            Debug.Log("🤲 Hand tracking STOPPED");
                            SendMessageToClient(client, new ResponseMessage
                            {
                                type = "response",
                                data = new ResponseData { message = "Hand tracking stopped" }
                            });
                            break;
                        case "getHandTrackingFrame":
                            Debug.Log("📸 Capturing single hand tracking frame");
                            SendHandTrackingData(client);
                            break;
                    }
                    return;
                }
                
                // For messages with data
                var simpleMsg2 = JsonUtility.FromJson<SimpleMessage>(message);

                switch (simpleMsg2.type)
                {
                    case "startGame":
                        var startGameMsg = JsonUtility.FromJson<StartGameMessage>(message);
                        HandleStartGame(startGameMsg.data, client);
                        break;
                    case "getProfiles":
                        HandleGetProfiles(client);
                        break;
                    case "selectProfile":
                        var selectProfileMsg = JsonUtility.FromJson<SelectProfileMessage>(message);
                        HandleSelectProfile(selectProfileMsg.data, client);
                        break;
                    case "createDebugProfile":
                        HandleCreateDebugProfile(client);
                        break;
                    default:
                        SendMessageToClient(client, new ErrorMessage
                        {
                            type = "error",
                            data = new ErrorData { message = $"Unknown command: {simpleMsg2.type}" }
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing message: {e.Message}");
                Debug.LogError($"Message was: {message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
            }
        }

        /// <summary>
        /// Handle start game command
        /// </summary>
        private void HandleStartGame(StartGameData data, TcpClient client)
        {
            dispatcher.Enqueue(() =>
            {
                try
                {
                    GameManager.GameType gameType = (GameManager.GameType)Enum.Parse(typeof(GameManager.GameType), data.gameType);
                    
                    // Apply FreePlay settings if provided
                    if (data.freePlaySettings != null && GameCreator.Instance != null)
                    {
                        GameCreator.Instance.SetFreePlaySettings(
                            data.freePlaySettings.spawnInterval,
                            data.freePlaySettings.maxSimultaneous
                        );
                        Debug.Log($"FreePlay settings applied: Interval={data.freePlaySettings.spawnInterval}s, Max={data.freePlaySettings.maxSimultaneous}");
                    }
                    
                    // Start game immediately (no countdown for now)
                    GameManager.Instance.NewGame(gameType, data.level);
                    
                    SendMessageToClient(client, new ResponseMessage
                    {
                        type = "response",
                        data = new ResponseData { message = $"Started {data.gameType} level {data.level}" }
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error starting game: {e.Message}");
                    SendMessageToClient(client, new ErrorMessage
                    {
                        type = "error",
                        data = new ErrorData { message = $"Error: {e.Message}" }
                    });
                }
            });
        }

        /// <summary>
        /// Countdown coroutine before starting game
        /// </summary>
        private IEnumerator CountdownThenStart(GameManager.GameType gameType, int level, int countdownDuration, TcpClient client, StartGameData data)
        {
            for (int i = countdownDuration; i > 0; i--)
            {
                // Check if user went back to home during countdown
                if (GameManager.Instance.CurrentState == GameManager.GameState.HOME)
                {
                    Debug.Log("Countdown cancelled - returned to home");
                    yield break;
                }
                
                Debug.Log($"Countdown: {i}");
                BroadcastMessage(new ResponseMessage
                {
                    type = "countdown",
                    data = new ResponseData { message = i.ToString() }
                });
                yield return new WaitForSeconds(1f);
            }
            
            // Final check before starting game
            if (GameManager.Instance.CurrentState == GameManager.GameState.HOME)
            {
                Debug.Log("Game start cancelled - returned to home");
                yield break;
            }
            
            // Send GO!
            BroadcastMessage(new ResponseMessage
            {
                type = "countdown",
                data = new ResponseData { message = "GO!" }
            });
            
            yield return new WaitForSeconds(0.5f);
            
            // Start the game
            GameManager.Instance.NewGame(gameType, level);
            SendMessageToClient(client, new ResponseMessage
            {
                type = "response",
                data = new ResponseData { message = $"Started {data.gameType} level {data.level}" }
            });
        }

        /// <summary>
        /// Handle go home command
        /// </summary>
        private void HandleGoHome(TcpClient client)
        {
            dispatcher.Enqueue(() =>
            {
                try
                {
                    GameManager.Instance.Home();
                    
                    SendMessageToClient(client, new ResponseMessage
                    {
                        type = "response",
                        data = new ResponseData { message = "Returned to home menu" }
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error going home: {e.Message}");
                }
            });
        }

        /// <summary>
        /// Handle quit game command
        /// </summary>
        private void HandleQuitGame(TcpClient client)
        {
            dispatcher.Enqueue(() =>
            {
                try
                {
                    if (GameCreator.Instance != null)
                    {
                        GameCreator.Instance.QuitLevel();
                    }
                    GameManager.Instance.Home();
                    
                    SendMessageToClient(client, new ResponseMessage
                    {
                        type = "response",
                        data = new ResponseData { message = "Game stopped" }
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error quitting game: {e.Message}");
                }
            });
        }

        /// <summary>
        /// Escape string for JSON
        /// </summary>
        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            
            var sb = new System.Text.StringBuilder();
            foreach (char c in str)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        // Handle control characters and non-ASCII
                        if (c < 32 || c > 127)
                        {
                            sb.AppendFormat("\\u{0:x4}", (int)c);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Send player profile to a client
        /// </summary>
        private void SendPlayerProfile(TcpClient client)
        {
            try
            {
                if (PopBalloons.Data.ProfilesManager.Instance != null && 
                    PopBalloons.Data.ProfilesManager.Instance.CurrentProfile != null)
                {
                    var profile = PopBalloons.Data.ProfilesManager.Instance.CurrentProfile;
                    var profileData = profile.data;

                    if (profileData != null)
                    {
                        // Convert levelsInfo to LevelInfo array
                        var levels = new System.Collections.Generic.List<LevelInfo>();
                        if (profile.levelsInfo != null)
                        {
                            foreach (var level in profile.levelsInfo)
                            {
                                levels.Add(new LevelInfo
                                {
                                    name = level.name,
                                    score = level.score
                                });
                            }
                        }

                        var profileMessage = new ProfileMessage
                        {
                            type = "profile",
                            data = new ProfileData
                            {
                                id = profileData.id,
                                username = profileData.username,
                                avatar = profileData.avatar != null ? new AvatarInfo
                                {
                                    colorOption = profileData.avatar.colorOption,
                                    eyeOption = profileData.avatar.eyeOption,
                                    accessoryOption = profileData.avatar.accessoryOption
                                } : null,
                                levels = levels.ToArray()
                            }
                        };
                        
                        // Build JSON manually to ensure arrays are serialized correctly
                        string levelsJson = "[";
                        for (int i = 0; i < levels.Count; i++)
                        {
                            if (i > 0) levelsJson += ",";
                            levelsJson += $"{{\"name\":\"{EscapeJsonString(levels[i].name)}\",\"score\":{levels[i].score}}}";
                        }
                        levelsJson += "]";
                        
                        string avatarJson = profileData.avatar != null ? 
                            $"{{\"colorOption\":{profileData.avatar.colorOption},\"eyeOption\":{profileData.avatar.eyeOption},\"accessoryOption\":{profileData.avatar.accessoryOption}}}" : 
                            "null";
                        
                        string profileJson = $"{{\"type\":\"profile\",\"data\":{{\"id\":\"{EscapeJsonString(profileData.id)}\",\"username\":\"{EscapeJsonString(profileData.username)}\",\"avatar\":{avatarJson},\"levels\":{levelsJson}}}}}";
                        
                        SendRawJsonToClient(client, profileJson);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sending player profile: {e.Message}");
            }
        }

        /// <summary>
        /// Handle get profiles command - send all available profiles to the client
        /// </summary>
        private void HandleGetProfiles(TcpClient client)
        {
            try
            {
                if (PopBalloons.Data.ProfilesManager.Instance != null)
                {
                    var profiles = PopBalloons.Data.ProfilesManager.Instance.GetAvailableProfiles();
                    var profilesList = new System.Collections.Generic.List<ProfileData>();

                    foreach (var profile in profiles)
                        {
                            if (profile.data != null)
                            {
                                profilesList.Add(new ProfileData
                                {
                                    id = profile.data.id,
                                    username = profile.data.username,
                                    avatar = profile.data.avatar != null ? new AvatarInfo
                                    {
                                        colorOption = profile.data.avatar.colorOption,
                                        eyeOption = profile.data.avatar.eyeOption,
                                        accessoryOption = profile.data.avatar.accessoryOption
                                    } : null
                                });
                            }
                        }

                        var message = new ProfilesListMessage
                        {
                            type = "profilesList",
                            data = new ProfilesListData { profiles = profilesList.ToArray() }
                        };

                        // Build JSON manually for profiles list
                        string profilesArrayJson = "[";
                        for (int i = 0; i < profilesList.Count; i++)
                        {
                            if (i > 0) profilesArrayJson += ",";
                            
                            var p = profilesList[i];
                            string avatarJson = p.avatar != null ? 
                                $"{{\"colorOption\":{p.avatar.colorOption},\"eyeOption\":{p.avatar.eyeOption},\"accessoryOption\":{p.avatar.accessoryOption}}}" : 
                                "null";
                            
                            profilesArrayJson += $"{{\"id\":\"{EscapeJsonString(p.id)}\",\"username\":\"{EscapeJsonString(p.username)}\",\"avatar\":{avatarJson}}}";
                        }
                        profilesArrayJson += "]";
                        
                    string profilesListJson = $"{{\"type\":\"profilesList\",\"data\":{{\"profiles\":{profilesArrayJson}}}}}";
                    
                    SendRawJsonToClient(client, profilesListJson);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting profiles: {e.Message}");
                SendMessageToClient(client, new ErrorMessage
                {
                    type = "error",
                    data = new ErrorData { message = $"Error: {e.Message}" }
                });
            }
        }        /// <summary>
        /// Handle select profile command
        /// </summary>
        private void HandleSelectProfile(SelectProfileData data, TcpClient client)
        {
            dispatcher.Enqueue(() =>
            {
                try
                {
                    if (PopBalloons.Data.ProfilesManager.Instance != null)
                    {
                        var profiles = PopBalloons.Data.ProfilesManager.Instance.GetAvailableProfiles();
                        var selectedProfile = profiles.Find(p => p.data != null && p.data.id == data.profileId);

                        if (selectedProfile != null)
                        {
                            PopBalloons.Data.ProfilesManager.Instance.SetCurrentProfile(selectedProfile);
                            
                            SendMessageToClient(client, new ResponseMessage
                            {
                                type = "response",
                                data = new ResponseData { message = $"Profile changed to {selectedProfile.data.username}" }
                            });
                        }
                        else
                        {
                            SendMessageToClient(client, new ErrorMessage
                            {
                                type = "error",
                                data = new ErrorData { message = "Profile not found" }
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error selecting profile: {e.Message}");
                    SendMessageToClient(client, new ErrorMessage
                    {
                        type = "error",
                        data = new ErrorData { message = $"Error: {e.Message}" }
                    });
                }
            });
        }

        /// <summary>
        /// Handle create debug profile command
        /// </summary>
        private void HandleCreateDebugProfile(TcpClient client)
        {
            dispatcher.Enqueue(() =>
            {
                try
                {
                    if (PopBalloons.Data.ProfilesManager.Instance != null)
                    {
                        var debugProfile = PopBalloons.Data.ProfilesManager.Instance.CreateDebugProfile();
                        
                        SendMessageToClient(client, new ResponseMessage
                        {
                            type = "response",
                            data = new ResponseData { message = $"Debug profile created with {debugProfile.levelsInfo.Count} levels" }
                        });

                        // Send updated profiles list to all clients
                        HandleGetProfiles(client);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error creating debug profile: {e.Message}");
                    SendMessageToClient(client, new ErrorMessage
                    {
                        type = "error",
                        data = new ErrorData { message = $"Error: {e.Message}" }
                    });
                }
            });
        }

        /// <summary>
        /// Broadcast profile update to all connected clients
        /// </summary>
        private void BroadcastProfileUpdate()
        {
            try
            {
                if (PopBalloons.Data.ProfilesManager.Instance != null && 
                    PopBalloons.Data.ProfilesManager.Instance.CurrentProfile != null)
                {
                    var profile = PopBalloons.Data.ProfilesManager.Instance.CurrentProfile;
                    var profileData = profile.data;

                    if (profileData != null)
                    {
                        // Convert levelsInfo to LevelInfo array
                        var levels = new System.Collections.Generic.List<LevelInfo>();
                        if (profile.levelsInfo != null)
                        {
                            foreach (var level in profile.levelsInfo)
                            {
                                levels.Add(new LevelInfo
                                {
                                    name = level.name,
                                    score = level.score
                                });
                            }
                        }

                        var profileMessage = new ProfileMessage
                        {
                            type = "profile",
                            data = new ProfileData
                            {
                                id = profileData.id,
                                username = profileData.username,
                                avatar = profileData.avatar != null ? new AvatarInfo
                                {
                                    colorOption = profileData.avatar.colorOption,
                                    eyeOption = profileData.avatar.eyeOption,
                                    accessoryOption = profileData.avatar.accessoryOption
                                } : null,
                                levels = levels.ToArray()
                            }
                        };
                        
                        // Build JSON manually to ensure arrays are serialized correctly
                        string levelsJson = "[";
                        for (int i = 0; i < levels.Count; i++)
                        {
                            if (i > 0) levelsJson += ",";
                            levelsJson += $"{{\"name\":\"{EscapeJsonString(levels[i].name)}\",\"score\":{levels[i].score}}}";
                        }
                        levelsJson += "]";
                        
                        string avatarJson = profileData.avatar != null ? 
                            $"{{\"colorOption\":{profileData.avatar.colorOption},\"eyeOption\":{profileData.avatar.eyeOption},\"accessoryOption\":{profileData.avatar.accessoryOption}}}" : 
                            "null";
                        
                        string profileJson = $"{{\"type\":\"profile\",\"data\":{{\"id\":\"{EscapeJsonString(profileData.id)}\",\"username\":\"{EscapeJsonString(profileData.username)}\",\"avatar\":{avatarJson},\"levels\":{levelsJson}}}}}";
                        
                        BroadcastRawJson(profileJson);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error broadcasting profile update: {e.Message}");
            }
        }

        /// <summary>
        /// Send current game stats to all clients
        /// </summary>
        private void SendStats()
        {
            if (GameManager.Instance.CurrentState == GameManager.GameState.PLAY)
            {
                int score = ScoreManager.instance != null ? ScoreManager.instance.score : 0;
                int balloons = GameCreator.Instance != null ? (int)GameCreator.Instance.BalloonDestroyed : 0;
                float time = TimerManager.GetTime();

                BroadcastMessage(new StatsMessage
                {
                    type = "stats",
                    data = new StatsData
                    {
                        score = score,
                        balloons = balloons,
                        time = time
                    }
                });
            }
        }

        /// <summary>
        /// Send hand tracking data to all clients
        /// </summary>
        private void SendHandTrackingData()
        {
            if (handTrackingCollector != null)
            {
                var frame = handTrackingCollector.GetCurrentFrame();
                
                // Debug: Log hand tracking status
                string leftStatus = (frame.leftHand != null && frame.leftHand.isTracked) ? $"✅ LEFT ({frame.leftHand.joints.Count} joints)" : "❌ LEFT not tracked";
                string rightStatus = (frame.rightHand != null && frame.rightHand.isTracked) ? $"✅ RIGHT ({frame.rightHand.joints.Count} joints)" : "❌ RIGHT not tracked";
                Debug.Log($"🤲 Hand tracking frame @ {frame.timestamp:F2}s | {leftStatus} | {rightStatus}");
                
                SendHandTrackingFrame(frame);
            }
            else
            {
                Debug.LogWarning("⚠️ Hand tracking collector is NULL!");
            }
        }

        /// <summary>
        /// Send hand tracking data to a specific client
        /// </summary>
        private void SendHandTrackingData(TcpClient client)
        {
            if (handTrackingCollector != null)
            {
                var frame = handTrackingCollector.GetCurrentFrame();
                SendHandTrackingFrameToClient(client, frame);
            }
        }

        /// <summary>
        /// Broadcast hand tracking frame to all clients
        /// </summary>
        private void SendHandTrackingFrame(PopBalloons.Network.HandTrackingFrame frame)
        {
            try
            {
                string json = BuildHandTrackingJson(frame);
                BroadcastRawJson(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error broadcasting hand tracking data: {e.Message}");
            }
        }

        /// <summary>
        /// Send hand tracking frame to specific client
        /// </summary>
        private void SendHandTrackingFrameToClient(TcpClient client, PopBalloons.Network.HandTrackingFrame frame)
        {
            try
            {
                string json = BuildHandTrackingJson(frame);
                SendRawJsonToClient(client, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sending hand tracking data: {e.Message}");
            }
        }

        /// <summary>
        /// Build JSON string for hand tracking data manually (more efficient than JsonUtility for complex nested objects)
        /// </summary>
        private string BuildHandTrackingJson(PopBalloons.Network.HandTrackingFrame frame)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("{\"type\":\"handTracking\",\"data\":{");
            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "\"timestamp\":{0},\"timestampMs\":{1}", frame.timestamp, frame.timestampMs);
            
            // Left hand
            if (frame.leftHand != null && frame.leftHand.isTracked)
            {
                sb.Append(",\"leftHand\":");
                sb.Append(BuildHandDataJson(frame.leftHand));
            }
            else
            {
                sb.Append(",\"leftHand\":null");
            }
            
            // Right hand
            if (frame.rightHand != null && frame.rightHand.isTracked)
            {
                sb.Append(",\"rightHand\":");
                sb.Append(BuildHandDataJson(frame.rightHand));
            }
            else
            {
                sb.Append(",\"rightHand\":null");
            }
            
            sb.Append("}}");
            return sb.ToString();
        }

        /// <summary>
        /// Build JSON for a single hand's data
        /// </summary>
        private string BuildHandDataJson(PopBalloons.Network.HandData hand)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"handedness\":\"{0}\",\"isTracked\":{1},\"joints\":[", 
                EscapeJsonString(hand.handedness), 
                hand.isTracked.ToString().ToLower());
            
            for (int i = 0; i < hand.joints.Count; i++)
            {
                if (i > 0) sb.Append(",");
                var joint = hand.joints[i];
                sb.Append("{");
                sb.AppendFormat("\"jointName\":\"{0}\",", EscapeJsonString(joint.jointName));
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "\"position\":{{\"x\":{0},\"y\":{1},\"z\":{2}}},", 
                    joint.position.x, joint.position.y, joint.position.z);
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "\"rotation\":{{\"x\":{0},\"y\":{1},\"z\":{2},\"w\":{3}}}", 
                    joint.rotation.x, joint.rotation.y, joint.rotation.z, joint.rotation.w);
                sb.Append("}");
            }
            
            sb.Append("]}");
            return sb.ToString();
        }


        /// <summary>
        /// Get local IP address
        /// </summary>
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        #region Data Classes
        [Serializable]
        private class SimpleMessage
        {
            public string type;
        }

        [Serializable]
        private class StartGameMessage
        {
            public string type;
            public StartGameData data;
        }

        [Serializable]
        private class StartGameData
        {
            public string gameType;
            public int level;
            public FreePlaySettings freePlaySettings;
        }

        [Serializable]
        private class FreePlaySettings
        {
            public float spawnInterval = 1.5f;
            public int maxSimultaneous = 10;
        }

        [Serializable]
        private class ResponseData
        {
            public string message;
        }

        [Serializable]
        private class ResponseMessage
        {
            public string type;
            public ResponseData data;
        }

        [Serializable]
        private class StatsData
        {
            public int score;
            public int balloons;
            public float time;
        }

        [Serializable]
        private class StatsMessage
        {
            public string type;
            public StatsData data;
        }

        [Serializable]
        private class ErrorData
        {
            public string message;
        }

        [Serializable]
        private class ErrorMessage
        {
            public string type;
            public ErrorData data;
        }

        [Serializable]
        private class AvatarInfo
        {
            public int colorOption;
            public int eyeOption;
            public int accessoryOption;
        }

        [Serializable]
        private class LevelInfo
        {
            public string name;
            public int score;
        }

        [Serializable]
        private class ProfileData
        {
            public string id;
            public string username;
            public AvatarInfo avatar;
            public LevelInfo[] levels;
        }

        [Serializable]
        private class ProfileMessage
        {
            public string type;
            public ProfileData data;
        }

        [Serializable]
        private class ProfilesListData
        {
            public ProfileData[] profiles;
        }

        [Serializable]
        private class ProfilesListMessage
        {
            public string type;
            public ProfilesListData data;
        }

        [Serializable]
        private class SelectProfileData
        {
            public string profileId;
        }

        [Serializable]
        private class SelectProfileMessage
        {
            public string type;
            public SelectProfileData data;
        }
        #endregion
    }
