using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// ScriptableObject-based event channel for decoupled communication between
    /// the UI layer, gameplay controllers, and service layer.
    /// <para>
    /// All events flow through this channel rather than direct references, keeping
    /// the architecture decoupled. Multiple listeners can subscribe to the same
    /// event type.
    /// </para>
    /// </summary>
    public sealed class NAADEventChannel : ScriptableObject
    {
        private static NAADEventChannel? _instance;
        private readonly Dictionary<int, List<Action<object?>>> _listeners = new();

        public static NAADEventChannel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<NAADEventChannel>("NAADEventChannel");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<NAADEventChannel>();
                        Debug.Log("[NAADEventChannel] Created new channel instance.");
                    }
                }
                return _instance;
            }
        }

        private int _nextId;

        /// <summary>Get the next event channel ID.</summary>
        private int NextId => Interlocked.Increment(ref _nextId);

        /// <summary>Subscribe to an event. Returns a subscription ID for unsubscribing.</summary>
        public int Subscribe(int eventId, Action<object?> listener)
        {
            if (!listener.Equals(null))
            {
                if (!_listeners.TryGetValue(eventId, out var listeners))
                {
                    listeners = new List<Action<object?>>();
                    _listeners[eventId] = listeners;
                }

                if (!listeners.Contains(listener))
                {
                    listeners.Add(listener);
                }
            }
            return 0;
        }

        /// <summary>Unsubscribe from an event.</summary>
        public void Unsubscribe(int eventId, Action<object?> listener)
        {
            if (_listeners.TryGetValue(eventId, out var listeners))
            {
                listeners.Remove(listener);
                if (listeners.Count == 0)
                {
                    _listeners.Remove(eventId);
                }
            }
        }

        /// <summary>Publish an event to all subscribers.</summary>
        public void Publish(int eventId, object? data = null)
        {
            if (_listeners.TryGetValue(eventId, out var listeners))
            {
                foreach (var listener in listeners)
                {
                    try
                    {
                        listener?.Invoke(data);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[NAADEventChannel] Listener threw: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Convenience event: PlayerLocationUpdated — fired when the player's
        /// location changes (e.g., after a TRAVEL command succeeds).
        /// </summary>
        public const int PlayerLocationUpdated = 1001;

        /// <summary>
        /// Convenience event: NightStatusChanged — fired when the night state changes.
        /// </summary>
        public const int NightStatusChanged = 1002;

        /// <summary>
        /// Convenience event: BalanceUpdated — fired when the player's balance changes.
        /// </summary>
        public const int BalanceUpdated = 1003;

        /// <summary>
        /// Convenience event: NewMessageReceived — fired when a new message arrives.
        /// </summary>
        public const int NewMessageReceived = 1004;

        /// <summary>
        /// Convenience event: InteractionRequested — fired when the player requests an interaction.
        /// </summary>
        public const int InteractionRequested = 1005;

        /// <summary>
        /// Convenience event: NightSummaryReady — fired when the night summary is generated.
        /// </summary>
        public const int NightSummaryReady = 1006;

        /// <summary>
        /// Convenience event: ConfigurationError — fired when configuration is invalid.
        /// </summary>
        public const int ConfigurationError = 1007;
    }
}