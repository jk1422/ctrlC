using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using ctrlC.Components.Prefabs;
using ctrlC.Data;
using Game.Prefabs;
using System;
using System.IO;
using Unity.Entities;

namespace ctrlC.AssetManagement
{
    [Serializable]
    public class CtrlCPrefabData
    {
        // Stores metadata for the prefab, including its name, description, and image path.
        public string Name;
        public string Description;
        public string ImagePath;
    }

    public static class AssetSaveSystem
    {
        // Logger used for logging events and errors in the SaveSystem.
        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(AssetSaveSystem)}").SetShowsErrorsInUI(false);

        // Method for saving an AssetStampPrefab to the database.
        public static void SavePrefab(EntityManager entityManager, PrefabSystem prefabSystem, AssetStampPrefab prefab, string inputName, int category)
        {
            // Check if the prefab is null to prevent further operations on a non-existing object.
            if (prefab == null)
            {
                log.Error("Prefab object is null. Cannot proceed with saving.");
                return;
            }

            // Set the name of the prefab. If no name is provided, use the default "Saved Object".
            string name = string.IsNullOrEmpty(inputName) ? "Saved Object" : inputName;

            // Ensure the name is unique by appending a number if a prefab with the same name already exists.
            int count = 1;
            string originalName = name;
            string prefabDirectory = Path.Combine(EnvironmentConstants.PrefabStorage, $"ctrlC_{name}");
            while (Directory.Exists(prefabDirectory))
            {
                name = $"{originalName} ({count++})";
                prefabDirectory = Path.Combine(EnvironmentConstants.PrefabStorage, $"ctrlC_{name}");
            }

            // Ensure that the prefab has a components list before adding components to it.
            if (prefab.components == null)
            {
                log.Error("Prefab components list is null. Cannot add ctrlCObject or UIObject.");
                return;
            }

            try
            {
                // Add a custom component to the prefab that contains metadata such as name and category.
                prefab.components.Add(new CtrlCPrefabComponent
                {
                    c_name = name,
                    c_description = "",
                    c_imagePath = $"/ctrlC_{name}.png",
                    c_category = category
                });
                // Add a UIObject component to the prefab for user interface purposes.
                prefab.components.Add(new UIObject
                {
                    m_Priority = 10000,
                    name = "ctrlC"
                });
            }
            catch (Exception ex)
            {
                log.Error($"Error when adding components to prefab: {ex}");
                return;
            }

            // Adding the ctrlC_ prefix to the name for consistency 
            prefab.name = $"ctrlC_{name}";
            string relativePath = Uri.EscapeUriString(EnvironmentConstants.RelativePath.Replace("\\", "/")) + "/";

            // Create an asset path for saving the prefab and save it to the AssetDatabase.
            AssetDataPath path = AssetDataPath.Create(relativePath + prefab.name, prefab.name);
            (prefab.asset ?? AssetDatabase.user.AddAsset(path, prefab)).Save();

            // Create a thumbnail for the saved prefab to visually represent it in the UI.
            CreateThumbnail(prefab, Path.Combine(EnvironmentConstants.PrefabStorage, prefab.name).Replace("\\", "/") + "/");
        }

        // Method for creating a thumbnail for the prefab.
        private static void CreateThumbnail(AssetStampPrefab prefab, string modPath)
        {
            string defaultThumbnailPath = Path.Combine(EnvironmentConstants.ModPath, "images", "prefabThumbnail.png").Replace("\\", "/");
            string newThumbnailPath = Path.Combine(modPath, prefab.name + ".png").Replace("\\", "/");

            try
            {
                // Copy the default thumbnail image to the new location for the prefab.
                File.Copy(defaultThumbnailPath, newThumbnailPath, true);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to copy thumbnail image: {ex}");
            }
        }
    }
}