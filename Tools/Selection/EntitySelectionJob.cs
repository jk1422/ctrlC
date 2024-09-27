using Game.Buildings;
using Game.Net;
using Game.Objects;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace ctrlC.Tools.Selection
{
	[BurstCompile]
	public struct EntitySelectionJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<Entity> selectables;
		[ReadOnly] public float3 center;
		[ReadOnly] public float radiusSquared;
		[ReadOnly] public EntityManager entityManager;

		[NativeDisableParallelForRestriction] public NativeQueue<Entity>.ParallelWriter roadsQueue;
		[NativeDisableParallelForRestriction] public NativeQueue<Entity>.ParallelWriter buildingsQueue;
		[NativeDisableParallelForRestriction] public NativeQueue<Entity>.ParallelWriter treesQueue;
		[NativeDisableParallelForRestriction] public NativeQueue<Entity>.ParallelWriter propsQueue;
		[NativeDisableParallelForRestriction] public NativeQueue<Entity>.ParallelWriter areasQueue;

		public void Execute(int index)
		{
			var entity = selectables[index];
			bool isInRadius = false;

			if (entityManager.HasComponent<Game.Objects.Transform>(entity))
			{
				var transform = entityManager.GetComponentData<Game.Objects.Transform>(entity);
				float3 position = transform.m_Position;
				float distanceSquared = math.lengthsq(position - center);
				isInRadius = distanceSquared <= radiusSquared;
			}
			else if (entityManager.HasComponent<Curve>(entity))
			{
				var bezier = entityManager.GetComponentData<Curve>(entity).m_Bezier;
				isInRadius = math.lengthsq(bezier.a - center) <= radiusSquared ||
							 math.lengthsq(bezier.b - center) <= radiusSquared ||
							 math.lengthsq(bezier.c - center) <= radiusSquared ||
							 math.lengthsq(bezier.d - center) <= radiusSquared;
			}
			else if (entityManager.HasComponent<Game.Areas.Node>(entity))
			{
				var nodesBuffer = entityManager.GetBuffer<Game.Areas.Node>(entity);
				for (int j = 0; j < nodesBuffer.Length; j++)
				{
					if (math.lengthsq(nodesBuffer[j].m_Position - center) <= radiusSquared)
					{
						isInRadius = true;
						break;
					}
				}
			}

			if (isInRadius)
			{
				if (entityManager.HasComponent<Curve>(entity))
				{
					roadsQueue.Enqueue(entity);
				}
				else if (entityManager.HasComponent<Building>(entity))
				{
					buildingsQueue.Enqueue(entity);
				}
				else if (entityManager.HasComponent<Plant>(entity))
				{
					treesQueue.Enqueue(entity);
				}
				else if (entityManager.HasComponent<Game.Objects.Object>(entity))
				{
					propsQueue.Enqueue(entity);
				}else if (entityManager.HasComponent<Game.Areas.Area>(entity))
				{
					areasQueue.Enqueue(entity);
				}
			}
		}
	}
}
