using Colossal.Mathematics;
using Colossal.Serialization.Entities;
using Game.Common;
using Game.Net;
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
using UnityEngine.Assertions.Must;

namespace ctrlC.Components
{




   







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
