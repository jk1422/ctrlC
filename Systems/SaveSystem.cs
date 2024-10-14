using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using ctrlC.Components.Prefabs;
using ctrlC.Data;
using Game.Prefabs;
using System;
using System.IO;
using Unity.Entities;

namespace ctrlC.Systems
{
    [Serializable]
	public class CtrlCPrefabData
	{
		public string Name;
		public string Description;
		public string ImagePath;
	}
	public static class SaveSystem
	{
		public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(SaveSystem)}").SetShowsErrorsInUI(false);


		public static void SavePrefab(EntityManager entityManager, PrefabSystem prefabSystem, AssetStampPrefab prefab, string inputName, int category)
		{
			string name = "Saved Object";


			if (!string.IsNullOrEmpty(inputName))
			{
				name = inputName;
			}
			// Create UIObjectData
			UIObject uiObject = new UIObject
			{

				m_Priority = 10000,
				name = "ctrlC"
			};
			CtrlCPrefabComponent ctrlCObject = new CtrlCPrefabComponent
			{
				c_name = name,
				c_description = "ctrlC Object",
				c_imagePath = "/ctrlC_" + name + ".png",
				c_category = category
			};
			if (prefab == null)
			{
				log.Error("Prefab object is null. Cannot add ctrlCObject.");
				return;
			}
			if (prefab.components == null)
			{
				log.Error("Prefab components list is null. Cannot add ctrlCObject.");
				return;
			}
			if (ctrlCObject == null)
			{
				log.Error("ctrlCObject is null. Cannot add to prefab.");
				return;
			}
			try
			{
				prefab.components.Add(ctrlCObject);
			}
			catch (Exception ex)
			{
				log.Error($"Error when adding ctrlCObject: {ex}");
			}
			// Assign UIObjectData to AssetStampPrefab
			prefab.components.Add(uiObject);

			prefab.name = "ctrlC_" + name;
			string relativePath = EnvironmentConstants.RelativePath.Replace("\\", "/");

			string escapedRelativePath = Uri.EscapeUriString(relativePath);
			escapedRelativePath = escapedRelativePath + "/";

			AssetDataPath path = AssetDataPath.Create(escapedRelativePath + prefab.name, prefab.name ?? "");
			(prefab.asset ?? AssetDatabase.user.AddAsset(path, prefab)).Save();
            string ModPath = EnvironmentConstants.PrefabStorage + "/" + prefab.name +"/";
            CreateThumbnail(prefab, ModPath);

        }

		private static void CreateThumbnail(AssetStampPrefab prefab, string ModPath)
		{
            // Sökväg till standard thumbnail bilden
            string dllPath = EnvironmentConstants.ModPath;
            string imagesFolderPath = Path.Combine(dllPath, "images");
            // Sökväg till standard thumbnailbilden i images mappen
            string defaultThumbnailPath = Path.Combine(imagesFolderPath, "prefabThumbnail.png").Replace("\\", "/");
            // Sökväg till den nya thumbnailbilden
            string newThumbnailPath = Path.Combine(ModPath, prefab.name +".png").Replace("%5C", "/").Replace("%20", " ").Replace("\\", "/");

            // Kopiera standardbilden till prefab mappen
            try
            {
                File.Copy(defaultThumbnailPath, newThumbnailPath, true); // true för att skriva över om filen redan finns
            }
            catch (Exception ex)
            {
                log.Error($"Failed to copy thumbnail image: {ex}");
            }
        }
	}
}
