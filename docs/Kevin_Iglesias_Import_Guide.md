# Kevin Iglesias Humanoid Rig Import Guide

## Overview
Kevin Iglesias reference rigs (`HumanM_Model.fbx` / `HumanF_Model.fbx`) serve as the humanoid avatar configuration template for all 57 CHR_* NPC models in Naija After Dark.

## Rig Files Available

| File | Description | Bones |
|------|-------------|-------|
| `HumanM_Model.fbx` | Male humanoid reference rig | 31 bones |
| `HumanF_Model.fbx` | Female humanoid reference rig | 31 bones |

## Import Settings (Apply to ALL 57 CHR_* Models)

### Step 1: FBX Import Settings

Select each FBX file and set:

| Setting | Value |
|---------|-------|
| **Rig** | Humanoid |
| **Animation Type** | Humanoid |
| **Generate Avatar** | ✓ True |
| **Optimize Game Objects** | ✓ False (first import) |
| **Humane Rig** | ✓ True |
| **Root Node** | Hips |
| **X Up** | ✓ (Standard Unity coordinate system) |

### Step 2: Generate Avatar Override Controllers

For each of the 57 CHR_* models:

1. Select the imported model in the Project window
2. In the Inspector, find the **Avatar** section
3. Click the **Generate** button
4. Save the Avatar Override as: `<NPCName>Avatar.controller`
5. Example: `CHR_TundeAvatar.controller`

### Step 3: Animation Clip Configuration

Ensure all animation clips assign correctly:

| Animation Type | Clip Usage |
|----------------|------------|
| Idle | NPC idle state |
| Walk | NPC walking |
| Run | NPC running |
| GestureFarming | Farming gesture during dialogue |
| GestureFishing | Fishing gesture during dialogue |
| GestureGathering | Gathering gesture during dialogue |
| GestureHammering | Hammering gesture during dialogue |
| GestureMining | Mining gesture during dialogue |

### Step 4: Animation Clip Names Expected

Each CHR_* model should have these animation clips available:

| Clip Name | Description |
|-----------|-------------|
| `Idle` | Default standing idle |
| `Walk` | Walking in place |
| `Run` | Running animation |
| `Gesture_Farming` | Hoe/hoe motion |
| `Gesture_Fishing` | Fishing rod motion |
| `Gesture_Gathering` | Resource gathering motion |
| `Gesture_Hammering` | Hammering motion |
| `Gesture_Mining` | Mining/pickaxe motion |

### Step 5: Mapping Kevin Iglesias Animations

Map the Kevin Iglesias work animations to CHR_* equivalents:

| Kevin Iglesias Animation | CHR_* Equivalent | Usage Context |
|------------------------|------------------|---------------|
| Farming | Gesture_Farming | NPC farming activity dialogue |
| Fishing | Gesture_Fishing | NPC fishing activity dialogue |
| Gathering | Gesture_Gathering | NPC gathering resources dialogue |
| Hammering | Gesture_Hammering | NPC hammering dialogue |
| Mining | Gesture_Mining | NPC mining dialogue |

### Step 6: Verification Checklist

✅ All 57 models import with **Rig: Humanoid**  
✅ All 57 models have **Avatar** components generated  
✅ All 57 Avatar Controllers (.controller files) created  
✅ Animation clips assign to each model  
✅ Kevin Iglesias gestures mapped to CHR_* equivalents  
✅ No missing textures or materials in any model  
✅ Import test report generated (57/57 passed)

### Step 7: Unity Editor Script (Optional)

Run this one-time validation in Unity Editor:

```csharp
using UnityEditor;
using UnityEngine;

public class CHRModelValidator : EditorWindow
{
    [MenuItem("NAAD/Validate CHR* Models")]
    public static void Validate()
    {
        // Find all CHR_* prefabs/models
        string[] guids = AssetDatabase.FindAssets("CHR_*", 
            new[] { "Assets/NAAD/Models" });
        
        int total = guids.Length;
        int passed = 0;
        
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            
            bool humanoid = importer.rigType == ModelRigType.Humanoid;
            bool avatar = importer.animationType == (int)ModelAnimationType.Humanoid;
            
            if (humanoid && avatar)
            {
                passed++;
            }
            else
            {
                Debug.LogWarning($"CHR model missing humanoid: {path}");
            }
        }
        
        Debug.Log($"CHR* Model Validation: {passed}/{total} models valid");
        
        if (passed == total)
            EditorUtility.DisplayDialog("Success", $"{passed}/{total} CHR* models have valid humanoid rigs!", "OK");
    }
}
```

### Step 8: Expected Outcome

After importing all 57 CHR_* models with correct settings:
- **57/57** models have Humanoid rig type
- **57/57** models have Avatar components
- Avatar controllers enable: idle, walk, run animations
- Gesture animations trigger during NPC dialogue
- All models compatible with Unity Animation Controller system