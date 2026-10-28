using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Unified Gate C entry point and workflow controller.
    /// </summary>
    public static class GateC
    {
        /// <summary>
        /// Executes the full Gate C pipeline: asset setup, validator, and contract report.
        /// Intended for batch-mode / CI runners.
        /// </summary>
        public static void PerformAll()
        {
            SetupAllAssets();
            VerifyAllAssets();
            Debug.Log("[NAAD][GateC] Gate C pipeline complete.");
        }

        /// <summary>
        /// Menu entry: NAAD > Gate C > Setup All Assets
        /// </summary>
        [MenuItem("NAAD/Gate C/Setup All Assets", priority = 500)]
        public static void SetupAllAssets()
        {
            SetupGateCAssets.SetupAll();
        }

        /// <summary>
        /// Menu entry: NAAD > Verify Animation Assets
        /// </summary>
        [MenuItem("NAAD/Verify Animation Assets", priority = 501)]
        public static void VerifyAllAssets()
        {
            VerifyAnimationAssets.RunVerification();
        }
    }
}
