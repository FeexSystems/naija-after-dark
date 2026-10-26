using System;
using System.Globalization;
using UnityEngine;

namespace NAAD.Gameplay
{
    /// <summary>
    /// Mobile optimization configuration for Gate 16.
    /// Defines profiling targets and thresholds for low/mid/high-end Android and iPhone.
    /// </summary>
    [CreateAssetMenu(fileName = "MobileProfilingConfig", menuName = "NAAD/Content/MobileProfilingConfig", order = 100)]
    public sealed class MobileProfilingConfig : ScriptableObject
    {
        [Header("Target Metrics")]
        public MobileDevice TargetDevice = MobileDevice.MidAndroid;

        public struct FpsTargets
        {
            public int Low;
            public int Mid;
            public int High;
        }

        public struct RamTargets
        {
            public int Low;
            public int Mid;
            public int High;
        }

        public struct LoadTargets
        {
            public int ColdStartSec;
            public int SceneLoadSec;
        }

        public struct NetworkTargets
        {
            public int P95ms;
        }

        [SerializeField] private FpsTargets _fpsTargets = new FpsTargets { Low = 30, Mid = 45, High = 60 };
        [SerializeField] private RamTargets _ramTargets = new RamTargets { Low = 1200, Mid = 1800, High = 2400 };
        [SerializeField] private LoadTargets _loadTargets = new LoadTargets { ColdStartSec = 8, SceneLoadSec = 3 };
        [SerializeField] private NetworkTargets _networkTargets = new NetworkTargets { P95ms = 500 };

        public FpsTargets Fps => _fpsTargets;
        public RamTargets Ram => _ramTargets;
        public LoadTargets Load => _loadTargets;
        public NetworkTargets Network => _networkTargets;

        public static MobileProfilingConfig Default => ScriptableObject.CreateInstance<MobileProfilingConfig>();
    }

    public enum MobileDevice
    {
        LowAndroid,
        MidAndroid,
        HighAndroid,
        iPhone,
        Generic
    }
}
