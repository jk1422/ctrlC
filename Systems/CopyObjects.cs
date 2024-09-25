using Colossal.Logging;
using Colossal.Mathematics;
using Colossal.Entities;
using Game.Net;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Game.Common;
using ctrlC.Components;

namespace ctrlC.Systems
{
	public class CopyObjects
	{
		public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(CopyObjects)}").SetShowsErrorsInUI(false);

		private static float baseHeight = float.MaxValue;
		private static float3 centroid = float3.zero;
		private static int pointCount = 0;
		public UIObjectData uiObjectData;
		private static EntityManager _entityManager;

		public static CtrlCStampPrefab CreatePrefab(List<Entity> entities, PrefabSystem prefabSystem, EntityManager entityManager)
		{
			InitializeVariables();
			_entityManager = entityManager;
			// Compute baseHeight and centroid
			ComputeBaseHeightAndCentroid(entities, _entityManager);

			// Duplicate AssetStampPrefab
			CtrlCStampPrefab assetStamp = CreateAssetStampPrefab();
			
			// Initialize ObjectSubObjects and ObjectSubNetsaa
			ObjectSubObjects objectSubObjects = ScriptableObject.CreateInstance<ObjectSubObjects>();
			ObjectSubNets objectSubNets = ScriptableObject.CreateInstance<ObjectSubNets>();

			List<ObjectSubObjectInfo> subObjectInfos = new List<ObjectSubObjectInfo>();
			objectSubNets.m_InvertWhen = NetInvertMode.LefthandTraffic;
			List<ObjectSubNetInfo> subNetInfos = new List<ObjectSubNetInfo>();
			
			Dictionary<Node, int> nodeIndexMapping = new Dictionary<Node, int>();
			int currentIndex = 0;



			// Map nodes to indices
			MapNodesToIndices(entities, entityManager, nodeIndexMapping, ref currentIndex);

			// Process entities for duplication
			ProcessEntitiesForDuplication(entities, entityManager, prefabSystem, subObjectInfos, nodeIndexMapping);
			
			// Add ObjectSubObjects to the duplicate prefab
			objectSubObjects.m_SubObjects = subObjectInfos.ToArray();
			assetStamp.components.Add(objectSubObjects);

			// Process edges and curves
			ProcessEdgesAndCurves(entities, entityManager, prefabSystem, subNetInfos, nodeIndexMapping);

			// Add ObjectSubNets to the components list of the duplicate prefab
			objectSubNets.m_SubNets = subNetInfos.ToArray();
			assetStamp.components.Add(objectSubNets);

			// Process surface entities
			ProcessSurfaceEntities(entities, entityManager, prefabSystem, assetStamp);

			// Add the duplicate prefab to the prefab system
			AddPrefabToSystem(prefabSystem, assetStamp);

			
			return assetStamp;
		}
		private void GetSeed(Entity entity)
		{

			if (_entityManager.TryGetComponent<PseudoRandomSeed>(entity, out PseudoRandomSeed seedComponent))
			{
				var seed = seedComponent.m_Seed;

			}
		}


		private static void InitializeVariables()
		{
			baseHeight = float.MaxValue;
			centroid = float3.zero;
			pointCount = 0;
		}

		private static void ComputeBaseHeightAndCentroid(List<Entity> entities, EntityManager entityManager)
		{
			foreach (var selected in entities)
			{
				if (entityManager.HasComponent<Edge>(selected) || entityManager.HasComponent<Curve>(selected))
				{
					var curve = entityManager.GetComponentData<Curve>(selected);
					if (entityManager.HasComponent<Game.Net.Elevation>(selected))
					{
						var elevation = entityManager.GetComponentData<Game.Net.Elevation>(selected).m_Elevation;
						if (elevation.x < 0 || elevation.y < 0)
							continue;
					}

					baseHeight = math.min(baseHeight, curve.m_Bezier.a.y);
					baseHeight = math.min(baseHeight, curve.m_Bezier.b.y);
					baseHeight = math.min(baseHeight, curve.m_Bezier.c.y);
					baseHeight = math.min(baseHeight, curve.m_Bezier.d.y);

					centroid += curve.m_Bezier.a + curve.m_Bezier.b + curve.m_Bezier.c + curve.m_Bezier.d;
					pointCount += 4;
				}
				else if (entityManager.HasComponent<Game.Objects.Transform>(selected))
				{
					centroid += entityManager.GetComponentData<Game.Objects.Transform>(selected).m_Position;
					pointCount++;
				}
				else if (entityManager.HasComponent<Game.Areas.Node>(selected))
				{
					var nodesBuffer = entityManager.GetBuffer<Game.Areas.Node>(selected);
					foreach (var node in nodesBuffer)
					{
						centroid += node.m_Position;
						pointCount++;
					}
				}
			}

			if (pointCount > 0)
				centroid /= pointCount;
		}

		private static CtrlCStampPrefab CreateAssetStampPrefab()
		{
			CtrlCStampPrefab assetStamp = new CtrlCStampPrefab
			{
				name = "CopiedSelection",
				m_Width = 1,
				m_Depth = 1,
				m_ConstructionCost = 0,
				m_UpKeepCost = 250,
				isDirty = true,
				active = true,
				components = new List<ComponentBase>()
			};
			return assetStamp;
		}

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

		private static void ProcessEntitiesForDuplication(List<Entity> entities, EntityManager entityManager, PrefabSystem prefabSystem, List<ObjectSubObjectInfo> subObjectInfos, Dictionary<Node, int> nodeIndexMapping)
		{
			foreach (var selectedEntity in entities)
			{
				if (entityManager.HasComponent<Curve>(selectedEntity) || entityManager.HasComponent<NetData>(selectedEntity) ||
					entityManager.HasComponent<Road>(selectedEntity) || entityManager.HasComponent<Node>(selectedEntity))
				{
					continue;
				}

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
		private static void ProcessSurfaceEntities(List<Entity> entities, EntityManager entityManager, PrefabSystem prefabSystem, CtrlCStampPrefab assetStamp)
		{
			ObjectSubAreas objectSubAreas = ScriptableObject.CreateInstance<ObjectSubAreas>();
			List<ObjectSubAreaInfo> objectSubAreaInfos = new List<ObjectSubAreaInfo>();

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

			objectSubAreas.m_SubAreas = objectSubAreaInfos.ToArray();
			assetStamp.components.Add(objectSubAreas);
		}

		private static void AddPrefabToSystem(PrefabSystem prefabSystem, CtrlCStampPrefab assetStamp)
		{
			try
			{
				prefabSystem.AddPrefab(assetStamp);
				prefabSystem.Update();
			}
			catch (Exception ex)
			{
				log.Error($"Error when trying to add Prefab: {ex.Message}");
				throw;
			}
		}
	}
}