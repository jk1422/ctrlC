using Colossal.Logging;
using ctrlC.Components;
using Game.Net;
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
    // Before my little break I was rebuilding the CopySystem from scratch. The idea was to split each type of entity for better and cleaner structure, 
    // and to be able to make custom copy-logic for each different type, such as the trees age or the buildings seed etc. 
    //
    // However, since my little break made me forget where I was, I decided to quickly assemble the Copy System to just make it work. 
    // So the code is pretty dirty and there is some changes needed here in order to make it work as intended.

    internal static partial class CopySystem
    {
		public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(CopySystem)}").SetShowsErrorsInUI(false);
		private static float baseHeight = float.MaxValue;
		internal static float3 centroid = float3.zero; 
		private static int pointCount = 0;
		private static EntityManager _entityManager;

		private static List<ObjectSubObjectInfo> subObjectInfos = new List<ObjectSubObjectInfo>();
		internal static List<CtrlCSubBuildingsInfo> subBuildingsInfos = new List<CtrlCSubBuildingsInfo>();
		public static CtrlCStampPrefab CopyItems(EntityManager entityManager, PrefabSystem prefabSystem, List<Entity> buildings, List<Entity> roads, List<Entity> props, List<Entity> trees)
		{

			log.Info($"Copying stuff");
			
			InitializeVariables();
			CtrlCStampPrefab assetStamp = CreateAssetStampPrefab();
			_entityManager = entityManager;
			List<Entity> items = new List<Entity>();	
			if(roads.Count > 0) { items.AddRange(roads); }
			if(buildings.Count > 0) { items.AddRange(buildings); }
			if(trees.Count > 0) { items.AddRange(trees); }
			if(props.Count > 0) { items.AddRange(props); }

			ComputeBaseHeightAndCentroid(items, _entityManager);

            CtrlCSubBuildings ctrlCSubBuildings = ScriptableObject.CreateInstance<CtrlCSubBuildings>();
			subBuildingsInfos = new List<CtrlCSubBuildingsInfo>();

            // Initialize ObjectSubObjects and ObjectSubNetsaa
            ObjectSubObjects objectSubObjects = ScriptableObject.CreateInstance<ObjectSubObjects>();
			ObjectSubNets objectSubNets = ScriptableObject.CreateInstance<ObjectSubNets>();

			objectSubNets.m_InvertWhen = NetInvertMode.LefthandTraffic;

			subObjectInfos = new List<ObjectSubObjectInfo>();
			List<ObjectSubNetInfo> subNetInfos = new List<ObjectSubNetInfo>();

			Dictionary<Node, int> nodeIndexMapping = new Dictionary<Node, int>();
            int currentIndex = 0;
            // Map nodes to indices
            MapNodesToIndices(roads, entityManager, nodeIndexMapping, ref currentIndex);
            if (roads.Count > 0)
            {
                ProcessEdgesAndCurves(roads, entityManager, prefabSystem, subNetInfos, nodeIndexMapping);
            }
            if (buildings.Count > 0) { CopyBuildings(buildings, prefabSystem); }
			if(props.Count > 0)
			{
				CopyProps(props, entityManager, prefabSystem, subObjectInfos);

            }
			if(trees.Count > 0)
			{
				CopyTrees(trees, entityManager, prefabSystem, subObjectInfos);
			}
			ctrlCSubBuildings.m_SubObjects = subBuildingsInfos.ToArray();
			assetStamp.components.Add(ctrlCSubBuildings);
            objectSubObjects.m_SubObjects = subObjectInfos.ToArray();
			assetStamp.components.Add(objectSubObjects);

            objectSubNets.m_SubNets = subNetInfos.ToArray();
            assetStamp.components.Add(objectSubNets);
            // Add the duplicate prefab to the prefab system
            AddPrefabToSystem(prefabSystem, assetStamp);
			return assetStamp;
		}
		private static void InitializeVariables()
		{
			baseHeight = float.MaxValue;
			centroid = float3.zero;
			pointCount = 0;
			subObjectInfos = new List<ObjectSubObjectInfo>();
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

	}
}
