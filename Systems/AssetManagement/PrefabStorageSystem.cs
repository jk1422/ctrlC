using Colossal.Logging;
using ctrlC.Constants;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ctrlC.Systems.AssetManagement
{
    internal static class PrefabStorageSystem
    {
        internal static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(PrefabStorageSystem)}").SetShowsErrorsInUI(false);

        public static List<StorageObject> StoredPrefabs = new List<StorageObject>();

        public static List<List<string>> GetStringifiedPrefabs()
        {
            return StoredPrefabs
                .Select(prefab => prefab.GetStringified())
                .ToList();
        }

        public static bool TryGetPrefab(string id, out StorageObject result)
        {
            result = StoredPrefabs.FirstOrDefault(p => p.ID == id);

            return result != null;
        }

        internal static bool TrySaveNewPrefab(AssetStampPrefab prefab, string name, int category, out StorageObject result)
        {
            result = null;
            log.Info($"Will attempt to save prefab: {name}");
            try
            {
                if(!StorageObject.TryCreateNew(prefab, name, category, out StorageObject newStorageObject)) return false;

                log.Info($"Created new storage object:");
                log.Info($"ID: {newStorageObject.ID}");
                log.Info($"Name: {newStorageObject.Name}");
                log.Info($"Category: {newStorageObject.Category}");

                StoredPrefabs.Add(newStorageObject);
                result = newStorageObject;

                return true;
            }
            catch(Exception ex)
            {
                log.Error($"Failed to save prefab: {ex}");
                return false;
            }
        }

        internal static bool TryRemovePrefab(string id)
        {
            var asset = StoredPrefabs.FirstOrDefault(a => a.ID == id);
            if (asset != null)
            {
                if (asset.TryRemove())
                {
                    StoredPrefabs.Remove(asset);
                    return true;
                }
            }
            return false;
        }

        internal static bool TryRemovePrefab(StorageObject obj)
        {
            if (obj != null && StoredPrefabs.Contains(obj))
            {
                if (obj.TryRemove())
                {
                    StoredPrefabs.Remove(obj);
                    return true;
                }
            }
            return false;
        }
        internal static bool TryUpdatePrefab(string id, string name, int category)
        {
            var obj = StoredPrefabs.FirstOrDefault(p => p.ID == id);

            if (obj != null)
            {
                if(obj.TryUpdate(name, category))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool TryLoadPrefabs()
        {
            StoredPrefabs.Clear();

            // Move Included assets to prefab folder
            Stopwatch stopwatch = Stopwatch.StartNew();
            CheckAndMoveFeaturedAssets();
            stopwatch.Stop();
            log.Info($"Checked and moved in {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            // Get a list of all prefabs that needs to be loaded
            List<string> foldersToLoad = GetPrefabFolders(PathConstants.PrefabStoragePath);

            StoredPrefabs = LoadPrefabs(foldersToLoad);
            stopwatch.Stop();
            log.Info($"Loaded {StoredPrefabs.Count} saved prefabs to storage in {stopwatch.ElapsedMilliseconds}ms");


            return true;
        }

        private static List<StorageObject> LoadPrefabs(List<string> folders)
        {
            List<StorageObject> result = new List<StorageObject>();
            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder)) continue;
                if(StorageObject.TryCreateFromFolder(folder, out StorageObject obj))
                {
                    result.Add(obj);
                }
            }

            return result;
        }


        private static List<string> GetPrefabFolders(string directory)
        {
            if (!Directory.Exists(PathConstants.PrefabStoragePath))
            {
                Directory.CreateDirectory(PathConstants.PrefabStoragePath);
            }
            return Directory.GetDirectories(directory).ToList();
        }

        private static void CheckAndMoveFeaturedAssets()
        {
            // this is the mods location (\AppData\LocalLow\Colossal Order\Cities Skylines II\.cache\Mods\mods_subscribed\*ctrlC's ID and version*)
            string includedAssetsPath = Path.Combine(PathConstants.GetIncludedAssetsFolder());
            if (!Directory.Exists(includedAssetsPath)) return;

            DirectoryInfo dir = new DirectoryInfo(includedAssetsPath);
            
            log.Info($"Checking for included assets in '{includedAssetsPath}'...");
            if (dir.Exists && dir.GetDirectories().Length > 0)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    log.Info($"Found included asset directory: '{subDir.Name}'");

                    string destinationPath = Path.Combine(PathConstants.PrefabStoragePath, subDir.Name);

                    if (Directory.Exists(destinationPath))
                    {
                        log.Info($"Asset '{subDir.Name}' already exists in '{PathConstants.PrefabStoragePath}'. Removing the asset.");
                        Directory.Delete(subDir.FullName, true);
                        continue;
                    }

                    try
                    {
                        // Move the asset directory to the prefab storage location.
                        Directory.Move(subDir.FullName, destinationPath);
                        log.Info($"Successfully moved asset '{subDir.Name}' to '{PathConstants.PrefabStoragePath}'");
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
        }
    }
}
