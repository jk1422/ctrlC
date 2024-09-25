using Colossal.Serialization.Entities;
using Game.Common;
using Game.Objects;
using Game.Prefabs;
using Game.Rendering;
using Game.UI.Widgets;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ctrlC.Components
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
    public struct CtrlCObject : IComponentData
    {
        public int m_Priority;//
    }

    [ComponentMenu("Objects/", new Type[] { })]
    public class CtrlCStampPrefab : ObjectPrefab
    {
        [InputField]
        [Range(1f, 1000f)]
        public int m_Width = 1;

        [InputField]
        [Range(1f, 1000f)]
        public int m_Depth = 1;

        public uint m_ConstructionCost;

        public uint m_UpKeepCost;

        public override bool canIgnoreUnlockDependencies => false;

        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {
            base.GetPrefabComponents(components);
            components.Add(ComponentType.ReadWrite<ObjectGeometryData>());
            components.Add(ComponentType.ReadWrite<CtrlCStampData>());
            components.Add(ComponentType.ReadWrite<CtrlCSubBuildings>());
        }

        public override void GetArchetypeComponents(HashSet<ComponentType> components)
        {
            base.GetArchetypeComponents(components);
            components.Add(ComponentType.ReadWrite<Static>());
            components.Add(ComponentType.ReadWrite<CtrlCStamp>());
            components.Add(ComponentType.ReadWrite<CullingInfo>());
            components.Add(ComponentType.ReadWrite<PseudoRandomSeed>());
            components.Add(ComponentType.ReadWrite<CtrlCSubBuildings>());
        }
    }
    public struct CtrlCStampData : IComponentData, IQueryTypeParameter
    {
        public int2 m_Size;

        public uint m_ConstructionCost;

        public uint m_UpKeepCost;
    }
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct CtrlCStamp : IComponentData, IQueryTypeParameter, IEmptySerializable
    {
    }


    [ComponentMenu("Objects/", new Type[] { typeof(CtrlCStampPrefab) })]
    public class CtrlCSubBuildings : ComponentBase
    {
        public CtrlCSubBuildingsInfo[] m_SubObjects;

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
    public class CtrlCSubBuildingsInfo
    {
        public ObjectPrefab m_Object;

        public float3 m_Position;

        public quaternion m_Rotation;

        public ushort seed;
    }
    [InternalBufferCapacity(0)]
    public struct SubB : IBufferElementData, IEquatable<SubB>, IEmptySerializable
    {
        public Entity m_SubObject;  // Justera fälten som krävs för SubB
        public float3 m_Position;
        public quaternion m_Rotation;
        public uint seed;

        public SubB(Entity subObject, float3 position, quaternion rotation, uint seed)
        {
            m_SubObject = subObject;
            m_Position = position;
            m_Rotation = rotation;
            this.seed = seed;
        }

        public bool Equals(SubB other)
        {
            return m_SubObject.Equals(other.m_SubObject);
        }

        public override int GetHashCode()
        {
            return m_SubObject.GetHashCode();
        }
    }
    [InternalBufferCapacity(0)]
    public struct SubBuilding : IBufferElementData, IEquatable<SubBuilding>, IEmptySerializable
    {
        public Entity m_SubObject;

        public SubBuilding(Entity subObject)
        {
            m_SubObject = subObject;
        }

        public bool Equals(SubBuilding other)
        {
            return m_SubObject.Equals(other.m_SubObject);
        }

        public override int GetHashCode()
        {
            return m_SubObject.GetHashCode();
        }
    }
}
