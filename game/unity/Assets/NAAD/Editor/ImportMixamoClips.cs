using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Gate C — convert the Mixamo FBX takes in
    /// <c>Assets/NAAD/Animation/Animations/NAADASSETSNEW</c> into the eight clips the
    /// Animator contract requires.
    ///
    /// Why a tool and not a one-off batch: the sources are animation-only FBX
    /// ("Without Skin" — no geometry, no skin weights) built on the <c>mixamorig</c>
    /// skeleton, which is the same skeleton PlayerRig.fbx uses. Importing them as
    /// Humanoid with "Copy From Other Avatar" pointed at PlayerRig's avatar makes the
    /// muscle curves retarget for free.
    ///
    /// The target .anim files are OVERWRITTEN IN PLACE. Their .meta files are never
    /// touched, so the GUIDs stay stable and PlayerController.controller keeps its
    /// bindings — deleting and re-creating the assets would silently break every
    /// state transition.
    ///
    /// Run from the menu: NAAD > Gate C > Import Mixamo Clips
    /// Or headless: -executeMethod NAAD.Editor.ImportMixamoClips.Run
    /// </summary>
    public static class ImportMixamoClips
    {
        private const string SourceFolder = "Assets/NAAD/Animation/Animations/NAADASSETSNEW";
        private const string RigPath = "Assets/NAAD/Animation/Characters/Player/PlayerRig.fbx";

        private static readonly string Root = "Assets/NAAD/Animation/Animations";

        /// <summary>target .anim -> source FBX (file name without extension).</summary>
        private static readonly (string target, string source, bool derived)[] Map =
        {
            (Root + "/Locomotion/Idle.anim",  "Neutral Idle",                        false),
            (Root + "/Locomotion/Walk.anim",  "Feminine Walk Forward",               false),
            (Root + "/Locomotion/Run.anim",   "Running Forward Quickly",             false),
            (Root + "/Phone/Retrieve.anim",   "Searching Front And Back Pockets",    false),
            (Root + "/Phone/Browse.anim",     "Female Using Touchscreen Tablet",     false),
            (Root + "/Social/Greeting.anim",  "Quick Informal Bow",                  false),

            // The pack has no phone Hold or PutAway take. Derive them from real motion
            // rather than leaving the states empty; revisit when a dedicated take lands.
            (Root + "/Phone/Hold.anim",       "Female Using Touchscreen Tablet",     true),
            (Root + "/Phone/PutAway.anim",    "Neutral Idle",                        true),
        };

        [MenuItem("NAAD/Gate C/Import Mixamo Clips")]
        public static void RunMenu() => Run();

        public static void Run()
        {
            var avatar = LoadSourceAvatar();
            if (avatar == null)
            {
                Debug.LogWarning("[NAAD][Mixamo] No humanoid Avatar on " + RigPath +
                                 " — importing anyway; each take gets its own avatar.");
            }

            var results = new List<string>();
            var failed = 0;

            foreach (var entry in Map)
            {
                var sourcePath = SourceFolder + "/" + entry.source + ".fbx";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath) == null)
                {
                    results.Add("MISSING SOURCE: " + sourcePath);
                    failed++;
                    continue;
                }

                ConfigureHumanoid(sourcePath, avatar);

                var target = AssetDatabase.LoadAssetAtPath<AnimationClip>(entry.target);
                if (target == null)
                {
                    results.Add("MISSING TARGET (create it first): " + entry.target);
                    failed++;
                    continue;
                }

                var source = LoadFirstClip(sourcePath);
                if (source == null)
                {
                    results.Add("NO CLIP IN: " + sourcePath);
                    failed++;
                    continue;
                }

                // Copy, never delete-and-recreate: the target keeps its .meta and GUID.
                EditorUtility.CopySerialized(source, target);
                EditorUtility.SetDirty(target);

                results.Add(string.Format(
                    "{0}  <-  {1}{2}  ({3:F2}s)",
                    entry.target.Substring(Root.Length + 1),
                    entry.source,
                    entry.derived ? "  [derived]" : "",
                    target.length));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[NAAD][Mixamo] " + (Map.Length - failed) + "/" + Map.Length +
                      " clips written.\n  " + string.Join("\n  ", results.ToArray()));
        }

        private static Avatar LoadSourceAvatar()
        {
            return AssetDatabase.LoadAllAssetsAtPath(RigPath)
                .OfType<Avatar>()
                .FirstOrDefault(a => a != null && a.isHuman);
        }

        private static void ConfigureHumanoid(string fbxPath, Avatar sourceAvatar)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer == null)
                return;

            importer.animationType = ModelImporterAnimationType.Human;
            importer.importAnimation = true;

            // Do NOT copy PlayerRig's avatar. These takes name bones "mixamorig:Hips"
            // (colon) while PlayerRig.fbx uses "mixamorigHips" — copying produces
            // "Copied Avatar Rig Configuration mis-match. Transform 'mixamorigHips' for
            // human bone 'Hips' not found" and the clips never import. Each file gets
            // its own humanoid avatar instead; humanoid clips are stored in muscle
            // space, so they still retarget onto PlayerRig at runtime.
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            // Always reimport. Previously this was gated on a `changed` flag, which was
            // a bug: the importer fields can already read as correct while the asset on
            // disk was last built with the old settings, leaving the model imported as
            // a bare hierarchy with no Avatar and no AnimationClip.
            importer.SaveAndReimport();

            // SaveAndReimport alone leaves the model as a skeleton-only hierarchy with
            // no Avatar and no AnimationClip (verified: Rig tab shows "no avatar").
            // A forced ImportAsset is what actually makes Unity build the humanoid
            // Avatar and extract the takes.
            AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);
        }

        private static AnimationClip LoadFirstClip(string fbxPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .Where(c => c != null && !c.name.StartsWith("__preview__", StringComparison.Ordinal))
                .OrderByDescending(c => c.length)
                .FirstOrDefault();
        }
    }
}
