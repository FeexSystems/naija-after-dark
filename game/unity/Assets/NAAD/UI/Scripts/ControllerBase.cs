using System;
using UnityEngine;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// Base class for all NAAD UI controllers.
    /// <para>
    /// Provides a unified lifecycle (Awake, OnEnable, Start, OnDestroy),
    /// dependency injection via <see cref="Initialize"/>, and view wiring.
    /// Subclasses override the virtual methods as needed.
    /// </para>
    /// </summary>
    public abstract class ControllerBase : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        protected ScreenViewBase? _view;

        [Header("Services (Mock in tests, live in production)")]
        [SerializeField]
        protected IAuthService? _auth;

        [SerializeField]
        protected ICommandService? _commands;

        [SerializeField]
        protected IPlayerService? _players;

        [SerializeField]
        protected IWorldStateService? _world;

        [SerializeField]
        protected IPhoneService? _phone;

        [SerializeField]
        protected IDialogueService? _dialogue;

        protected NAADEventChannel EventChannel => NAADEventChannel.Instance;

        protected virtual void Awake()
        {
            // Register this controller with the service container (set in Manager)
            ServiceContainer.RegisterController(this);
        }

        protected virtual void OnEnable()
        {
            // Subscribe to event channel when the controller is active
            EventChannel.Subscribe(NAADEventChannel.PlayerLocationUpdated, OnPlayerLocationUpdated);
            EventChannel.Subscribe(NAADEventChannel.NightStatusChanged, OnNightStatusChanged);
            EventChannel.Subscribe(NAADEventChannel.BalanceUpdated, OnBalanceUpdated);
            EventChannel.Subscribe(NAADEventChannel.NewMessageReceived, OnNewMessageReceived);
            EventChannel.Subscribe(NAADEventChannel.InteractionRequested, OnInteractionRequested);
            EventChannel.Subscribe(NAADEventChannel.NightSummaryReady, OnNightSummaryReady);
            EventChannel.Subscribe(NAADEventChannel.ConfigurationError, OnConfigurationError);

            SubscribeViewEvents();
        }

        protected virtual void Start()
        {
            Initialize();
            Flow();
        }

        protected virtual void OnDestroy()
        {
            EventChannel.Unsubscribe(NAADEventChannel.PlayerLocationUpdated, OnPlayerLocationUpdated);
            EventChannel.Unsubscribe(NAADEventChannel.NightStatusChanged, OnNightStatusChanged);
            EventChannel.Unsubscribe(NAADEventChannel.BalanceUpdated, OnBalanceUpdated);
            EventChannel.Unsubscribe(NAADEventChannel.NewMessageReceived, OnNewMessageReceived);
            EventChannel.Unsubscribe(NAADEventChannel.InteractionRequested, OnInteractionRequested);
            EventChannel.Unsubscribe(NAADEventChannel.NightSummaryReady, OnNightSummaryReady);
            EventChannel.Unsubscribe(NAADEventChannel.ConfigurationError, OnConfigurationError);

            UnsubscribeViewEvents();
        }

        /// <summary>
        /// Override to inject dependencies (mock or live services).
        /// </summary>
        protected virtual void Initialize() { }

        /// <summary>
        /// Override to define the controller's startup flow.
        /// </summary>
        protected virtual void Flow() { }

        /// <summary>
        /// Called when the view is ready to receive data.
        /// </summary>
        protected virtual void OnViewReady() { }

        #region Event Handlers

        private void OnPlayerLocationUpdated(object? data)
        {
            var location = data as string;
            if (_view != null && location != null)
            {
                _view.SetState(ScreenState.Success);
                _view.SetState(ScreenState.Success);
                // Trigger view update via reflection or type-specific handler
            }
        }

        private void OnNightStatusChanged(object? data)
        {
            // Handle night status changes
        }

        private void OnBalanceUpdated(object? data)
        {
            if (_view != null)
            {
                // Update balance UI if view exposes a method
            }
        }

        private void OnNewMessageReceived(object? data)
        {
            // Handle new message
        }

        private void OnInteractionRequested(object? data)
        {
            // Handle interaction request
        }

        private void OnNightSummaryReady(object? data)
        {
            // Display night summary
        }

        private void OnConfigurationError(object? data)
        {
            // Show configuration error
        }

        protected virtual void SubscribeViewEvents() { }

        protected virtual void UnsubscribeViewEvents() { }

        #endregion
    }
}