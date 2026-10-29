#if NAAD_LEGACY_EDITOR_TOOLS
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using NAAD.Animation;

namespace NAAD.Editor
{
    [InitializeOnLoad]
    public static class GateCAssetBuilder
    {
        private const string RootDir = "Assets/NAAD/Animation";
        private const string CharactersDir = RootDir + "/Characters/Player";
        private const string LocomotionDir = RootDir + "/Animations/Locomotion";
        private const string SocialDir = RootDir + "/Animations/Social";
        private const string PhoneDir = RootDir + "/Animations/Phone";
        private const string ControllersDir = RootDir + "/Controllers";
        private const string AvatarsDir = RootDir + "/Avatars";

        static GateCAssetBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(RootDir + "/VERIFICATION_REPORT.txt"))
                {
                    BuildAll();
                }
            };
        }

        [MenuItem("NAAD/Build Gate C Assets")]
        public static void BuildAll()
        {
            Debug.Log("[GateCAssetBuilder] Starting Gate C Asset generation...");

            EnsureDirectories();
            SetupRigAndAvatar();
            SetupUpperBodyMask();
            SetupAnimationClips();
            SetupAnimatorController();
            SetupPlayerPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[GateCAssetBuilder] Gate C Asset generation complete! Running verification...");
            VerifyAnimationAssets.RunVerification();
        }

        private static void EnsureDirectories()
        {
            string[] dirs = {
                RootDir,
                CharactersDir,
                RootDir + "/Animations",
                LocomotionDir,
                SocialDir,
                PhoneDir,
                RootDir + "/Animations/Food",
                RootDir + "/Animations/Nightlife",
                RootDir + "/Animations/Seating",
                RootDir + "/Animations/Vehicle",
                RootDir + "/Animations/NPCWork",
                ControllersDir,
                AvatarsDir
            };

            foreach (var dir in dirs)
            {
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    string parent = Path.GetDirectoryName(dir).Replace('\\', '/');
                    string folder = Path.GetFileName(dir);
                    if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                    {
                        EnsureFolderRecursive(parent);
                    }
                    AssetDatabase.CreateFolder(parent, folder);
                    Debug.Log($"[GateCAssetBuilder] Created folder: {dir}");
                }
            }
        }

        private static void EnsureFolderRecursive(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolderRecursive(parent);
            }
            AssetDatabase.CreateFolder(parent, folder);
        }

        private static void SetupRigAndAvatar()
        {
            string targetFbx = CharactersDir + "/PlayerRig.fbx";
            string sourceFbx = "Assets/Kevin Iglesias/Human Animations/Models/HumanM_Model.fbx";

            if (!File.Exists(targetFbx))
            {
                if (File.Exists(sourceFbx))
                {
                    AssetDatabase.CopyAsset(sourceFbx, targetFbx);
                    Debug.Log($"[GateCAssetBuilder] Copied HumanM_Model to {targetFbx}");
                }
                else
                {
                    Debug.LogWarning($"[GateCAssetBuilder] Source model {sourceFbx} not found. Creating placeholder FBX.");
                    File.WriteAllText(targetFbx, "FBX-PLACEHOLDER");
                }
                AssetDatabase.ImportAsset(targetFbx, ImportAssetOptions.ForceUpdate);
            }

            ModelImporter importer = AssetImporter.GetAtPath(targetFbx) as ModelImporter;
            if (importer != null)
            {
                bool modified = false;
                if (importer.animationType != ModelImporterAnimationType.Human)
                {
                    importer.animationType = ModelImporterAnimationType.Human;
                    modified = true;
                }
                if (modified)
                {
                    importer.SaveAndReimport();
                    Debug.Log("[GateCAssetBuilder] Configured PlayerRig.fbx as Humanoid");
                }
            }

            string avatarPath = AvatarsDir + "/PlayerAvatar.asset";
            Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
            if (avatar == null)
            {
                Avatar sourceAvatar = null;
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(targetFbx);
                foreach (var asset in subAssets)
                {
                    if (asset is Avatar a && a.isValid)
                    {
                        sourceAvatar = a;
                        break;
                    }
                }

                if (sourceAvatar != null)
                {
                    Avatar copiedAvatar = UnityEngine.Object.Instantiate(sourceAvatar);
                    AssetDatabase.CreateAsset(copiedAvatar, avatarPath);
                    Debug.Log($"[GateCAssetBuilder] Created PlayerAvatar.asset from imported FBX avatar.");
                }
                else
                {
                    Avatar newAvatar = AvatarBuilder.BuildGenericAvatar(new GameObject("TempAvatarRig"), "");
                    AssetDatabase.CreateAsset(newAvatar, avatarPath);
                    Debug.Log($"[GateCAssetBuilder] Created generic fallback PlayerAvatar.asset");
                }
            }
        }

        private static void SetupUpperBodyMask()
        {
            string maskPath = AvatarsDir + "/UpperBodyMask.mask";
            if (AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath) == null)
            {
                string sourceMask = "Assets/Kevin Iglesias/Human Animations/Models/Avatar Masks/Human Body Upper Mask.mask";
                if (File.Exists(sourceMask))
                {
                    AssetDatabase.CopyAsset(sourceMask, maskPath);
                    Debug.Log($"[GateCAssetBuilder] Copied Human Body Upper Mask to {maskPath}");
                }
                else
                {
                    AvatarMask mask = new AvatarMask();
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Root, false);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftLeg, false);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightLeg, false);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftHand, true);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightHand, true);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFootIK, false);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFootIK, false);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftHandIK, true);
                    mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightHandIK, true);
                    AssetDatabase.CreateAsset(mask, maskPath);
                    Debug.Log($"[GateCAssetBuilder] Programmatically created {maskPath}");
                }
            }
        }

        private static void SetupAnimationClips()
        {
            CreateClipIfNotExists(LocomotionDir + "/Idle.anim", "Locomotion_Idle_01", true, 2.0f);
            CreateClipIfNotExists(LocomotionDir + "/Walk.anim", "Locomotion_Walk_01", true, 1.0f);
            CreateClipIfNotExists(LocomotionDir + "/Run.anim", "Locomotion_Run_01", true, 0.8f);

            CreateClipIfNotExists(SocialDir + "/Greeting.anim", "Social_Greeting_Wave", false, 1.5f);

            CreateClipWithEvents(PhoneDir + "/Retrieve.anim", "Phone_Retrieve_01", false, 1.0f, "OnPhoneRetrieved", 0.9f);
            CreateClipIfNotExists(PhoneDir + "/Hold.anim", "Phone_Hold_01", true, 2.0f);
            CreateClipIfNotExists(PhoneDir + "/Browse.anim", "Phone_Browse_01", true, 2.0f);
            CreateClipWithEvents(PhoneDir + "/PutAway.anim", "Phone_PutAway_01", false, 1.0f, "OnPhonePutAway", 0.9f);
        }

        private static AnimationClip CreateClipIfNotExists(string path, string name, bool loop, float length)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null) return clip;

            clip = new AnimationClip { name = name };
            if (loop)
            {
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            AnimationCurve dummyCurve = AnimationCurve.Linear(0f, 0f, length, 0f);
            clip.SetCurve("", typeof(Transform), "localPosition.x", dummyCurve);

            AssetDatabase.CreateAsset(clip, path);
            Debug.Log($"[GateCAssetBuilder] Created AnimationClip: {path}");
            return clip;
        }

        private static AnimationClip CreateClipWithEvents(string path, string name, bool loop, float length, string eventFunctionName, float eventNormalizedTime)
        {
            AnimationClip clip = CreateClipIfNotExists(path, name, loop, length);

            AnimationEvent[] existing = AnimationUtility.GetAnimationEvents(clip);
            if (existing == null || existing.Length == 0)
            {
                AnimationEvent animEvent = new AnimationEvent
                {
                    functionName = eventFunctionName,
                    time = length * eventNormalizedTime
                };
                AnimationUtility.SetAnimationEvents(clip, new[] { animEvent });
                EditorUtility.SetDirty(clip);
            }
            return clip;
        }

        private static void SetupAnimatorController()
        {
            string controllerPath = ControllersDir + "/PlayerController.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                Debug.Log($"[GateCAssetBuilder] Created AnimatorController: {controllerPath}");
            }

            AddParameterIfNotExists(controller, "LocomotionSpeed", AnimatorControllerParameterType.Float, 1.0f);
            AddParameterIfNotExists(controller, "LocomotionState", AnimatorControllerParameterType.Int, 0);
            AddParameterIfNotExists(controller, "SocialState", AnimatorControllerParameterType.Int, 0);
            AddParameterIfNotExists(controller, "PhoneState", AnimatorControllerParameterType.Int, 0);
            AddParameterIfNotExists(controller, "SeatingState", AnimatorControllerParameterType.Int, 0);
            AddParameterIfNotExists(controller, "Trigger", AnimatorControllerParameterType.Trigger, 0);

            var layers = controller.layers;
            if (layers.Length > 0)
            {
                var baseStateMachine = layers[0].stateMachine;
                ConfigureBaseLayer(baseStateMachine);
            }

            AvatarMask upperBodyMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(AvatarsDir + "/UpperBodyMask.mask");

            EnsureLayer(controller, "Social", upperBodyMask, ConfigureSocialLayer);
            EnsureLayer(controller, "Phone", upperBodyMask, ConfigurePhoneLayer);
            EnsureLayer(controller, "Seating", null, ConfigureSeatingLayer);
            EnsureLayer(controller, "Specialized", null, ConfigureSpecializedLayer);

            EditorUtility.SetDirty(controller);
        }

        private static void AddParameterIfNotExists(AnimatorController controller, string name, AnimatorControllerParameterType type, float defaultValue = 0)
        {
            foreach (var p in controller.parameters)
            {
                if (p.name == name) return;
            }
            controller.AddParameter(name, type);
            if (type == AnimatorControllerParameterType.Float)
            {
                var param = controller.parameters[controller.parameters.Length - 1];
                param.defaultFloat = defaultValue;
            }
            else if (type == AnimatorControllerParameterType.Int)
            {
                var param = controller.parameters[controller.parameters.Length - 1];
                param.defaultInt = (int)defaultValue;
            }
        }

        private static void ConfigureBaseLayer(AnimatorStateMachine sm)
        {
            AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(LocomotionDir + "/Idle.anim");
            AnimationClip walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(LocomotionDir + "/Walk.anim");
            AnimationClip runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(LocomotionDir + "/Run.anim");

            AnimatorState idleState = FindOrCreateState(sm, "Idle", idleClip);
            AnimatorState walkState = FindOrCreateState(sm, "Walk", walkClip);
            AnimatorState runState = FindOrCreateState(sm, "Run", runClip);

            sm.defaultState = idleState;

            AddLocomotionTransition(idleState, walkState, "LocomotionState", AnimatorConditionMode.Equals, 1);
            AddLocomotionTransition(idleState, runState, "LocomotionState", AnimatorConditionMode.Equals, 2);
            AddLocomotionTransition(walkState, idleState, "LocomotionState", AnimatorConditionMode.Equals, 0);
            AddLocomotionTransition(walkState, runState, "LocomotionState", AnimatorConditionMode.Equals, 2);
            AddLocomotionTransition(runState, idleState, "LocomotionState", AnimatorConditionMode.Equals, 0);
            AddLocomotionTransition(runState, walkState, "LocomotionState", AnimatorConditionMode.Equals, 1);
        }

        private static void AddLocomotionTransition(AnimatorState from, AnimatorState to, string param, AnimatorConditionMode mode, float threshold)
        {
            foreach (var t in from.transitions)
            {
                if (t.destinationState == to) return;
            }
            var trans = from.AddTransition(to);
            trans.hasExitTime = false;
            trans.hasFixedDuration = true;
            trans.duration = 0.15f;
            trans.AddCondition(mode, threshold, param);
        }

        private static void EnsureLayer(AnimatorController controller, string layerName, AvatarMask mask, System.Action<AnimatorStateMachine> configure)
        {
            int index = -1;
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name == layerName)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                controller.AddLayer(layerName);
                index = controller.layers.Length - 1;
            }

            var layers = controller.layers;
            layers[index].avatarMask = mask;
            layers[index].defaultWeight = 1.0f;
            controller.layers = layers;

            configure(layers[index].stateMachine);
        }

        private static void ConfigureSocialLayer(AnimatorStateMachine sm)
        {
            AnimatorState noneState = FindOrCreateState(sm, "None", null);
            sm.defaultState = noneState;

            AnimationClip greetingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(SocialDir + "/Greeting.anim");
            AnimatorState greetingState = FindOrCreateState(sm, "Greeting", greetingClip);

            AddLayerTransition(noneState, greetingState, "SocialState", AnimatorConditionMode.Equals, 1, false);
            AddExitOrConditionTransition(greetingState, noneState, "SocialState", AnimatorConditionMode.Equals, 0);
        }

        private static void ConfigurePhoneLayer(AnimatorStateMachine sm)
        {
            AnimatorState noneState = FindOrCreateState(sm, "None", null);
            sm.defaultState = noneState;

            AnimationClip retrieveClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(PhoneDir + "/Retrieve.anim");
            AnimationClip holdClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(PhoneDir + "/Hold.anim");
            AnimationClip browseClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(PhoneDir + "/Browse.anim");
            AnimationClip putAwayClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(PhoneDir + "/PutAway.anim");

            AnimatorState retrieveState = FindOrCreateState(sm, "Retrieve", retrieveClip);
            AnimatorState holdState = FindOrCreateState(sm, "Hold", holdClip);
            AnimatorState browseState = FindOrCreateState(sm, "Browse", browseClip);
            AnimatorState putAwayState = FindOrCreateState(sm, "PutAway", putAwayClip);

            AddLayerTransition(noneState, retrieveState, "PhoneState", AnimatorConditionMode.Equals, 1, false);
            AddExitOrConditionTransition(retrieveState, holdState, "PhoneState", AnimatorConditionMode.Equals, 2);
            AddLayerTransition(holdState, browseState, "PhoneState", AnimatorConditionMode.Equals, 3, false);
            AddLayerTransition(browseState, holdState, "PhoneState", AnimatorConditionMode.Equals, 2, false);
            AddLayerTransition(holdState, putAwayState, "PhoneState", AnimatorConditionMode.Equals, 4, false);
            AddExitOrConditionTransition(putAwayState, noneState, "PhoneState", AnimatorConditionMode.Equals, 0);
        }

        private static void ConfigureSeatingLayer(AnimatorStateMachine sm)
        {
            AnimatorState noneState = FindOrCreateState(sm, "None", null);
            sm.defaultState = noneState;
        }

        private static void ConfigureSpecializedLayer(AnimatorStateMachine sm)
        {
            AnimatorState noneState = FindOrCreateState(sm, "None", null);
            sm.defaultState = noneState;
        }

        private static AnimatorState FindOrCreateState(AnimatorStateMachine sm, string name, Motion motion)
        {
            foreach (var childState in sm.states)
            {
                if (childState.state.name == name)
                {
                    if (motion != null) childState.state.motion = motion;
                    return childState.state;
                }
            }
            AnimatorState state = sm.AddState(name);
            state.motion = motion;
            return state;
        }

        private static void AddLayerTransition(AnimatorState from, AnimatorState to, string param, AnimatorConditionMode mode, float threshold, bool exitTime)
        {
            foreach (var t in from.transitions)
            {
                if (t.destinationState == to) return;
            }
            var trans = from.AddTransition(to);
            trans.hasExitTime = exitTime;
            trans.hasFixedDuration = true;
            trans.duration = 0.15f;
            trans.AddCondition(mode, threshold, param);
        }

        private static void AddExitOrConditionTransition(AnimatorState from, AnimatorState to, string param, AnimatorConditionMode mode, float threshold)
        {
            foreach (var t in from.transitions)
            {
                if (t.destinationState == to) return;
            }
            var trans = from.AddTransition(to);
            trans.hasExitTime = true;
            trans.exitTime = 1.0f;
            trans.hasFixedDuration = true;
            trans.duration = 0.15f;
            trans.AddCondition(mode, threshold, param);
        }

        private static void SetupPlayerPrefab()
        {
            string prefabPath = CharactersDir + "/PlayerRig.prefab";
            string fbxPath = CharactersDir + "/PlayerRig.fbx";
            string controllerPath = ControllersDir + "/PlayerController.controller";
            string avatarPath = AvatarsDir + "/PlayerAvatar.asset";

            GameObject prefabRoot = null;
            GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

            if (fbxAsset != null)
            {
                prefabRoot = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
                prefabRoot.name = "PlayerRig";
            }
            else
            {
                prefabRoot = new GameObject("PlayerRig");
            }

            Animator animator = prefabRoot.GetComponent<Animator>();
            if (animator == null)
            {
                animator = prefabRoot.AddComponent<Animator>();
            }

            RuntimeAnimatorController rac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            animator.runtimeAnimatorController = rac;
            animator.applyRootMotion = false;

            Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
            if (avatar != null)
            {
                animator.avatar = avatar;
            }

            if (prefabRoot.GetComponent<CharacterAnimationController>() == null)
            {
                prefabRoot.AddComponent<CharacterAnimationController>();
            }

            if (prefabRoot.GetComponent<AnimationStateManager>() == null)
            {
                prefabRoot.AddComponent<AnimationStateManager>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            UnityEngine.Object.DestroyImmediate(prefabRoot);
            Debug.Log($"[GateCAssetBuilder] Saved {prefabPath} with Animator, CharacterAnimationController, and AnimationStateManager");
        }
    }
}
#endif
