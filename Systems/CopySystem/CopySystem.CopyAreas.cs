using ctrlC.Components;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ctrlC.Systems
{
    internal static partial class CopySystem
    {
        private static void CopyAreas(List<Entity> entities, EntityManager entityManager, PrefabSystem prefabSystem, List<ObjectSubAreaInfo> objectSubAreaInfos)
        {

            foreach (var selectedEntity in entities)
            {
                if (!entityManager.HasComponent<Game.Areas.Surface>(selectedEntity))
                    continue;

                var prefabRef = entityManager.GetComponentData<PrefabRef>(selectedEntity);
                if (!prefabSystem.TryGetPrefab(entityManager.GetComponentData<PrefabData>(prefabRef.m_Prefab), out PrefabBase areaPrefab))
                    continue;

                var nodesBuffer = entityManager.GetBuffer<Game.Areas.Node>(selectedEntity);
                var nodePositions = new float3[nodesBuffer.Length];
                for (int i = 0; i < nodesBuffer.Length; i++)
                {
                    nodePositions[i] = nodesBuffer[i].m_Position - centroid;
                }

                objectSubAreaInfos.Add(new ObjectSubAreaInfo
                {
                    m_AreaPrefab = areaPrefab as AreaPrefab,
                    m_NodePositions = nodePositions,
                    m_ParentMeshes = new int[0]
                });
            }

        }

    }
}
