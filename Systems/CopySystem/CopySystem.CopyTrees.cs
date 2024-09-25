using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

namespace ctrlC.Systems
{
    internal static partial class CopySystem
    {
        private static void CopyTrees(List<Entity> trees, EntityManager entityManager, PrefabSystem prefabSystem, List<ObjectSubObjectInfo> subObjectInfos)
        {
            foreach (var selectedEntity in trees)
            {

                var prefabRef = entityManager.GetComponentData<PrefabRef>(selectedEntity);
                if (!prefabSystem.TryGetPrefab(entityManager.GetComponentData<PrefabData>(prefabRef.m_Prefab), out PrefabBase objectPre))
                {
                    continue;
                }

                if (objectPre is ObjectPrefab objectPrefab)
                {
                    var transform = entityManager.GetComponentData<Game.Objects.Transform>(selectedEntity);
                    float3 normalizedPosition = new float3(transform.m_Position.x - centroid.x, 0, transform.m_Position.z - centroid.z);

                    subObjectInfos.Add(new ObjectSubObjectInfo
                    {
                        m_Object = objectPrefab,
                        m_Position = normalizedPosition,
                        m_Rotation = transform.m_Rotation,
                        m_ParentMesh = 0,
                        m_GroupIndex = 0,
                        m_Probability = 100
                    });
                }

            }
        }
    }
}
