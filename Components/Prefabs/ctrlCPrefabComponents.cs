using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using ctrlC.Components.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace ctrlC.Components.Prefabs
{
    public class CtrlCPrefabComponent : ComponentBase
    {
        public string c_name;
        public string c_description;
        public string c_imagePath;
        public int c_category;
        public string c_id = Guid.NewGuid().ToString();

        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {

            components.Add(ComponentType.ReadWrite<CtrlCObject>());

        }

        public override void GetArchetypeComponents(HashSet<ComponentType> components)
        {
        }
    }


    [ComponentMenu("Objects/", new Type[] { })]
    public class CtrlCBuildings : ComponentBase
    {
        public CtrlCBuildingsInfo[] m_SubObjects;


        public override bool ignoreUnlockDependencies => true;

        public override void GetDependencies(List<PrefabBase> prefabs)
        {
            base.GetDependencies(prefabs);
            for (int i = 0; i < m_SubObjects.Length; i++)
            {
                prefabs.Add(m_SubObjects[i].m_Object);
            }
        }

        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {
            components.Add(ComponentType.ReadWrite<SubB>());
        }

        public override void GetArchetypeComponents(HashSet<ComponentType> components)
        {
            components.Add(ComponentType.ReadWrite<SubBuilding>());
        }
    }
    [Serializable]
    public class CtrlCBuildingsInfo
    {
        public ObjectPrefab m_Object;

        public float3 m_Position;

        public quaternion m_Rotation;

        public ushort seed;

        [Range(0f, 100f)]
        public int m_Probability = 100;

        public int m_ID;
    }

    public class CtrlCSubAreas : ComponentBase
    {
        public CtrlCSubAreaInfo[] m_SubAreas;

        public override bool ignoreUnlockDependencies => true;

        public override void GetDependencies(List<PrefabBase> prefabs)
        {
            base.GetDependencies(prefabs);
            if (m_SubAreas != null)
            {
                for (int i = 0; i < m_SubAreas.Length; i++)
                {
                    prefabs.Add(m_SubAreas[i].m_AreaPrefab);
                }
            }
        }

        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {
            components.Add(ComponentType.ReadWrite<SubArea>());
            components.Add(ComponentType.ReadWrite<SubAreaNode>());
        }

        public override void GetArchetypeComponents(HashSet<ComponentType> components)
        {
            components.Add(ComponentType.ReadWrite<Game.Areas.SubArea>());
        }
    }

    [Serializable]
    public class CtrlCSubAreaInfo
    {
        public AreaPrefab m_AreaPrefab;

        public float3[] m_NodePositions;

        public int[] m_ParentMeshes;

        public int m_ParentID;
    }
}
