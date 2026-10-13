using System;
using UnityEngine;

namespace NAAD.Animation
{
    /// <summary>
    /// High-level state management for character animations.
    /// This layer provides a simpler API for game systems to interact with
    /// the animation system, handling state transitions and ensuring consistency.
    /// 
    /// This is the authoritative source for animation state - game systems
    /// should query this to understand current animation state.
    /// </summary>
    public class AnimationStateManager : MonoBehaviour
    {
        private CharacterAnimationController _animationController;
        private AnimationCommandType _currentState = AnimationCommandType.LocomotionIdle;
        private AnimationCommandType _previousState = AnimationCommandType.LocomotionIdle;
        private float _stateStartTime;

        [Header("State Tracking")]
        [SerializeField] private bool _isMoving = false;
        [SerializeField] private float _currentSpeed = 0f;
        [SerializeField] private bool _hasPhone = false;
        [SerializeField] private bool _isSeated = false;
        [SerializeField] private bool _isInVehicle = false;

        public event Action<AnimationCommandType, AnimationCommandType> OnStateChanged;

        private void Awake()
        {
            _animationController = GetComponent<CharacterAnimationController>();
            if (_animationController == null)
            {
                Debug.LogError("[AnimationStateManager] No CharacterAnimationController found");
            }
        }

        private void Start()
        {
            _stateStartTime = Time.time;
        }

        #region Public API

        /// <summary>
        /// Get the current animation state.
        /// </summary>
        public AnimationCommandType GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// Get the previous animation state.
        /// </summary>
        public AnimationCommandType GetPreviousState()
        {
            return _previousState;
        }

        /// <summary>
        /// Get how long the current state has been active.
        /// </summary>
        public float GetStateDuration()
        {
            return Time.time - _stateStartTime;
        }

        /// <summary>
        /// Check if the character is currently moving.
        /// </summary>
        public bool IsMoving()
        {
            return _isMoving;
        }

        /// <summary>
        /// Check if the character currently has a phone out.
        /// </summary>
        public bool HasPhone()
        {
            return _hasPhone;
        }

        /// <summary>
        /// Check if the character is currently seated.
        /// </summary>
        public bool IsSeated()
        {
            return _isSeated;
        }

        /// <summary>
        /// Check if the character is in a vehicle.
        /// </summary>
        public bool IsInVehicle()
        {
            return _isInVehicle;
        }

        #endregion

        #region Locomotion Commands

        /// <summary>
        /// Set the character to idle state.
        /// </summary>
        public void SetIdle()
        {
            if (_currentState == AnimationCommandType.LocomotionIdle)
                return;

            TransitionTo(AnimationCommandType.LocomotionIdle);
            _isMoving = false;
            _currentSpeed = 0f;
        }

        /// <summary>
        /// Set the character to walking state.
        /// </summary>
        public void SetWalk(float speed = 1f)
        {
            if (_currentState == AnimationCommandType.LocomotionWalk && Mathf.Approximately(_currentSpeed, speed))
                return;

            TransitionTo(AnimationCommand.LocomotionWalk(speed));
            _isMoving = true;
            _currentSpeed = speed;
        }

        /// <summary>
        /// Set the character to running state.
        /// </summary>
        public void SetRun(float speed = 1f)
        {
            if (_currentState == AnimationCommandType.LocomotionRun && Mathf.Approximately(_currentSpeed, speed))
                return;

            TransitionTo(AnimationCommand.LocomotionRun(speed));
            _isMoving = true;
            _currentSpeed = speed;
        }

        /// <summary>
        /// Stop all locomotion and return to idle.
        /// </summary>
        public void Stop()
        {
            if (!_isMoving)
                return;

            TransitionTo(AnimationCommand.Stop());
            _isMoving = false;
            _currentSpeed = 0f;
        }

        /// <summary>
        /// Turn left while standing or moving.
        /// </summary>
        public void TurnLeft()
        {
            TransitionTo(AnimationCommandType.LocomotionTurnLeft);
        }

        /// <summary>
        /// Turn right while standing or moving.
        /// </summary>
        public void TurnRight()
        {
            TransitionTo(AnimationCommandType.LocomotionTurnRight);
        }

        #endregion

        #region Social Commands

        /// <summary>
        /// Play a greeting animation.
        /// </summary>
        public void Greet()
        {
            TransitionTo(AnimationCommand.Greeting());
        }

        /// <summary>
        /// Start conversation animation.
        /// </summary>
        public void StartConversation()
        {
            TransitionTo(AnimationCommandType.SocialConversation);
        }

        /// <summary>
        /// Start listening animation.
        /// </summary>
        public void StartListening()
        {
            TransitionTo(AnimationCommandType.SocialListening);
        }

        /// <summary>
        /// Play laugh animation.
        /// </summary>
        public void Laugh()
        {
            TransitionTo(AnimationCommandType.SocialLaugh);
        }

        /// <summary>
        /// Play celebrate animation.
        /// </summary>
        public void Celebrate()
        {
            TransitionTo(AnimationCommandType.SocialCelebrate);
        }

        #endregion

        #region Phone Commands

        /// <summary>
        /// Retrieve phone from pocket/bag.
        /// </summary>
        public void RetrievePhone()
        {
            if (_hasPhone)
                return;

            TransitionTo(AnimationCommand.PhoneRetrieve());
        }

        /// <summary>
        /// Hold phone in hand (after retrieve).
        /// </summary>
        public void HoldPhone()
        {
            TransitionTo(AnimationCommand.PhoneHold());
            _hasPhone = true;
        }

        /// <summary>
        /// Browse phone (scrolling/tapping).
        /// </summary>
        public void BrowsePhone()
        {
            if (!_hasPhone)
            {
                RetrievePhone();
                return;
            }

            TransitionTo(AnimationCommand.PhoneBrowse());
        }

        /// <summary>
        /// Put phone away.
        /// </summary>
        public void PutAwayPhone()
        {
            if (!_hasPhone)
                return;

            TransitionTo(AnimationCommand.PhonePutAway());
            _hasPhone = false;
        }

        #endregion

        #region Food Commands

        /// <summary>
        /// Order food/drink.
        /// </summary>
        public void OrderFood()
        {
            TransitionTo(AnimationCommandType.FoodOrder);
        }

        /// <summary>
        /// Serve food/drink.
        /// </summary>
        public void ServeFood()
        {
            TransitionTo(AnimationCommandType.FoodServe);
        }

        /// <summary>
        /// Eat food.
        /// </summary>
        public void Eat()
        {
            TransitionTo(AnimationCommandType.FoodEat);
        }

        /// <summary>
        /// Drink beverage.
        /// </summary>
        public void Drink()
        {
            TransitionTo(AnimationCommandType.FoodDrink);
        }

        #endregion

        #region Nightlife Commands

        /// <summary>
        /// Start dancing.
        /// </summary>
        public void Dance()
        {
            TransitionTo(AnimationCommandType.NightlifeDance);
        }

        /// <summary>
        /// Cheer.
        /// </summary>
        public void Cheer()
        {
            TransitionTo(AnimationCommandType.NightlifeCheer);
        }

        /// <summary>
        /// Perform (DJ, singing, etc.).
        /// </summary>
        public void Perform()
        {
            TransitionTo(AnimationCommandType.NightlifePerform);
        }

        /// <summary>
        /// Socialise in group.
        /// </summary>
        public void Socialise()
        {
            TransitionTo(AnimationCommandType.NightlifeSocialise);
        }

        #endregion

        #region Seating Commands

        /// <summary>
        /// Sit down.
        /// </summary>
        public void Sit()
        {
            if (_isSeated)
                return;

            TransitionTo(AnimationCommandType.SeatingSit);
            _isSeated = true;
        }

        /// <summary>
        /// Idle while seated.
        /// </summary>
        public void IdleSeated()
        {
            if (!_isSeated)
                return;

            TransitionTo(AnimationCommandType.SeatingIdleSeated);
        }

        /// <summary>
        /// Stand up from seat.
        /// </summary>
        public void Stand()
        {
            if (!_isSeated)
                return;

            TransitionTo(AnimationCommandType.SeatingStand);
            _isSeated = false;
        }

        #endregion

        #region Vehicle Commands

        /// <summary>
        /// Enter vehicle.
        /// </summary>
        public void EnterVehicle()
        {
            TransitionTo(AnimationCommandType.VehicleEnter);
            _isInVehicle = true;
        }

        /// <summary>
        /// Exit vehicle.
        /// </summary>
        public void ExitVehicle()
        {
            if (!_isInVehicle)
                return;

            TransitionTo(AnimationCommandType.VehicleExit);
            _isInVehicle = false;
        }

        /// <summary>
        /// Sit as driver.
        /// </summary>
        public void SitAsDriver()
        {
            TransitionTo(AnimationCommandType.VehicleDriver);
        }

        /// <summary>
        /// Sit as passenger.
        /// </summary>
        public void SitAsPassenger()
        {
            TransitionTo(AnimationCommandType.VehiclePassenger);
        }

        #endregion

        #region NPC Work Commands

        /// <summary>
        /// Operate vending machine.
        /// </summary>
        public void WorkVending()
        {
            TransitionTo(AnimationCommandType.WorkVending);
        }

        /// <summary>
        /// DJ performance.
        /// </summary>
        public void WorkDJ()
        {
            TransitionTo(AnimationCommandType.WorkDJ);
        }

        /// <summary>
        /// Serve customers.
        /// </summary>
        public void WorkServing()
        {
            TransitionTo(AnimationCommandType.WorkServing);
        }

        /// <summary>
        /// Security patrol.
        /// </summary>
        public void WorkSecurity()
        {
            TransitionTo(AnimationCommandType.WorkSecurity);
        }

        #endregion

        #region Private Methods

        private void TransitionTo(AnimationCommandType commandType)
        {
            TransitionTo(new AnimationCommand(commandType));
        }

        private void TransitionTo(AnimationCommand command)
        {
            _previousState = _currentState;
            _currentState = command.Type;
            _stateStartTime = Time.time;

            if (_animationController != null)
            {
                _animationController.QueueCommand(command);
            }

            OnStateChanged?.Invoke(_previousState, _currentState);
        }

        #endregion
    }
}
