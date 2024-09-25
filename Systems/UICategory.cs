using Colossal.Entities;
using Colossal.Logging;
using Colossal.UI;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace ctrlC.Systems
{
	
	public static class UICategory
	{
		public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.UICategory").SetShowsErrorsInUI(false);
		private static EntityManager entityManager;
		private static PrefabSystem prefabSystem;
		public static void CreateOrFindMenu(EntityManager _entityManager, PrefabSystem _prefabSystem)
		{
			entityManager = _entityManager;
			prefabSystem = _prefabSystem;

			Entity toolbarGroup = FindToolbarGroup();
			if (toolbarGroup == Entity.Null)
			{
				// Handle error: Toolbar group not found
				log.Error("Toolbar group not found.");
				return;
			}

			UIAssetMenuPrefab assetMenuPrefab = CreateAssetMenuPrefab();

			prefabSystem.AddPrefab(assetMenuPrefab);
			prefabSystem.Update();
			
			
			// Create an entity and assign the prefab data


			Entity assetMenuEntity = entityManager.CreateEntity();
			entityManager.AddComponentData(assetMenuEntity, new PrefabData { m_Index = assetMenuPrefab.GetInstanceID() });

			

			entityManager.AddComponentData(assetMenuEntity, new UIObjectData { m_Group = toolbarGroup, m_Priority = 31 });
			entityManager.AddComponent<UIAssetCategoryData>(assetMenuEntity);
			// Add the UIGroupElement to the toolbar group
			DynamicBuffer<UIGroupElement> buffer = entityManager.GetBuffer<UIGroupElement>(toolbarGroup);
			buffer.Add(new UIGroupElement { m_Prefab = assetMenuEntity });

			
		}

		public static UIAssetMenuPrefab CreateAssetMenuPrefab()
		{
			UIAssetMenuPrefab assetMenuPrefab = ScriptableObject.CreateInstance<UIAssetMenuPrefab>();
			assetMenuPrefab.name = "ctrlC";
			assetMenuPrefab.isDirty = true;
			assetMenuPrefab.active = true;

			UIObject uiObject = new UIObject
			{
				m_Group = assetMenuPrefab,
				m_Priority = 31,
				m_Icon = "Media/Game/Icons/ZoneSignature.svg",
				m_LargeIcon = null,
				m_IsDebugObject = false,
				active = true
			};

			assetMenuPrefab.components.Add(uiObject);
			return assetMenuPrefab;
		}

		public static Entity FindToolbarGroup()
		{
			var query = entityManager.CreateEntityQuery(typeof(UIToolbarGroupData));
			var entities = query.ToEntityArray(Allocator.TempJob);
			log.Info($"Number of entities with UIToolbarGroupData: {entities.Length}");

			foreach (var entity in entities)
			{
				try
				{
					log.Info($"Processing entity {entity.Index}");

					if (!entityManager.HasBuffer<UIGroupElement>(entity))
					{
						log.Warn($"Entity {entity} does not have UIGroupElement buffer.");
						continue;
					}




					entityManager.TryGetBuffer<UIGroupElement>(entity,false ,out DynamicBuffer<UIGroupElement> buffer);
					if (buffer.Length == 0)
					{
						log.Warn($"Entity {entity} does not have UIGroupElement buffer.");
						continue;
					}
					log.Info($"Buffer: {buffer}");

					// iterate through buffer
					foreach (var element in buffer)
					{
						if (prefabSystem.TryGetPrefab(element.m_Prefab, out PrefabBase menuPrefab))
						{
							log.Info($"GroupPrefabName is: {menuPrefab.name}");
							if (menuPrefab is UIAssetMenuPrefab assetMenuPrefab && assetMenuPrefab.name == "Zones")
							{

								log.Info($"Toolbar entity: {entity}");
								return entity;
							}
						}
						else
						{
							log.Warn($"PrefabSystem could not find prefab for index {element.m_Prefab}");
						}
					}


				}
				catch (Exception ex)
				{
					log.Error($"Error processing entity {entity.Index}: {ex.Message}");
				}
			}

			entities.Dispose();
			return Entity.Null;
		}


	}
}
