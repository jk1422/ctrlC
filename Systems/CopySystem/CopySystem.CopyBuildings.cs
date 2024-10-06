using Colossal.Entities;
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
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using static Game.Prefabs.TriggerPrefabData;

namespace ctrlC.Systems
{
    internal static partial class CopySystem
    {
		
		private static void CopyBuildings(List<Entity> buildings, PrefabSystem prefabSystem, List<Entity> roads, List<ObjectSubAreaInfo> areaInfos)
		{
			for (int i = 0; i < buildings.Count; i++)
			{
				var prefabref = _entityManager.GetComponentData<PrefabRef>(buildings[i]).m_Prefab;
				var prefabData = _entityManager.GetComponentData<PrefabData>(prefabref);

				if (!prefabSystem.TryGetPrefab(prefabData, out BuildingPrefab prefab)) continue;
				else
				{
					var transform = _entityManager.GetComponentData<Game.Objects.Transform>(buildings[i]);
					var seed = _entityManager.GetComponentData<Game.Common.PseudoRandomSeed>(buildings[i]);
					float3 normalizedPosition = new float3(transform.m_Position.x - centroid.x, 0, transform.m_Position.z - centroid.z);


                    ProcessSubElements(buildings[i], areaInfos);

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
        private static void ProcessSubElements(Entity building, List<ObjectSubAreaInfo> areaInfos)
		{
            if (_entityManager.TryGetBuffer<Game.Areas.SubArea>(building, true, out DynamicBuffer<Game.Areas.SubArea> areas))
            {
                foreach (var area in areas)
                {
                    var prefabRef = _entityManager.GetComponentData<PrefabRef>(area.m_Area);
                    if (!_prefabSystem.TryGetPrefab(_entityManager.GetComponentData<PrefabData>(prefabRef.m_Prefab), out PrefabBase areaPrefab))
                        continue;
                    if (areaPrefab is not SpacePrefab spacePrefab) continue;
                    DynamicBuffer<Game.Areas.Node> nodes = new DynamicBuffer<Game.Areas.Node>();
                    nodes = _entityManager.GetBuffer<Game.Areas.Node>(area.m_Area);


                    var nodePositions = new float3[nodes.Length];
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        nodePositions[i] = nodes[i].m_Position - centroid;
                    }

                    areaInfos.Add(new ObjectSubAreaInfo
                    {
                        m_AreaPrefab = spacePrefab,
                        m_NodePositions = nodePositions,
                        m_ParentMeshes = new int[0]
                    });

                }
            }
        }
	}
}
