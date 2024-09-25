
using Unity.Collections;
using Unity.Entities;
using Game.Prefabs;


using Colossal.Logging;
using System;
using System.Collections.Generic;
using ctrlC.Components;
namespace ctrlC.Systems
{
	public static class UIObjectHelper
	{
		public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.UIObject").SetShowsErrorsInUI(false);
		public static UIGroupPrefab FindGroupPrefab(EntityManager entityManager, PrefabSystem prefabSystem)
		{
			var query = entityManager.CreateEntityQuery(typeof(UIObjectData));
			var entities = query.ToEntityArray(Allocator.TempJob);
			

			foreach (var entity in entities)
			{
				try
				{
					

					if (!entityManager.HasComponent<PrefabData>(entity))
					{
						log.Warn($"Entity {entity} does not have PrefabData component.");
						continue;
					}

					
					

					var prefabGroupIndex = entityManager.GetComponentData<PrefabData>(entity);
					

					if (prefabSystem.TryGetPrefab(prefabGroupIndex, out PrefabBase groupPrefab))
					{
						
						if (groupPrefab is UIGroupPrefab groupPrefabData && groupPrefabData.name == "RoadsIntersections")
						{
							entities.Dispose();
							return groupPrefab as UIGroupPrefab;
						}
					}
					else
					{
						log.Warn($"PrefabSystem could not find prefab for index {prefabGroupIndex.m_Index}");
					}
				}
				catch (Exception)
				{
					
				}
			}

			entities.Dispose();
			return null;
		}

		public static void FindPrefabsInGroup(EntityManager entityManager)
		{
			var query = entityManager.CreateEntityQuery(typeof(UIObjectData));
			var entities = query.ToEntityArray(Allocator.TempJob);

			foreach (var entity in entities)
			{
				try
				{


					var data = entityManager.GetComponentData<UIObjectData>(entity);
					




				}
				catch (Exception)
				{

				}
			}
			


			
		}


	}
}
