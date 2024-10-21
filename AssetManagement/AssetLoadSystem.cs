using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using ctrlC.Components.Prefabs;
using ctrlC.Data;
using Game.Prefabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace ctrlC.AssetManagement
{
    public class MonoComponent : MonoBehaviour
    {
        // Prevents this GameObject from being destroyed when loading a new scene.
        void Awake()
        {
            DontDestroyOnLoad(transform.gameObject);
        }
    }
    public static class ctrlCPrefabStorage
    {
        // Dictionary that stores loaded prefabs, indexed by a unique string ID.
        // This allows us to quickly look up prefabs by their unique ID.
        public static Dictionary<string, PrefabBase> PrefabDict { get; private set; } = new Dictionary<string, PrefabBase>();

        // List that stores metadata about each prefab, including ID, name, description, image path, and category.
        // The metadata is useful for displaying information about the prefabs in the UI or for categorization.
        public static List<List<string>> PrefabList { get; private set; } = new List<List<string>>();

        // Logger
        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(AssetLoadSystem)}").SetShowsErrorsInUI(false);

        // Loads all prefabs from the asset database into storage.
        // This method clears any existing prefabs and then loads new ones from the asset database.
        public static void LoadAssetsToStorage()
        {
            PrefabDict.Clear();
            PrefabList.Clear();

            // Get all prefabs whose names start with "ctrlC_" from the asset database.
            // Using the ctrlC_ prefix to ensure we are not trying to load anything else.
            var allPrefabs = AssetDatabase.user.GetAssets<PrefabAsset>().Where(prefab => prefab.name.StartsWith("ctrlC_")).ToList();

            foreach (var prefab in allPrefabs)
            {
                try
                {
                    // Attempt to load the prefab as a PrefabBase type.
                    if (prefab.Load() is PrefabBase p)
                    {
                        var comp = p.GetComponent<CtrlCPrefabComponent>();

                        if (comp != null)
                        {
                            // The image path to the thumbnail to help the UI system find the correct thumbnail
                            string imagePath = Path.Combine(EnvironmentConstants.PrefabStorage, prefab.name, prefab.name + ".png").Replace("\\", "/");

                            // Store metadata about the prefab.
                            // This metadata is later used by the UI System to get the correct name, thumbnail, category and so on. 
                            var prefabData = new List<string> { comp.c_id, comp.c_name, comp.c_description, imagePath, comp.c_category.ToString() };

                            // Add the prefab to the dictionary and metadata list if it doesn't already exist.
                            if (!PrefabDict.ContainsKey(comp.c_id))
                            {
                                PrefabDict.Add(comp.c_id, p);
                                PrefabList.Add(prefabData);
                                log.Info($"Successfully added prefab: {comp.c_name} with ID: {comp.c_id}");
                            }
                            else
                            {
                                log.Warn($"Prefab with ID {comp.c_id} already exists in storage. Skipping addition.");
                            }
                        }
                        else
                        {
                            log.Warn($"Prefab '{prefab.name}' does not contain the required CtrlCPrefabComponent. Skipping prefab.");
                        }
                    }
                    else
                    {
                        log.Warn($"Failed to load prefab '{prefab.name}' as PrefabBase. Skipping prefab.");
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error while processing prefab '{prefab.name}': {ex.Message}");
                }
            }
        }
    }
    public static class AssetLoadSystem
    {
        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(AssetLoadSystem)}").SetShowsErrorsInUI(false);
        private static MonoComponent _monoComponent;
        private static PrefabSystem _prefabSystem;

        // Initializes and loads custom prefabs into the system.
        // This function sets up necessary components and starts the asynchronous loading process.
        public static void LoadCustomPrefabs()
        {
            // Get (or create) the PrefabSystem, which we will use later when adding prefabs.
            _prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();

            // Create a new GameObject to manage coroutines and ensure it persists across scenes.
            _monoComponent = new GameObject("ctrlC-AssetLoadSystem").AddComponent<MonoComponent>();

            // Create the prefab storage directory(/.ctrlC~) if it does not exist.
            if (!Directory.Exists(EnvironmentConstants.PrefabStorage))
            {
                Directory.CreateDirectory(EnvironmentConstants.PrefabStorage);
            }

            // Start the asynchronous loading process.
            _monoComponent.StartCoroutine(AsyncronousLoad());
        }

        // Asynchronous process to load assets.
        // This coroutine runs asynchronously to prevent blockage of the game's and other mods loading processes
        private static IEnumerator AsyncronousLoad()
        {
            // First, we check for any featured assets that are included with the mod and move them to the /.ctrlC~ folder
            yield return CheckAndMoveFeaturedAssets();

            log.Info("CheckAndMoveFeaturedAssets has completed. Proceeding...");

            // Retrieve all prefabs from the prefab storage directory(/.ctrlC~).
            List<FileInfo> prefabs = GetPrefabs(EnvironmentConstants.PrefabStorage);

            log.Info("GetPrefabsFinished has completed. Proceeding...");

            // Prepare the retrieved prefabs for use.
            yield return PrepareAssets(prefabs);
        }

        // Retrieves all prefabs from a specified directory and its subdirectories.
        // This method searches the given directory recursively for prefab files.
        private static List<FileInfo> GetPrefabs(string directory)
        {
            return Directory.GetFiles(directory, "ctrlC_*.Prefab", SearchOption.AllDirectories)
                            .Select(path => new FileInfo(path)).ToList();
        }

        // Checks for included assets and moves them to the .ctrlC~ folder
        // This ensures that any featured assets are in the correct location for loading.
        private static IEnumerator CheckAndMoveFeaturedAssets()
        {
            string includedAssetsPath = Path.Combine(EnvironmentConstants.ModPath, ".CtrlCAssets");

            DirectoryInfo dir = new DirectoryInfo(includedAssetsPath);

            log.Info($"Checking for included assets in '{includedAssetsPath}'...");
            if (dir.Exists && dir.GetDirectories().Length > 0)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    log.Info($"Found included asset directory: '{subDir.Name}'");

                    string destinationPath = Path.Combine(EnvironmentConstants.PrefabStorage, subDir.Name);

                    if (Directory.Exists(destinationPath))
                    {
                        log.Info($"Asset '{subDir.Name}' already exists in '{EnvironmentConstants.PrefabStorage}'. Removing the asset.");
                        Directory.Delete(subDir.FullName, true);
                        continue;
                    }

                    try
                    {
                        // Move the asset directory to the prefab storage location.
                        Directory.Move(subDir.FullName, destinationPath);
                        log.Info($"Successfully moved asset '{subDir.Name}' to '{EnvironmentConstants.PrefabStorage}'");
                    }
                    catch (Exception ex)
                    {
                        // Log an error if the move operation fails.
                        log.Error($"Failed to move asset '{subDir.Name}': {ex.Message}");
                    }
                }
            }
            else
            {
                log.Info("No included assets found in the specified directory.");
            }
            yield break;
        }

        // Prepares assets for loading by creating asset paths and adding them to the asset database.
        // This method reads metadata and adds each asset to the asset database so it can be used by the game.
        private static IEnumerator PrepareAssets(List<FileInfo> modAssets)
        {
            foreach (var file in modAssets)
            {
                try
                {
                    // Extract the file name without extension and create the relative path.
                    var fileName = Path.GetFileNameWithoutExtension(file.Name);
                    var relativePath = Path.Combine(EnvironmentConstants.RelativePath, fileName).Replace("\\", "/");
                    relativePath = Uri.UnescapeDataString(relativePath);

                    // Create the path object that represents where the asset should be in the database.
                    var path = AssetDataPath.Create(relativePath, fileName);
                    var cidFilename = Path.Combine(EnvironmentConstants.PrefabStorage, fileName, fileName + ".Prefab.cid");

                    // Check if the CID file exists before proceeding.
                    if (!File.Exists(cidFilename))
                    {
                        log.Warn($"CID file for prefab '{fileName}' not found. Skipping this asset.");
                        continue;
                    }

                    // Read the CID (a unique identifier for the asset) and add the asset to the database.
                    using (StreamReader sr = new StreamReader(cidFilename))
                    {
                        var CID = sr.ReadToEnd().Trim();
                        AssetDatabase.user.AddAsset<PrefabAsset>(path, CID);
                    }
                }
                catch (Exception e)
                {
                    // Log an error if the asset cannot be loaded.
                    log.Error($"Failed to load asset '{file.Name}': {e.Message}");
                }
            }
            yield return LoadAssets();
        }

        // Loads all assets into the prefab system for use in the game.
        // This final step adds the prefabs to the prefabsystem and makes them available in the game
        private static IEnumerator LoadAssets()
        {
            var allPrefabs = AssetDatabase.user.GetAssets<PrefabAsset>();
            foreach (PrefabAsset prefabAsset in allPrefabs)
            {
                try
                {
                    // Attempt to load the asset as a PrefabBase and add it to the prefab system.
                    if (prefabAsset.Load() is PrefabBase prefabBase)
                    {
                        _prefabSystem.AddPrefab(prefabBase, null, null, null);
                    }
                }
                catch (Exception e)
                {
                    // Log an error if the asset cannot be added to the prefab system.
                    log.Error($"Failed to add asset '{prefabAsset.name}' to the database: {e.Message}");
                }

                yield return null;
            }
        }
    }
}
