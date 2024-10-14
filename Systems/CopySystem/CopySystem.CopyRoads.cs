using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Game.Net;
using Colossal.Mathematics;
using Game.Prefabs;
using Unity.Mathematics;
using Colossal.Entities;

namespace ctrlC.Systems
{
    internal static partial class CopySystem
    {
        private static void MapNodesToIndices(List<Entity> entities, EntityManager entityManager, Dictionary<Entity, int> nodeIndexMapping, ref int currentIndex)
        {
            foreach (var selected in entities)
            {
                if (!entityManager.HasComponent<Edge>(selected))
                    continue;

                var edge = entityManager.GetComponentData<Edge>(selected);
                var startNode = edge.m_Start;
                var endNode = edge.m_End;

                if (!nodeIndexMapping.ContainsKey(startNode))
                    nodeIndexMapping[startNode] = currentIndex++;
                if (!nodeIndexMapping.ContainsKey(endNode))
                    nodeIndexMapping[endNode] = currentIndex++;
            }
        }

        private static void ProcessEdgesAndCurves(List<Entity> entities, EntityManager entityManager, PrefabSystem prefabSystem, List<ObjectSubNetInfo> subNetInfos, Dictionary<Entity, int> nodeIndexMapping)
        {
            foreach (var selectedEntity in entities)
            {
                if (!entityManager.HasComponent<Edge>(selectedEntity) || !entityManager.HasComponent<Curve>(selectedEntity))
                    continue;

                var curve = entityManager.GetComponentData<Curve>(selectedEntity);
                var prefabRef = entityManager.GetComponentData<PrefabRef>(selectedEntity);
                if (!prefabSystem.TryGetPrefab(entityManager.GetComponentData<PrefabData>(prefabRef.m_Prefab), out PrefabBase netPre))
                    continue;
                if (netPre is NetPrefab netPrefab)
                {
                    var elevation = entityManager.GetComponentData<Game.Net.Elevation>(selectedEntity);
                    entityManager.AddComponentData(selectedEntity, new Game.Net.Elevation { m_Elevation = elevation.m_Elevation });

                    var edge = entityManager.GetComponentData<Edge>(selectedEntity);
                    var startNode = edge.m_Start;
                    var endNode = edge.m_End;
                    float startEl = 0f;
                    float endEl = 0f;
                    if (entityManager.TryGetComponent<Elevation>(startNode, out Elevation startElevation))
                    {
                        if (startElevation.m_Elevation.x > 0.1f || startElevation.m_Elevation.y > 0.1f)
                        {
                            startEl = startElevation.m_Elevation.x;
                        }
                    }
                    if (entityManager.TryGetComponent<Elevation>(endNode, out Elevation endElevation))
                    {
                        if (endElevation.m_Elevation.x > 0.1f || endElevation.m_Elevation.y > 0.1f)
                        {
                            endEl = endElevation.m_Elevation.x;
                        }
                    }

                    float lowestY = Math.Min(curve.m_Bezier.a.y, Math.Min(curve.m_Bezier.b.y, Math.Min(curve.m_Bezier.c.y, curve.m_Bezier.d.y)));
                    float lowestElevation = Math.Min(startEl, endEl);
                    lowestY = lowestY - lowestElevation;
                    int2 nodeIndex = new int2(
                        nodeIndexMapping.TryGetValue(startNode, out int startIndex) ? startIndex : -1,
                        nodeIndexMapping.TryGetValue(endNode, out int endIndex) ? endIndex : -1
                    );



                    Bezier4x3 normalizedBezier = new Bezier4x3
                    {
                        a = new float3(curve.m_Bezier.a.x - centroid.x, startEl, curve.m_Bezier.a.z - centroid.z),
                        b = new float3(curve.m_Bezier.b.x - centroid.x, 0, curve.m_Bezier.b.z - centroid.z),
                        c = new float3(curve.m_Bezier.c.x - centroid.x, 0, curve.m_Bezier.c.z - centroid.z),
                        d = new float3(curve.m_Bezier.d.x - centroid.x, endEl, curve.m_Bezier.d.z - centroid.z)
                    };


                    subNetInfos.Add(new ObjectSubNetInfo
                    {
                        m_NetPrefab = netPrefab,
                        m_BezierCurve = normalizedBezier,
                        m_NodeIndex = nodeIndex,
                        m_ParentMesh = new int2(1, 1)
                    });
                }
            }
        }
    }
}
