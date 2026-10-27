using UnityEditor;
using UnityEngine;

public class ContentExpansionAssetCreator : EditorWindow
{
    [MenuItem("NAAD/Create Content Expansion Locations Asset")]
    public static void CreateLocationsAsset()
    {
        // Create the ScriptableObject asset
        var asset = ScriptableObject.CreateInstance<ContentExpansionLocations>();
        
        // Set the asset name
        asset.name = "CoreLocations";
        
        // Populate ExtraLocations with sample data
        asset.ExtraLocations = new ContentExpansionLocations.LocationEntry[]
        {
            new ContentExpansionLocations.LocationEntry
            {
                Id = "rest_01",
                Name = "Taste of Lagos Restaurant",
                Description = "A popular Lagos restaurant serving jollof rice and grilled fish.",
                Type = ContentExpansionLocations.LocationType.Restaurant,
                InteractionRange = 3f,
                CostToEnter = 2000
            },
            new ContentExpansionLocations.LocationEntry
            {
                Id = "lounge_01",
                Name = "City Lounge",
                Description = "Upscale lounge with views of the Lagos skyline.",
                Type = ContentExpansionLocations.LocationType.Lounge,
                InteractionRange = 2f,
                CostToEnter = 3000
            },
            new ContentExpansionLocations.LocationEntry
            {
                Id = "hotel_01",
                Name = "Eko Hotel & Suites",
                Description = "Luxury hotel in the heart of the business district.",
                Type = ContentExpansionLocations.LocationType.Hotel,
                InteractionRange = 1f,
                CostToEnter = 5000
            }
        };
        
        // Save the asset to the World directory
        string path = "Assets/NAAD/Gameplay/World/CoreLocations.asset";
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[ContentAsset] Created CoreLocations.asset with sample data.");
        
        // Close the window
        GetWindow<ContentExpansionAssetCreator>().Close();
    }
}