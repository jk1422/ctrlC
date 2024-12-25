using Colossal.IO.AssetDatabase;
using ctrlC.Components.Prefabs;
using ctrlC.Constants;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Policy;
using Unity.Entities;
using UnityEngine;

namespace ctrlC.Systems.AssetManagement
{
    [Serializable]
    public class MetaData
    {
        public string id;
        public string cid;
        public string name;
        public int category;
    }
    public class StorageObject
    {
        public AssetStampPrefab Prefab { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string PrefixedName => $"ctrlC_{ID}";
        public int Category { get; set; }
        public CtrlCPrefabComponent CtrlCComponent { get; set; }
        private  Colossal.Hash128 _CID;
        public Colossal.Hash128 GetCID()
        {
            if (_CID == Colossal.Hash128.empty)
            {
                if (!File.Exists(CIDPath))
                {
                    PrefabStorageSystem.log.Error($"CID file not found at: {CIDPath}");
                    return Colossal.Hash128.empty;
                }

                using (StreamReader sr = new StreamReader(CIDPath))
                {
                    _CID = Colossal.Hash128.Parse(sr.ReadToEnd().Trim());
                }
            }
            return _CID;
        }

        /// <summary>
        /// Path to the prefab folder
        /// </summary>
        public string PrefabFolderPath => Path.Combine(PathConstants.PrefabStoragePath, PrefixedName).Replace("\\", "/");
        /// <summary>
        /// Gets the short path (".ctrlC~/Saved Prefab/").
        /// </summary>
        public string PrefabFolderPathShort => Path.Combine(PathConstants.PrefabStoragePathShort, PrefixedName).Replace("\\", "/");

        /// <summary>
        /// Path to the .prefab file
        /// </summary>
        public string PrefabPath => Path.Combine(PrefabFolderPath, $"{PrefixedName}.prefab").Replace("\\", "/");

        /// <summary>
        /// Path to the thumbnail
        /// </summary>
        public string ThumbnailPath => Path.Combine(PrefabFolderPath, $"{PrefixedName}.png").Replace("\\", "/");

        /// <summary>
        /// Path to the .prefab.cid file
        /// </summary>
        public string CIDPath => Path.Combine(PrefabFolderPath, $"{PrefixedName}.Prefab.cid").Replace("\\", "/");

        /// <summary>
        /// Path to the meta file
        /// </summary>
        public string MetaFilePath => Path.Combine(PrefabFolderPath, $"meta.json").Replace("\\", "/");


        public List<string> GetStringified()
        {
            return new List<string> { ID, Name, Category.ToString(), ThumbnailPath };
        }

        public static bool TryCreateNew(AssetStampPrefab prefab, string name, int category, out StorageObject result)
        {
            result = new StorageObject();
            if (string.IsNullOrEmpty(name))
            {
                PrefabStorageSystem.log.Warn($"Name was null or empty, setting name to default value");
                name = "Saved Object";
            }

            if (prefab == null)
            {
                PrefabStorageSystem.log.Error($"Prefab cannot be null");
                return false;
            }


            result = new StorageObject
            {
                Prefab = prefab,
                ID = Guid.NewGuid().ToString(),
                Name = name,
                Category = category,
            };

            result.CreateCtrlCComponent();
            if (!result.TryCreatePrefabFiles()) return false;
            if (!result.TryCopyThumbnail()) return false;
            result.CreateMetaFile();
            return true;
        }

        private void CreateMetaFile()
        {
            MetaData metaData = new MetaData()
            {
                name = Name,
                id = ID,
                cid = GetCID().ToString(),
                category = Category
            };
            string jsonString = JsonUtility.ToJson(metaData, true);

            File.WriteAllText(MetaFilePath, jsonString);
        }

        public static bool TryCreateFromFolder(string prefabFolder, out StorageObject result)
        {
            PrefabStorageSystem.log.Info($"Trying to load prefab from {prefabFolder}");
            result = new StorageObject();
            string metaFile = Path.Combine(prefabFolder, "meta.json");

            if (!File.Exists(metaFile))
            {
                PrefabStorageSystem.log.Info($"This prefab is outdated.");
                return false;
            }

            string jsonString = File.ReadAllText(metaFile);
            var metaData = JsonUtility.FromJson<MetaData>(jsonString);

            result.Name = metaData.name;
            result.ID = metaData.id;
            result.Category = metaData.category;
            result._CID = Colossal.Hash128.Parse(metaData.cid);

            var prefabFolderShort = Path.Combine(PathConstants.PrefabStoragePathShort, result.PrefixedName).Replace("\\", "/");

            var assetPath = AssetDataPath.Create(prefabFolderShort, result.PrefixedName);

            if (AssetDatabase.user.AddAsset<PrefabAsset>(assetPath, result.GetCID()).Load() is PrefabBase prefabBase)
            {
                PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
                prefabSystem.AddPrefab(prefabBase);
                result.Prefab = prefabBase as AssetStampPrefab;
            
                return true;
            }

            return false;
        }

        public bool TryRemove()
        {
            var databaseAsset = AssetDatabase.user.GetAsset(GetCID());

            if (databaseAsset != null)
            {
                databaseAsset.Unload(true);

                GC.Collect();
                GC.WaitForPendingFinalizers();

                try
                {
                    AssetDatabase.user.DeleteAsset(GetCID());
                    PrefabStorageSystem.log.Info($"Successfully deleted prefab: {PrefixedName}");


                    if (Directory.Exists(PrefabFolderPath))
                    {
                        foreach (var file in Directory.GetFiles(PrefabFolderPath))
                        {
                            try
                            {
                                File.Delete(file);
                                PrefabStorageSystem.log.Info($"Deleted file: {file}");
                            }
                            catch (IOException ex)
                            {
                                PrefabStorageSystem.log.Warn($"Failed to delete file '{file}': {ex.Message}");
                                return false;
                            }
                        }

                        // Ta bort själva mappen
                        try
                        {
                            Directory.Delete(PrefabFolderPath, true);
                            PrefabStorageSystem.log.Info($"Successfully deleted prefab folder: {PrefabFolderPath}");
                        }
                        catch (IOException ex)
                        {
                            PrefabStorageSystem.log.Error($"Failed to delete prefab folder '{PrefabFolderPath}': {ex.Message}");
                            return false;
                        }
                    }
                    else
                    {
                        PrefabStorageSystem.log.Warn($"Prefab folder '{PrefabFolderPath}' does not exist.");
                        return false;
                    }

                    return true;
                }
                catch (IOException ex)
                {
                    PrefabStorageSystem.log.Error($"Failed to delete asset. File might still be in use: {ex.Message}");
                }
            }
            return false;
        }

        public bool TryUpdate(string name, int category)
        {
            try
            {
                Name = name;
                Category = category;

                CreateMetaFile();
                return true;
            }
            catch
            {
                return false;
            }
        }


        private bool TryCreatePrefabFiles()
        {
            try
            {
                AssetDataPath path = AssetDataPath.Create(PrefabFolderPathShort, PrefixedName);
                (Prefab.asset ?? AssetDatabase.user.AddAsset(path, Prefab)).Save();
                return true;
            }
            catch (Exception ex)
            {
                PrefabStorageSystem.log.Error($"Error when trying to add prefab to database: {ex}");
                return false;
            }
        }

        private void CreateCtrlCComponent()
        {
            CtrlCComponent = new CtrlCPrefabComponent
            {
                c_name = Name,
                c_description = "",
                c_imagePath = ThumbnailPath,
                c_category = Category,
                c_id = ID
            };
            Prefab.components.Add(CtrlCComponent);
        }
        private bool TryCopyThumbnail()
        {
            string defaultThumbnailPath = PathConstants.GetDefaultThumbnailPath();

            if (!Directory.Exists(PrefabFolderPath))
            {
                PrefabStorageSystem.log.Error($"Failed to copy thumbnail: Directory didn't exist");
                return false;
            }

            try
            {
                File.Copy(defaultThumbnailPath, ThumbnailPath, true);
                return true;
            }
            catch (Exception ex)
            {
                PrefabStorageSystem.log.Error($"Failed to copy thumbnail image: {ex}");
                return false;
            }
        }
    }
}
