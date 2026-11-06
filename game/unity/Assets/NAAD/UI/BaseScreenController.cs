using System;
using NAAD.UI.Scripts;
using UnityEngine;

namespace NAAD.UI
{
    /// <summary>
    /// Base class for all screen controllers.
    /// Provides common functionality for state management, service binding, and lifecycle.
    /// </summary>
    public abstract class BaseScreenController : MonoBehaviour
    {
        protected ScreenState CurrentState = ScreenState.Loading;
        protected string ErrorMessage = string.Empty;

        [Header("View")]
        [SerializeField]
        [Tooltip("The presentation layer this controller renders into. Bound at runtime by " +
                 "ScreenNavigationManager.RegisterControllersAuto() when it is not set in the prefab.")]
        private ScreenViewBase? _view;

        /// <summary>The bound presentation layer, or null when nothing has been bound yet.</summary>
        public ScreenViewBase? View => _view;

        /// <summary>
        /// Bind this controller to its view and render once.
        /// <para>
        /// Controllers used to manage their data with no way to reach a view at all, so
        /// every <c>ShowHud</c>/<c>UpdateHud</c>-style method was an empty comment. This is
        /// the link that was missing between <c>NAAD.UI.*Controller</c> and
        /// <c>NAAD.UI.Scripts.*View</c>.
        /// </para>
        /// </summary>
        public void BindView(ScreenViewBase view)
        {
            if (view == null) return;

            _view = view;
            OnViewBound(view);
            Render();
        }

        /// <summary>Called once after a view is bound. Override for type-specific setup.</summary>
        protected virtual void OnViewBound(ScreenViewBase view)
        {
        }

        /// <summary>
        /// Called when screen state changes.
        /// Override to update UI based on state.
        /// </summary>
        protected virtual void OnStateChanged(ScreenState newState, ScreenState oldState)
        {
            // Override in derived classes to update UI
        }

        /// <summary>
        /// Set the current screen state and trigger UI update.
        /// </summary>
        protected void SetState(ScreenState newState, string errorMessage = "")
        {
            var oldState = CurrentState;
            CurrentState = newState;
            
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ErrorMessage = errorMessage;
            }
            
            OnStateChanged(newState, oldState);

            // Push the new state into the view. Derived classes implement Render() (or their
            // own Update* method) to move data onto the bound view.
            Render();
        }

        /// <summary>
        /// Initialize the screen.
        /// Override to bind services and setup UI.
        /// </summary>
        protected virtual void Initialize()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Load data for the screen.
        /// Override to fetch data from services.
        /// </summary>
        protected virtual void LoadData()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Render the screen based on current state.
        /// Override to update UI elements.
        /// </summary>
        protected virtual void Render()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Handle user interaction.
        /// Override to respond to user input.
        /// </summary>
        protected virtual void OnInteract()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Cleanup screen resources.
        /// Override to unbind services and cleanup.
        /// </summary>
        protected virtual void Cleanup()
        {
            // Override in derived classes
        }

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void Start()
        {
            LoadData();
        }

        protected virtual void OnDestroy()
        {
            Cleanup();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Check if screen is in a loading state.
        /// </summary>
        protected bool IsLoading()
        {
            return CurrentState == ScreenState.Loading || CurrentState == ScreenState.Retry;
        }

        /// <summary>
        /// Check if screen is in an error state.
        /// </summary>
        protected bool IsError()
        {
            return CurrentState == ScreenState.Error;
        }

        /// <summary>
        /// Check if screen is in a success state.
        /// </summary>
        protected bool IsSuccess()
        {
            return CurrentState == ScreenState.Success;
        }

        /// <summary>
        /// Check if screen is in an empty state.
        /// </summary>
        protected bool IsEmpty()
        {
            return CurrentState == ScreenState.Empty;
        }

        #endregion
    }
}
