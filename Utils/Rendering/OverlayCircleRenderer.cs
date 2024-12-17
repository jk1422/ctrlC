using Colossal.Entities;
using Game.Rendering;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace ctrlC.Rendering
{
    public partial class OverlayCircleRenderer : SystemBase
    {
        private OverlayRenderSystem _overlayRenderSystem;
        private EntityQuery _circleQuery;
        private EntityQuery _deselectCircleQuery;
        private EntityQuery _idleCircleQuery;
        private Entity overlayEntity;
        private Color idleColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0.1f);
        private Color selectColor = new Color(139f / 255f, 219f / 255f, 70f / 255f, 0.1f);
        private Color deSelectColor = new Color(255f / 255f, 0f / 255f, 0f / 255f, 0.1f);

        protected override void OnCreate()
        {
            base.OnCreate();
            _overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();

            _circleQuery = GetEntityQuery(ComponentType.ReadOnly<CircleOverlay>());
            _deselectCircleQuery = GetEntityQuery(ComponentType.ReadOnly<DeselectCircleOverlay>());
            _idleCircleQuery = GetEntityQuery(ComponentType.ReadOnly<CircleIdle>());

            // Create an entity for the overlay buffer
            overlayEntity = EntityManager.CreateEntity();
            EntityManager.AddBuffer<OverlayBufferElement>(overlayEntity);
        }

        protected override void OnUpdate()
        {
            // Få buffer och beroenden från overlayRenderSystem
            var buffer = _overlayRenderSystem.GetBuffer(out JobHandle dependencies);

            // Idle circle
            var circleOverlayDesc = new EntityQueryDesc()
            {
                Any = new ComponentType[] { typeof(CircleIdle), typeof(CircleOverlay), typeof(DeselectCircleOverlay) }
            };
            var overlayArray = GetEntityQuery(circleOverlayDesc).ToEntityArray(allocator: Allocator.Temp);

            foreach (var circleEntity in overlayArray)
            {
                if (EntityManager.TryGetComponent<CircleIdle>(circleEntity, out CircleIdle component1))
                {
                    buffer.DrawCircle(idleColor, component1.center, component1.radius * 2);
                }
                else if (EntityManager.TryGetComponent<CircleOverlay>(circleEntity, out CircleOverlay component2))
                {
                    buffer.DrawCircle(selectColor, component2.center, component2.radius * 2);
                }else if (EntityManager.TryGetComponent<DeselectCircleOverlay>(circleEntity, out DeselectCircleOverlay component3))
                {
                    buffer.DrawCircle(deSelectColor, component3.center, component3.radius * 2);
                }
            }

            // Lägg till buffer till render systemet
            _overlayRenderSystem.AddBufferWriter(dependencies);
        }

        private void DrawCircle(ref OverlayRenderSystem.Buffer buffer, Vector3 center, float radius, Color color)
        {
            buffer.DrawCircle(color, center, radius);
        }
    }

    public struct CircleOverlay : IComponentData
    {
        public Vector3 center;
        public float radius;
    }

    public struct DeselectCircleOverlay : IComponentData
    {
        public Vector3 center;
        public float radius;
    }

    public struct CircleIdle : IComponentData
    {
        public Vector3 center;
        public float radius;
    }

    public struct OverlayBufferElement : IBufferElementData
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
    }
}