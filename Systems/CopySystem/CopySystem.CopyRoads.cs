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

namespace ctrlC.Systems
{
    internal static partial class CopySystem
    {
		private static void MapNodesToIndices(List<Entity> entities, EntityManager entityManager, Dictionary<Node, int> nodeIndexMapping, ref int currentIndex)
		{
			foreach (var selected in entities)
			{
				if (!entityManager.HasComponent<Edge>(selected))
					continue;

				var edge = entityManager.GetComponentData<Edge>(selected);
				var startNode = entityManager.GetComponentData<Node>(edge.m_Start);
				var endNode = entityManager.GetComponentData<Node>(edge.m_End);

				if (!nodeIndexMapping.ContainsKey(startNode))
					nodeIndexMapping[startNode] = currentIndex++;
				if (!nodeIndexMapping.ContainsKey(endNode))
					nodeIndexMapping[endNode] = currentIndex++;
			}
		}

        private static void ProcessEdgesAndCurves(List<Entity> entities, EntityManager entityManager, PrefabSystem prefabSystem, List<ObjectSubNetInfo> subNetInfos, Dictionary<Node, int> nodeIndexMapping)
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
                    var startNode = entityManager.GetComponentData<Node>(edge.m_Start);
                    var endNode = entityManager.GetComponentData<Node>(edge.m_End);
                    //h
                    int2 nodeIndex = new int2(
                        nodeIndexMapping.TryGetValue(startNode, out int startIndex) ? startIndex : -1,
                        nodeIndexMapping.TryGetValue(endNode, out int endIndex) ? endIndex : -1
                    );

                    Bezier4x3 normalizedBezier = new Bezier4x3
                    {
                        a = new float3(curve.m_Bezier.a.x - centroid.x, curve.m_Bezier.a.y - baseHeight, curve.m_Bezier.a.z - centroid.z),
                        b = new float3(curve.m_Bezier.b.x - centroid.x, curve.m_Bezier.b.y - baseHeight, curve.m_Bezier.b.z - centroid.z),
                        c = new float3(curve.m_Bezier.c.x - centroid.x, curve.m_Bezier.c.y - baseHeight, curve.m_Bezier.c.z - centroid.z),
                        d = new float3(curve.m_Bezier.d.x - centroid.x, curve.m_Bezier.d.y - baseHeight, curve.m_Bezier.d.z - centroid.z)
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
