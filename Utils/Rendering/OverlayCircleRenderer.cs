using Game.Rendering;
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
            dependencies.Complete();
            // Idle circle
            Entities.WithAll<CircleIdle>().ForEach((ref CircleIdle circleIdle) =>
            {
                DrawCircle(ref buffer, circleIdle.center, circleIdle.radius * 2, idleColor);
            }).WithoutBurst().Run();

            // Draw the selection circle
            Entities.WithAll<CircleOverlay>().ForEach((ref CircleOverlay circleOverlay) =>
            {
                DrawCircle(ref buffer, circleOverlay.center, circleOverlay.radius * 2, selectColor);
            }).WithoutBurst().Run();

            // Draw the deselection circle
            Entities.WithAll<DeselectCircleOverlay>().ForEach((ref DeselectCircleOverlay deselectCircleOverlay) =>
            {
                DrawCircle(ref buffer, deselectCircleOverlay.center, deselectCircleOverlay.radius * 2, deSelectColor);
            }).WithoutBurst().Run();

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