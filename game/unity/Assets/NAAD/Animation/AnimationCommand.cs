using UnityEngine;

namespace NAAD.Animation
{
    /// <summary>
    /// Authoritative animation commands.
    /// These represent the game logic's intent to play an animation.
    /// Animation events MUST NOT trigger these - they are for presentation only.
    /// </summary>
    public enum AnimationCommandType
    {
        // Locomotion
        LocomotionIdle,
        LocomotionWalk,
        LocomotionRun,
        LocomotionStop,
        LocomotionTurnLeft,
        LocomotionTurnRight,

        // Social
        SocialGreeting,
        SocialConversation,
        SocialListening,
        SocialLaugh,
        SocialCelebrate,

        // Phone
        PhoneRetrieve,
        PhoneHold,
        PhoneBrowse,
        PhonePutAway,

        // Food
        FoodOrder,
        FoodServe,
        FoodEat,
        FoodDrink,

        // Nightlife
        NightlifeDance,
        NightlifeCheer,
        NightlifePerform,
        NightlifeSocialise,

        // Seating
        SeatingSit,
        SeatingIdleSeated,
        SeatingStand,

        // Vehicle
        VehicleEnter,
        VehicleExit,
        VehicleDriver,
        VehiclePassenger,

        // NPC Work
        WorkVending,
        WorkDJ,
        WorkServing,
        WorkSecurity
    }

    /// <summary>
    /// Authoritative animation command from game logic.
    /// This is the ONLY way game systems should request animations.
    /// Animation events cannot create these - they are presentation-only.
    /// </summary>
    public struct AnimationCommand
    {
        public AnimationCommandType Type;
        public float Duration; // 0 = use animation clip duration
        public bool Loop; // Whether to loop the animation
        public float Speed; // Animation playback speed multiplier
        public object Context; // Optional context data (e.g., target position, item reference)

        public AnimationCommand(AnimationCommandType type, float duration = 0f, bool loop = false, float speed = 1f, object context = null)
        {
            Type = type;
            Duration = duration;
            Loop = loop;
            Speed = speed;
            Context = context;
        }

        public static AnimationCommand Idle() => new AnimationCommand(AnimationCommandType.LocomotionIdle, loop: true);
        public static AnimationCommand Walk(float speed = 1f) => new AnimationCommand(AnimationCommandType.LocomotionWalk, loop: true, speed: speed);
        public static AnimationCommand Run(float speed = 1f) => new AnimationCommand(AnimationCommandType.LocomotionRun, loop: true, speed: speed);
        public static AnimationCommand Stop() => new AnimationCommand(AnimationCommandType.LocomotionStop);
        public static AnimationCommand Greeting() => new AnimationCommand(AnimationCommandType.SocialGreeting);
        public static AnimationCommand PhoneRetrieve() => new AnimationCommand(AnimationCommandType.PhoneRetrieve);
        public static AnimationCommand PhoneHold(bool loop = true) => new AnimationCommand(AnimationCommandType.PhoneHold, loop: loop);
        public static AnimationCommand PhoneBrowse() => new AnimationCommand(AnimationCommandType.PhoneBrowse, loop: true);
        public static AnimationCommand PhonePutAway() => new AnimationCommand(AnimationCommandType.PhonePutAway);
    }
}
