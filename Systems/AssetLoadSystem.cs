using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Common;
using Colossal.Serialization.Entities;
using ctrlC.Components;
using ctrlC.Components.Prefabs;
using ctrlC.Components.Entities;
using ctrlC.Data;
using Game;
using Game.Prefabs;
using Game.UI.Menu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using StreamReader = System.IO.StreamReader;

public class MonoComponent : MonoBehaviour
{
	void Awake()
	{
		DontDestroyOnLoad(transform.gameObject);
	}
}
public static class ctrlCPrefabStorage
{
	public static Dictionary<string, PrefabBase> _prefabDict { get; set; } = new Dictionary<string, PrefabBase>();
	public static List<List<string>> _prefabList { get; set; } = new List<List<string>>();

	public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(AssetLoadSystem)}").SetShowsErrorsInUI(false);


    public static void LoadAssetsToStorage()
    {
        _prefabDict.Clear();
        _prefabList.Clear();

        var allPrefabs = AssetDatabase.user.GetAssets<PrefabAsset>();

        foreach (var prefab in allPrefabs)
        {
            if (prefab.name.StartsWith("ctrlC_"))
            {
                try
                {
                    PrefabBase p = prefab.Load() as PrefabBase;
                    if (p == null)
                    {
                        log.Warn($"Prefab {prefab.name} could not be loaded as PrefabBase.");
                        continue;
                    }

                    var comp = p.GetComponent<CtrlCPrefabComponent>();

                    if (comp != null)
                    {
                        string imagePath = Path.Combine(EnvironmentConstants.PrefabStorage, prefab.name, prefab.name + ".png");
                        imagePath = imagePath.Replace("\\", "/");

                        var prefabData = new List<string> { comp.c_id, comp.c_name, comp.c_description, imagePath, comp.c_category.ToString() };

                        // Kolla om prefaben redan finns i _prefabDict
                        if (!_prefabDict.ContainsKey(comp.c_id))
                        {
                            _prefabDict.Add(comp.c_id, p);
                            _prefabList.Add(prefabData);
                            log.Info($"Added prefab: {comp.c_name} with ID: {comp.c_id}");
                        }
                        else
                        {
                            log.Warn($"Prefab with ID {comp.c_id} already exists. Skipping addition.");
                        }
                    }
                    else
                    {
                        log.Warn($"Prefab {prefab.name} does not contain a CtrlCPrefabComponent.");
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error processing prefab {prefab.name}: {ex.Message}");
                }
            }
        }
    }
}
public partial class AssetLoadSystem : GameSystemBase
{
	public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(AssetLoadSystem)}").SetShowsErrorsInUI(false);

	private static PrefabSystem _prefabSystem;
	
	
	
	private static readonly string[] SupportedThumbnailExtensions = { ".png", ".svg" };
	private static readonly Dictionary<string, List<string>> MissingCids = new();
	
	
	private static MonoComponent _monoComponent;


	

	protected override void OnCreate()
	{
		base.OnCreate();
		_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
		_monoComponent = new GameObject("ctrlC-AssetLoadSystem").AddComponent<MonoComponent>();
		





	}

	private void OpenLink()
	{

	}

	protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
	{
		base.OnGameLoadingComplete(purpose, mode);
		if(mode == GameMode.MainMenu)
		{

			
			LoadCustomPrefabs();
		}
		
	}

	private void LoadCustomPrefabs()
	{
		if (!Directory.Exists(EnvironmentConstants.PrefabStorage))
		{
			Directory.CreateDirectory(EnvironmentConstants.PrefabStorage);
			log.Warn($"Prefab storage directory does not exist: {EnvironmentConstants.PrefabStorage}");
			return;
		}

		var modAssets = GetPrefabsFromDirectoryRecursively(EnvironmentConstants.PrefabStorage, "CustomPrefabs");
		_monoComponent.StartCoroutine(PrepareAssets(modAssets));
	}

	private IEnumerator PrepareAssets(Dictionary<string, List<FileInfo>> modAssets)
	{
		foreach (var mod in modAssets)
		{
			foreach (var file in mod.Value)
			{
				try
				{
					

					
					

					var fileName = Path.GetFileNameWithoutExtension(file.Name);
					var relativePath = EnvironmentConstants.RelativePath.Replace("\\", "/");
					relativePath = relativePath + "/" + fileName;
					relativePath = Uri.UnescapeDataString(relativePath);
				
					var path = AssetDataPath.Create(relativePath, fileName);

				
					

					var cidFilename = Path.Combine(EnvironmentConstants.PrefabStorage, fileName, fileName + ".Prefab.cid");
					var thumbnailFilename = Path.Combine(EnvironmentConstants.PrefabStorage, fileName, fileName + ".png");
					using StreamReader sr = new StreamReader(cidFilename);
					var guid = sr.ReadToEnd();
					sr.Close();
					

					var a = AssetDatabase.user.AddAsset<PrefabAsset>(path, guid);
					

				}
				catch (Exception e)
				{
					log.Error($"Asset {file} could not be loaded: {e.Message}");
				}
			}
		}

		_monoComponent.StartCoroutine(LoadAssets());
		yield return null;
	}

	private IEnumerator LoadAssets()
	{
		var allPrefabs = AssetDatabase.user.GetAssets<PrefabAsset>();
		foreach (PrefabAsset prefabAsset in allPrefabs)
		{

			
			

			try
			{
				PrefabBase prefabBase = prefabAsset.Load() as PrefabBase;
				var i = _prefabSystem.AddPrefab(prefabBase, null, null, null);

				
				
			}
			catch (Exception e)
			{
				log.Error($"Asset {prefabAsset.name} could not be added to Database: {e.Message}");
			}

			yield return null;
		}
	}



	private static Dictionary<string, List<FileInfo>> GetPrefabsFromDirectoryRecursively(string directory, string modName)
	{
		Dictionary<string, List<FileInfo>> files = new();
		var dir = new DirectoryInfo(directory);

		foreach (var file in dir.GetFiles())
		{
			if (file.Extension == ".Prefab")
			{
				if (!files.ContainsKey(modName))
					files.Add(modName, new List<FileInfo>());

				files[modName].Add(file);
			}
			else if (SupportedThumbnailExtensions.Contains(file.Extension))
			{
				
			}
		}

		foreach (var subDir in dir.GetDirectories())
		{
			var subFiles = GetPrefabsFromDirectoryRecursively(subDir.FullName, modName);
			foreach (var kvp in subFiles)
			{
				if (!files.ContainsKey(kvp.Key))
					files.Add(kvp.Key, new List<FileInfo>());

				files[kvp.Key].AddRange(kvp.Value);
			}
		}

		return files;
	}



	protected override void OnUpdate() { }
}




