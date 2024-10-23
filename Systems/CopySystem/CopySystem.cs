using Colossal.Logging;
using ctrlC.Components.Prefabs;
using Game.Prefabs;
using System.Collections.Generic;
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

    // Also in order to optimize this code, I will 'burstify' it when I have the time

    internal static partial class CopySystem
    {
        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(CopySystem)}").SetShowsErrorsInUI(false);
        private static float baseHeight = float.MaxValue;
        internal static float3 centroid = float3.zero;
        private static int pointCount = 0;
        private static EntityManager _entityManager;
        private static PrefabSystem _prefabSystem;

        private static List<ObjectSubObjectInfo> subObjectInfos = new List<ObjectSubObjectInfo>();
        public static AssetStampPrefab CopyItems(EntityManager entityManager, PrefabSystem prefabSystem, List<Entity> buildings, List<Entity> roads, List<Entity> props, List<Entity> trees, List<Entity> areas)
        {
            log.Info($"Will copy: {buildings.Count} buildings,{roads.Count} roads, {props.Count} props, {trees.Count} trees and more.");
            InitializeVariables(prefabSystem);
            AssetStampPrefab assetStamp = CreateAssetStampPrefab();
            _entityManager = entityManager;
            List<Entity> items = new List<Entity>();
            if (roads.Count > 0) { items.AddRange(roads); }
            if (buildings.Count > 0) { items.AddRange(buildings); }
            if (trees.Count > 0) { items.AddRange(trees); }
            if (props.Count > 0) { items.AddRange(props); }

            ComputeBaseHeightAndCentroid(items, _entityManager);

            // Initialize ObjectSubObjects and ObjectSubNetsaa
            ObjectSubObjects objectSubObjects = ScriptableObject.CreateInstance<ObjectSubObjects>();
            ObjectSubNets objectSubNets = ScriptableObject.CreateInstance<ObjectSubNets>();
            ObjectSubAreas objectSubAreas = ScriptableObject.CreateInstance<ObjectSubAreas>();

            CtrlCSubAreas ctrlCSubAreas = ScriptableObject.CreateInstance<CtrlCSubAreas>();
            List<CtrlCSubAreaInfo> ctrlCSubAreaInfos = new List<CtrlCSubAreaInfo>();

            objectSubNets.m_InvertWhen = NetInvertMode.LefthandTraffic;

            subObjectInfos = new List<ObjectSubObjectInfo>();
            List<ObjectSubNetInfo> subNetInfos = new List<ObjectSubNetInfo>();
            List<ObjectSubAreaInfo> objectSubAreaInfos = new List<ObjectSubAreaInfo>();

            Dictionary<Entity, int> nodeIndexMapping = new Dictionary<Entity, int>();
            int currentIndex = 0;

            if (buildings.Count > 0)
            {
                CopyBuildings(buildings, prefabSystem, roads, objectSubAreaInfos);
            }

            // Map nodes to indices
            MapNodesToIndices(roads, entityManager, nodeIndexMapping, ref currentIndex);
            if (roads.Count > 0)
            {
                ProcessEdgesAndCurves(roads, entityManager, prefabSystem, subNetInfos, nodeIndexMapping);
            }

            if (props.Count > 0)
            {
                CopyProps(props, entityManager, prefabSystem, subObjectInfos);

            }
            if (trees.Count > 0)
            {
                CopyTrees(trees, entityManager, prefabSystem, subObjectInfos);
            }
            if (areas.Count > 0)
            {
                CopyAreas(areas, entityManager, prefabSystem, objectSubAreaInfos);
            }

            objectSubObjects.m_SubObjects = subObjectInfos.ToArray();
            assetStamp.components.Add(objectSubObjects);

            objectSubNets.m_SubNets = subNetInfos.ToArray();
            assetStamp.components.Add(objectSubNets);


            objectSubAreas.m_SubAreas = objectSubAreaInfos.ToArray();
            assetStamp.components.Add(objectSubAreas);

            ctrlCSubAreas.m_SubAreas = ctrlCSubAreaInfos.ToArray();
            assetStamp.components.Add(ctrlCSubAreas);


            // Add the duplicate prefab to the prefab system
            AddPrefabToSystem(prefabSystem, assetStamp);
            return assetStamp;
        }
        private static void InitializeVariables(PrefabSystem ps)
        {
            baseHeight = float.MaxValue;
            centroid = float3.zero;
            pointCount = 0;
            subObjectInfos = new List<ObjectSubObjectInfo>();
            _prefabSystem = ps;
        }
        private static AssetStampPrefab CreateAssetStampPrefab()
        {
            AssetStampPrefab assetStamp = new AssetStampPrefab
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
