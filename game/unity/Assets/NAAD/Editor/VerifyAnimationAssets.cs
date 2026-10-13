using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Verification script to check for missing animation assets.
    /// Run this from the Unity Editor menu: NAAD > Verify Animation Assets
    /// </summary>
    public class VerifyAnimationAssets
    {
        private class AnimationRequirement
        {
            public string Category;
            public string Name;
            public string ExpectedPath;
            public bool IsRequired;
            public bool IsFound;
        }

        private static List<AnimationRequirement> _requirements;

        [MenuItem("NAAD/Verify Animation Assets")]
        public static void RunVerification()
        {
            Debug.Log("[NAAD] Starting animation asset verification...");
            
            InitializeRequirements();
            CheckAssets();
            ReportResults();
        }

        private static void InitializeRequirements()
        {
            _requirements = new List<AnimationRequirement>();

            // Locomotion
            AddRequirement("Locomotion", "Idle", "Assets/NAAD/Animation/Animations/Locomotion/Idle.anim", true);
            AddRequirement("Locomotion", "Walk", "Assets/NAAD/Animation/Animations/Locomotion/Walk.anim", true);
            AddRequirement("Locomotion", "Run", "Assets/NAAD/Animation/Animations/Locomotion/Run.anim", true);
            AddRequirement("Locomotion", "Start", "Assets/NAAD/Animation/Animations/Locomotion/Start.anim", false);
            AddRequirement("Locomotion", "Stop", "Assets/NAAD/Animation/Animations/Locomotion/Stop.anim", false);
            AddRequirement("Locomotion", "TurnLeft", "Assets/NAAD/Animation/Animations/Locomotion/TurnLeft.anim", false);
            AddRequirement("Locomotion", "TurnRight", "Assets/NAAD/Animation/Animations/Locomotion/TurnRight.anim", false);

            // Social
            AddRequirement("Social", "Greeting", "Assets/NAAD/Animation/Animations/Social/Greeting.anim", true);
            AddRequirement("Social", "Conversation", "Assets/NAAD/Animation/Animations/Social/Conversation.anim", false);
            AddRequirement("Social", "Listening", "Assets/NAAD/Animation/Animations/Social/Listening.anim", false);
            AddRequirement("Social", "Laugh", "Assets/NAAD/Animation/Animations/Social/Laugh.anim", false);
            AddRequirement("Social", "Celebrate", "Assets/NAAD/Animation/Animations/Social/Celebrate.anim", false);

            // Phone
            AddRequirement("Phone", "Retrieve", "Assets/NAAD/Animation/Animations/Phone/Retrieve.anim", true);
            AddRequirement("Phone", "Hold", "Assets/NAAD/Animation/Animations/Phone/Hold.anim", true);
            AddRequirement("Phone", "Browse", "Assets/NAAD/Animation/Animations/Phone/Browse.anim", true);
            AddRequirement("Phone", "PutAway", "Assets/NAAD/Animation/Animations/Phone/PutAway.anim", true);

            // Food
            AddRequirement("Food", "Order", "Assets/NAAD/Animation/Animations/Food/Order.anim", false);
            AddRequirement("Food", "Serve", "Assets/NAAD/Animation/Animations/Food/Serve.anim", false);
            AddRequirement("Food", "Eat", "Assets/NAAD/Animation/Animations/Food/Eat.anim", false);
            AddRequirement("Food", "Drink", "Assets/NAAD/Animation/Animations/Food/Drink.anim", false);

            // Nightlife
            AddRequirement("Nightlife", "Dance", "Assets/NAAD/Animation/Animations/Nightlife/Dance.anim", false);
            AddRequirement("Nightlife", "Cheer", "Assets/NAAD/Animation/Animations/Nightlife/Cheer.anim", false);
            AddRequirement("Nightlife", "Perform", "Assets/NAAD/Animation/Animations/Nightlife/Perform.anim", false);
            AddRequirement("Nightlife", "Socialise", "Assets/NAAD/Animation/Animations/Nightlife/Socialise.anim", false);

            // Seating
            AddRequirement("Seating", "Sit", "Assets/NAAD/Animation/Animations/Seating/Sit.anim", false);
            AddRequirement("Seating", "IdleSeated", "Assets/NAAD/Animation/Animations/Seating/IdleSeated.anim", false);
            AddRequirement("Seating", "Stand", "Assets/NAAD/Animation/Animations/Seating/Stand.anim", false);

            // Vehicle
            AddRequirement("Vehicle", "Enter", "Assets/NAAD/Animation/Animations/Vehicle/Enter.anim", false);
            AddRequirement("Vehicle", "Exit", "Assets/NAAD/Animation/Animations/Vehicle/Exit.anim", false);
            AddRequirement("Vehicle", "Driver", "Assets/NAAD/Animation/Animations/Vehicle/Driver.anim", false);
            AddRequirement("Vehicle", "Passenger", "Assets/NAAD/Animation/Animations/Vehicle/Passenger.anim", false);

            // NPC Work
            AddRequirement("NPCWork", "Vending", "Assets/NAAD/Animation/Animations/NPCWork/Vending.anim", false);
            AddRequirement("NPCWork", "DJ", "Assets/NAAD/Animation/Animations/NPCWork/DJ.anim", false);
            AddRequirement("NPCWork", "Serving", "Assets/NAAD/Animation/Animations/NPCWork/Serving.anim", false);
            AddRequirement("NPCWork", "Security", "Assets/NAAD/Animation/Animations/NPCWork/Security.anim", false);

            // Character Rigs
            AddRequirement("Characters", "PlayerRig", "Assets/NAAD/Animation/Characters/Player/PlayerRig.fbx", true);
            AddRequirement("Characters", "PlayerPrefab", "Assets/NAAD/Animation/Characters/Player/PlayerRig.prefab", true);

            // Controllers
            AddRequirement("Controllers", "PlayerController", "Assets/NAAD/Animation/Controllers/PlayerController.controller", true);

            // Avatars
            AddRequirement("Avatars", "PlayerAvatar", "Assets/NAAD/Animation/Avatars/PlayerAvatar.asset", true);
            AddRequirement("Avatars", "UpperBodyMask", "Assets/NAAD/Animation/Avatars/UpperBodyMask.mask", true);
        }

        private static void AddRequirement(string category, string name, string expectedPath, bool isRequired)
        {
            _requirements.Add(new AnimationRequirement
            {
                Category = category,
                Name = name,
                ExpectedPath = expectedPath,
                IsRequired = isRequired,
                IsFound = false
            });
        }

        private static void CheckAssets()
        {
            foreach (var req in _requirements)
            {
                req.IsFound = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(req.ExpectedPath) != null;
            }
        }

        private static void ReportResults()
        {
            var missingRequired = _requirements.Where(r => r.IsRequired && !r.IsFound).ToList();
            var missingOptional = _requirements.Where(r => !r.IsRequired && !r.IsFound).ToList();
            var foundCount = _requirements.Count(r => r.IsFound);
            var totalCount = _requirements.Count;

            Debug.Log($"[NAAD] Animation Asset Verification Complete");
            Debug.Log($"[NAAD] Found: {foundCount}/{totalCount} assets");

            if (missingRequired.Count > 0)
            {
                Debug.LogError($"[NAAD] MISSING REQUIRED ASSETS ({missingRequired.Count}):");
                foreach (var req in missingRequired)
                {
                    Debug.LogError($"  - {req.Category}/{req.Name}: {req.ExpectedPath}");
                }
            }
            else
            {
                Debug.Log("[NAAD] ✓ All required animation assets are present");
            }

            if (missingOptional.Count > 0)
            {
                Debug.LogWarning($"[NAAD] Missing optional assets ({missingOptional.Count}):");
                foreach (var req in missingOptional)
                {
                    Debug.LogWarning($"  - {req.Category}/{req.Name}: {req.ExpectedPath}");
                }
            }

            // Generate report file
            GenerateReportFile(missingRequired, missingOptional, foundCount, totalCount);
        }

        private static void GenerateReportFile(List<AnimationRequirement> missingRequired, 
            List<AnimationRequirement> missingOptional, int foundCount, int totalCount)
        {
            var reportPath = "Assets/NAAD/Animation/VERIFICATION_REPORT.txt";
            var report = new System.Text.StringBuilder();

            report.AppendLine("NAAD Animation Asset Verification Report");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine($"Summary: {foundCount}/{totalCount} assets found");
            report.AppendLine($"Missing Required: {missingRequired.Count}");
            report.AppendLine($"Missing Optional: {missingOptional.Count}");
            report.AppendLine();

            if (missingRequired.Count > 0)
            {
                report.AppendLine("MISSING REQUIRED ASSETS:");
                foreach (var req in missingRequired)
                {
                    report.AppendLine($"  [X] {req.Category}/{req.Name}");
                    report.AppendLine($"      Path: {req.ExpectedPath}");
                    report.AppendLine($"      Action: Acquire from Mixamo, Asset Store, or create custom");
                    report.AppendLine();
                }
            }

            if (missingOptional.Count > 0)
            {
                report.AppendLine("MISSING OPTIONAL ASSETS:");
                foreach (var req in missingOptional)
                {
                    report.AppendLine($"  [ ] {req.Category}/{req.Name}");
                    report.AppendLine($"      Path: {req.ExpectedPath}");
                    report.AppendLine();
                }
            }

            if (missingRequired.Count == 0 && missingOptional.Count == 0)
            {
                report.AppendLine("✓ All animation assets are present and accounted for.");
            }

            System.IO.File.WriteAllText(reportPath, report.ToString());
            Debug.Log($"[NAAD] Verification report saved to: {reportPath}");
            AssetDatabase.Refresh();
        }
    }
}
