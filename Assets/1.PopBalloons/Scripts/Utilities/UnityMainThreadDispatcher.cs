using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows executing code on Unity's main thread from background threads.
/// Essential for WebSocket server to interact with Unity API.
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                // Try to find existing instance
                _instance = FindObjectOfType<UnityMainThreadDispatcher>();

                if (_instance == null)
                {
                    // Create new instance
                    GameObject dispatcherObject = new GameObject("UnityMainThreadDispatcher");
                    _instance = dispatcherObject.AddComponent<UnityMainThreadDispatcher>();
                    DontDestroyOnLoad(dispatcherObject);
                }
            }

            return _instance;
        }

        private void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        /// <summary>
        /// Enqueue an action to be executed on the main thread
        /// </summary>
        public void Enqueue(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }
    }
