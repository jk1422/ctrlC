using ctrlC.Utils;
using Game.Net;
using System;
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
        // Helper method to destroy an entity safely
        private void DestroyEntity(Entity entity)
        {
            if (EntityManager.Exists(entity))
            {
                EntityManager.DestroyEntity(entity);
            }
        }

        // Helper method to clear all selection lists
        private void ClearSelectionLists()
        {
            SelectedBuildings.Clear();
            SelectedProps.Clear();
            SelectedRoads.Clear();
            SelectedTrees.Clear();
            SelectedAreas.Clear();
        }

        // Helper method to reset input actions to null
        private void ResetInputActions()
        {
            _copyAction = null;
            _altModifier = null;
            _ApplyAction = null;
            _SecondaryApplyAction = null;
        }

        // Helper method to reset all state variables to their default values
        private void ResetStateVariables()
        {
            isSelecting = false;
            isDeselecting = false;
            circleCenter = Vector3.zero;
            circleRadius = 0f;
            deselectCircleCenter = Vector3.zero;
            deselectCircleRadius = 0f;
            lastSelectedEntity = Entity.Null;
            HoveredEntity = Entity.Null;
            LastPos = float3.zero;
            prev = Entity.Null;
            rayDefaultMode = true;
            activeFilters = SelectableFilters.None;
        }

        // Helper method to reset system references and queries
        private void ResetSystemReferences()
        {
            _ModUISystem = null;
            _PlacementTool = null;
            _ToolRaycastSystem = null;
            selectablesQuery = default;
            circleEntity = Entity.Null;
            deselectCircleEntity = Entity.Null;
            idleCircleEntity = Entity.Null;
        }

        // Helper method to dequeue entities and add them to the selection list
        private void DequeueEntitiesToSelection(NativeQueue<Entity> queue, List<Entity> selectionList)
        {
            while (queue.TryDequeue(out Entity entity))
            {
                if (selectionList.Contains(entity)) continue;
                else
                {
                    selectionList.Add(entity);
                    entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.AddHighlight);
                }
            }
        }

        // Helper method to safely dispose of a NativeContainer if it was created
        private void DisposeIfCreated<T>(ref T nativeContainer) where T : struct, IDisposable
        {
            if (nativeContainer is NativeArray<Entity> array && array.IsCreated)
            {
                array.Dispose();
            }
            else if (nativeContainer is NativeQueue<Entity> queue)
            {
                queue.Dispose();
            }
        }

        /// <summary>
        /// Selects all entities within a given circular area based on the specified center and radius.
        /// Adds selected entities to their respective lists (roads, buildings, trees, props, areas) 
        /// and highlights them in the game world.
        /// </summary>
        /// <param name="center">The center of the selection circle in world coordinates.</param>
        /// <param name="radius">The radius of the selection circle.</param>
        /// /// <remarks>
        /// This method uses a parallel job to efficiently filter and categorize the entities.
        /// Ensure that the selection lists are cleared before calling this method to avoid duplicates.
        /// </remarks>
        internal void EntitiesWithinSelection(Vector3 center, float radius)
        {
            // Initialize NativeArrays and NativeQueues with default values
            NativeArray<Entity> selectables = default;
            NativeQueue<Entity> roadsQueue = default;
            NativeQueue<Entity> buildingsQueue = default;
            NativeQueue<Entity> treesQueue = default;
            NativeQueue<Entity> propsQueue = default;
            NativeQueue<Entity> areasQueue = default;

            try
            {
                // Retrieve all selectable entities
                selectables = GetAllSelectebles().ToEntityArray(Allocator.TempJob);

                // Create NativeQueues for different types of entities
                roadsQueue = new NativeQueue<Entity>(Allocator.TempJob);
                buildingsQueue = new NativeQueue<Entity>(Allocator.TempJob);
                treesQueue = new NativeQueue<Entity>(Allocator.TempJob);
                propsQueue = new NativeQueue<Entity>(Allocator.TempJob);
                areasQueue = new NativeQueue<Entity>(Allocator.TempJob);

                // Calculate the square of the radius to avoid recalculating it in the job
                float radiusSquared = radius * radius;

                // Create and schedule the entity selection job
                var job = new EntitySelectionJob
                {
                    selectables = selectables,
                    center = center,
                    radiusSquared = radiusSquared,
                    entityManager = entityManager,
                    roadsQueue = roadsQueue.AsParallelWriter(),
                    buildingsQueue = buildingsQueue.AsParallelWriter(),
                    treesQueue = treesQueue.AsParallelWriter(),
                    propsQueue = propsQueue.AsParallelWriter(),
                    areasQueue = areasQueue.AsParallelWriter()
                };

                // Schedule and complete the job
                JobHandle handle = job.Schedule(selectables.Length, 64);
                handle.Complete();

                // Collect the entities from the queues into their respective lists
                DequeueEntitiesToSelection(roadsQueue, SelectedRoads);
                DequeueEntitiesToSelection(buildingsQueue, SelectedBuildings);
                DequeueEntitiesToSelection(treesQueue, SelectedTrees);
                DequeueEntitiesToSelection(propsQueue, SelectedProps);
                DequeueEntitiesToSelection(areasQueue, SelectedAreas);
            }
            finally
            {
                // Dispose of all native containers if they were created
                DisposeIfCreated(ref selectables);
                DisposeIfCreated(ref roadsQueue);
                DisposeIfCreated(ref buildingsQueue);
                DisposeIfCreated(ref treesQueue);
                DisposeIfCreated(ref propsQueue);
                DisposeIfCreated(ref areasQueue);
            }
        }
    }
}
