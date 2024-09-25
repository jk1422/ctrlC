using ctrlC.Utils;
using Game.Net;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ctrlC.Tools.Selection
{
	public partial class SelectionTool
	{
		internal void entitiesWithinSelection(Vector3 center, float radius)
		{
			NativeArray<Entity> selectables = default;
		

			NativeQueue<Entity> roadsQueue = default;
			NativeQueue<Entity> buildingsQueue = default;
			NativeQueue<Entity> treesQueue = default;
			NativeQueue<Entity> propsQueue = default;







			try
			{
				selectables = GetAllSelectebles().ToEntityArray(Allocator.TempJob);
				roadsQueue = new NativeQueue<Entity>(Allocator.TempJob);
				buildingsQueue = new NativeQueue<Entity>(Allocator.TempJob);
				treesQueue = new NativeQueue<Entity>(Allocator.TempJob);
				propsQueue = new NativeQueue<Entity>(Allocator.TempJob);


				float radiusSquared = radius * radius;
				var job = new EntitySelectionJob
				{
					selectables = selectables,
					center = center,
					radiusSquared = radiusSquared,
					entityManager = entityManager,
					roadsQueue = roadsQueue.AsParallelWriter(),
					buildingsQueue = buildingsQueue.AsParallelWriter(),
					treesQueue = treesQueue.AsParallelWriter(),
					propsQueue = propsQueue.AsParallelWriter()
				};

				JobHandle handle = job.Schedule(selectables.Length, 64);
				handle.Complete();


				// Collect the entities from the queues into their respective lists
				while (roadsQueue.TryDequeue(out Entity entity))
				{
					
					SelectedRoads.Add(entity);
					entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.AddHighlight);
				}

				while (buildingsQueue.TryDequeue(out Entity entity))
				{
					
					SelectedBuildings.Add(entity);
					entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.AddHighlight);
				}

				while (treesQueue.TryDequeue(out Entity entity))
				{
					
					SelectedTrees.Add(entity);
					entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.AddHighlight);
				}

				while (propsQueue.TryDequeue(out Entity entity))
				{
					
					SelectedProps.Add(entity);
					entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.AddHighlight);
				}

				
			}
			finally
			{
				if (selectables.IsCreated)
				{
					selectables.Dispose();
				}
				roadsQueue.Dispose();
				buildingsQueue.Dispose();
				treesQueue.Dispose();
				propsQueue.Dispose();
			}
		}
	}
}
