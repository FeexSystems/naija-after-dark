using System;
using System.Collections.Generic;
using UnityEngine;

namespace NAAD.Animation
{
    /// <summary>
    /// Bridges authoritative animation commands with Unity's Animator.
    /// This component receives AnimationCommands from game logic and translates them
    /// to Animator parameters and state changes.
    /// 
    /// IMPORTANT: Animation events in the Animator must NOT call methods on this
    /// component that would trigger game logic changes. Animation events are for
    /// presentation only (sound effects, particle triggers, etc.).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimationController : MonoBehaviour
    {
        private Animator _animator;
        private Queue<AnimationCommand> _commandQueue = new Queue<AnimationCommand>();
        private AnimationCommandType _currentCommandType = AnimationCommandType.LocomotionIdle;
        private bool _isTransitioning = false;

        [Header("Parameters")]
        [SerializeField] private string _locomotionSpeedParam = "LocomotionSpeed";
        [SerializeField] private string _locomotionStateParam = "LocomotionState";
        [SerializeField] private string _socialStateParam = "SocialState";
        [SerializeField] private string _phoneStateParam = "PhoneState";
        [SerializeField] private string _seatingStateParam = "SeatingState";
        [SerializeField] private string _triggerParam = "Trigger";

        [Header("State Values")]
        [SerializeField] private int _idleState = 0;
        [SerializeField] private int _walkState = 1;
        [SerializeField] private int _runState = 2;
        [SerializeField] private int _socialNone = 0;
        [SerializeField] private int _socialGreeting = 1;
        [SerializeField] private int _phoneNone = 0;
        [SerializeField] private int _phoneRetrieve = 1;
        [SerializeField] private int _phoneHold = 2;
        [SerializeField] private int _phoneBrowse = 3;
        [SerializeField] private int _phonePutAway = 4;
        [SerializeField] private int _seatingNone = 0;
        [SerializeField] private int _seatingSit = 1;
        [SerializeField] private int _seatingIdle = 2;
        [SerializeField] private int _seatingStand = 3;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("[CharacterAnimationController] No Animator component found");
            }
        }

        private void Update()
        {
            ProcessCommandQueue();
        }

        /// <summary>
        /// Queue an authoritative animation command from game logic.
        /// This is the ONLY way game systems should request animations.
        /// </summary>
        public void QueueCommand(AnimationCommand command)
        {
            _commandQueue.Enqueue(command);
        }

        /// <summary>
        /// Immediately execute an animation command, bypassing the queue.
        /// Use sparingly - typically for high-priority state changes.
        /// </summary>
        public void ExecuteCommandImmediate(AnimationCommand command)
        {
            ExecuteCommand(command);
        }

        private void ProcessCommandQueue()
        {
            if (_isTransitioning || _commandQueue.Count == 0)
                return;

            var command = _commandQueue.Dequeue();
            ExecuteCommand(command);
        }

        private void ExecuteCommand(AnimationCommand command)
        {
            _currentCommandType = command.Type;
            _animator.SetFloat(_locomotionSpeedParam, command.Speed);

            switch (command.Type)
            {
                // Locomotion
                case AnimationCommandType.LocomotionIdle:
                    SetLocomotionState(_idleState);
                    ResetSocialState();
                    ResetPhoneState();
                    ResetSeatingState();
                    break;

                case AnimationCommandType.LocomotionWalk:
                    SetLocomotionState(_walkState);
                    break;

                case AnimationCommandType.LocomotionRun:
                    SetLocomotionState(_runState);
                    break;

                case AnimationCommandType.LocomotionStop:
                    SetLocomotionState(_idleState);
                    break;

                case AnimationCommandType.LocomotionTurnLeft:
                case AnimationCommandType.LocomotionTurnRight:
                    // Turn animations are typically one-shot, then return to current locomotion
                    TriggerAnimation();
                    break;

                // Social
                case AnimationCommandType.SocialGreeting:
                    SetSocialState(_socialGreeting);
                    break;

                case AnimationCommandType.SocialConversation:
                case AnimationCommandType.SocialListening:
                case AnimationCommandType.SocialLaugh:
                case AnimationCommandType.SocialCelebrate:
                    // TODO: Implement additional social states
                    SetSocialState(_socialNone);
                    break;

                // Phone
                case AnimationCommandType.PhoneRetrieve:
                    SetPhoneState(_phoneRetrieve);
                    break;

                case AnimationCommandType.PhoneHold:
                    SetPhoneState(_phoneHold);
                    break;

                case AnimationCommandType.PhoneBrowse:
                    SetPhoneState(_phoneBrowse);
                    break;

                case AnimationCommandType.PhonePutAway:
                    SetPhoneState(_phonePutAway);
                    break;

                // Food
                case AnimationCommandType.FoodOrder:
                case AnimationCommandType.FoodServe:
                case AnimationCommandType.FoodEat:
                case AnimationCommandType.FoodDrink:
                    // TODO: Implement food states
                    break;

                // Nightlife
                case AnimationCommandType.NightlifeDance:
                case AnimationCommandType.NightlifeCheer:
                case AnimationCommandType.NightlifePerform:
                case AnimationCommandType.NightlifeSocialise:
                    // TODO: Implement nightlife states
                    break;

                // Seating
                case AnimationCommandType.SeatingSit:
                    SetSeatingState(_seatingSit);
                    break;

                case AnimationCommandType.SeatingIdleSeated:
                    SetSeatingState(_seatingIdle);
                    break;

                case AnimationCommandType.SeatingStand:
                    SetSeatingState(_seatingStand);
                    break;

                // Vehicle
                case AnimationCommandType.VehicleEnter:
                case AnimationCommandType.VehicleExit:
                case AnimationCommandType.VehicleDriver:
                case AnimationCommandType.VehiclePassenger:
                    // TODO: Implement vehicle states
                    break;

                // NPC Work
                case AnimationCommandType.WorkVending:
                case AnimationCommandType.WorkDJ:
                case AnimationCommandType.WorkServing:
                case AnimationCommandType.WorkSecurity:
                    // TODO: Implement NPC work states
                    break;
            }
        }

        private void SetLocomotionState(int state)
        {
            _animator.SetInteger(_locomotionStateParam, state);
        }

        private void SetSocialState(int state)
        {
            _animator.SetInteger(_socialStateParam, state);
        }

        private void ResetSocialState()
        {
            _animator.SetInteger(_socialStateParam, _socialNone);
        }

        private void SetPhoneState(int state)
        {
            _animator.SetInteger(_phoneStateParam, state);
        }

        private void ResetPhoneState()
        {
            _animator.SetInteger(_phoneStateParam, _phoneNone);
        }

        private void SetSeatingState(int state)
        {
            _animator.SetInteger(_seatingStateParam, state);
        }

        private void ResetSeatingState()
        {
            _animator.SetInteger(_seatingStateParam, _seatingNone);
        }

        private void TriggerAnimation()
        {
            _animator.SetTrigger(_triggerParam);
        }

        /// <summary>
        /// Called by Animator when an animation completes.
        /// This is a PRESENTATION callback only - no game logic here.
        /// </summary>
        public void OnAnimationComplete()
        {
            _isTransitioning = false;
            
            // Return to appropriate base state based on current command type
            switch (_currentCommandType)
            {
                case AnimationCommandType.SocialGreeting:
                    ResetSocialState();
                    break;
                case AnimationCommandType.PhoneRetrieve:
                    SetPhoneState(_phoneHold);
                    break;
                case AnimationCommandType.PhonePutAway:
                    ResetPhoneState();
                    break;
                case AnimationCommandType.SeatingSit:
                    SetSeatingState(_seatingIdle);
                    break;
                case AnimationCommandType.SeatingStand:
                    ResetSeatingState();
                    break;
            }
        }

        /// <summary>
        /// Called by Animator when an animation starts.
        /// This is a PRESENTATION callback only - no game logic here.
        /// </summary>
        public void OnAnimationStart()
        {
            _isTransitioning = true;
        }

        /// <summary>
        /// Get the current animation command type.
        /// Useful for debugging and state synchronization.
        /// </summary>
        public AnimationCommandType GetCurrentCommandType()
        {
            return _currentCommandType;
        }

        /// <summary>
        /// Check if currently transitioning between animations.
        /// </summary>
        public bool IsTransitioning()
        {
            return _isTransitioning;
        }
    }
}
