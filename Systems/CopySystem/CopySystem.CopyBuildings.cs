using ctrlC.Components;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.UI.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using static Game.Prefabs.TriggerPrefabData;

namespace ctrlC.Systems
{
    internal static partial class CopySystem
    {
		
		private static void CopyBuildings(List<Entity> buildings, PrefabSystem prefabSystem)
		{
			foreach (var building in buildings)
			{
				log.Info($"building!");
				var prefabref = _entityManager.GetComponentData<PrefabRef>(building).m_Prefab;
				var prefabData = _entityManager.GetComponentData<PrefabData>(prefabref);

				
				if (!prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefab)) 
				{

					log.Error($"Error when getting prefab");
					continue; 
				
				}
				else
				{
					log.Info($"building had prefab");
					var transform = _entityManager.GetComponentData<Game.Objects.Transform>(building);
					var seed = _entityManager.GetComponentData<Game.Common.PseudoRandomSeed>(building);
					float3 normalizedPosition = new float3(transform.m_Position.x - centroid.x, 0, transform.m_Position.z - centroid.z);
					//subBuildingsInfos.Add(new CtrlCSubBuildingsInfo
					//{
					//	m_Object = prefab as ObjectPrefab,
					//	m_Position = normalizedPosition,
					//	m_Rotation = transform.m_Rotation,
					//	seed = seed.m_Seed
                    //});

                    subObjectInfos.Add(new ObjectSubObjectInfo
					{
						m_Object = prefab as ObjectPrefab,
						m_Position = normalizedPosition,
						m_Rotation = transform.m_Rotation,
						m_GroupIndex = 0,
						m_ParentMesh = 0,
						m_Probability = 100,
					});
					log.Info($"added building to subBuildingsInfos");

				}


			}
		}
	}
}
