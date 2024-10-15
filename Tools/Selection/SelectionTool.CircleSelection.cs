using UnityEngine;
using Unity.Entities;
using ctrlC.Rendering;
using ctrlC.Utils;
using Game.Net;
using System.Collections.Generic;
using Unity.Collections;

namespace ctrlC.Tools.Selection
{
    public partial class SelectionTool
    {
        /// <summary>
        /// Updates the circle selection and deselection based on user input.
        /// Handles starting, updating, finalizing, and aborting both selection and deselection.
        /// </summary>
        private void UpdateCircleSelection()
        {
            if (_altModifier.IsPressed() == standardToolMode || isSelecting || isDeselecting)
            {
                // Update circle position if mouse is moved
                UpdateCircleIdle();

                // Handle circle selection input
                if (_ApplyAction.WasPressedThisFrame() && !isDeselecting)
                {
                    StartCircleSelection();
                }
                else if (isSelecting)
                {
                    if (_ApplyAction.IsPressed())
                    {
                        UpdateCircleSelectionRadius();
                    }
                    else if (_ApplyAction.WasReleasedThisFrame())
                    {
                        FinalizeCircleSelection();
                    }
                    else if (_SecondaryApplyAction.WasPressedThisFrame())
                    {
                        AbortCircleSelection();
                    }
                }

                // Handle circle deselection input
                if (_altModifier.IsPressed() == standardToolMode && _SecondaryApplyAction.WasPressedThisFrame() && !isSelecting)
                {
                    StartCircleDeselection();
                }
                else if (isDeselecting)
                {
                    if (_SecondaryApplyAction.IsPressed())
                    {
                        UpdateCircleDeselectionRadius();
                    }
                    else if (_SecondaryApplyAction.WasReleasedThisFrame())
                    {
                        FinalizeCircleDeselection();
                    }
                    else if (_ApplyAction.WasPressedThisFrame())
                    {
                        AbortCircleDeselection();
                    }
                }
            }
        }

        // Updates the position of the idle circle indicator based on mouse position.
        private void UpdateCircleIdle()
        {
            if (TryGetMouseWorldPosition(out Vector3 position) && !_ApplyAction.IsPressed() && !_SecondaryApplyAction.IsPressed())
            {
                if (!EntityManager.HasComponent<CircleIdle>(circleEntity))
                {
                    EntityManager.AddComponentData(circleEntity, new CircleIdle { center = position, radius = 2f });
                }
                else
                {
                    EntityManager.SetComponentData(circleEntity, new CircleIdle { center = position, radius = 2f });
                }
            }
            else if (EntityManager.HasComponent<CircleIdle>(circleEntity))
            {
                EntityManager.RemoveComponent<CircleIdle>(circleEntity);
            }
        }

        // Starts the circle selection process by initializing the selection center and radius.
        private void StartCircleSelection()
        {
            if (TryGetMouseWorldPosition(out Vector3 pos))
            {
                isSelecting = true;
                circleCenter = pos;
                circleRadius = 0; // Initial radius, will be updated as the user drags the mouse
                if (!EntityManager.HasComponent<CircleOverlay>(circleEntity))
                {
                    EntityManager.AddComponentData(circleEntity, new CircleOverlay { center = circleCenter, radius = circleRadius });
                }
            }
        }

        // Updates the radius of the circle selection based on the mouse position.
        private void UpdateCircleSelectionRadius()
        {
            if (TryGetMouseWorldPosition(out Vector3 currentMousePosition))
            {
                circleRadius = Vector3.Distance(circleCenter, currentMousePosition);
                EntityManager.SetComponentData(circleEntity, new CircleOverlay { center = circleCenter, radius = circleRadius });
            }
        }

        // Finalizes the circle selection and selects entities within the circle.
        private void FinalizeCircleSelection()
        {
            activeFilters = GetActiveFilters();
            SelectEntitiesWithinCircle(circleCenter, circleRadius);
            activeFilters = SelectableFilters.None;
            EntityManager.RemoveComponent<CircleOverlay>(circleEntity);
            isSelecting = false;
        }

        // Aborts the circle selection process and removes the circle overlay.
        private void AbortCircleSelection()
        {
            isSelecting = false;
            if (EntityManager.HasComponent<CircleOverlay>(circleEntity))
            {
                EntityManager.RemoveComponent<CircleOverlay>(circleEntity);
            }
        }

        // Starts the circle deselection process by initializing the deselection center and radius.
        private void StartCircleDeselection()
        {
            if (TryGetMouseWorldPosition(out Vector3 pos))
            {
                isDeselecting = true;
                deselectCircleCenter = pos;
                deselectCircleRadius = 0; // Initial radius, will be updated as the user drags the mouse
                if (!EntityManager.HasComponent<DeselectCircleOverlay>(deselectCircleEntity))
                {
                    EntityManager.AddComponentData(deselectCircleEntity, new DeselectCircleOverlay { center = deselectCircleCenter, radius = deselectCircleRadius });
                }
            }
        }

        // Updates the radius of the circle deselection based on the mouse position.
        private void UpdateCircleDeselectionRadius()
        {
            if (TryGetMouseWorldPosition(out Vector3 currentMousePosition))
            {
                deselectCircleRadius = Vector3.Distance(deselectCircleCenter, currentMousePosition);
                EntityManager.SetComponentData(deselectCircleEntity, new DeselectCircleOverlay { center = deselectCircleCenter, radius = deselectCircleRadius });
            }
        }

        // Finalizes the circle deselection and deselects entities within the circle.
        private void FinalizeCircleDeselection()
        {
            DeselectEntitiesWithinCircle(deselectCircleCenter, deselectCircleRadius);
            EntityManager.RemoveComponent<DeselectCircleOverlay>(deselectCircleEntity);
            isDeselecting = false;
        }

        // Aborts the circle deselection process and removes the deselection circle overlay.
        private void AbortCircleDeselection()
        {
            isDeselecting = false;
            if (EntityManager.HasComponent<DeselectCircleOverlay>(deselectCircleEntity))
            {
                EntityManager.RemoveComponent<DeselectCircleOverlay>(deselectCircleEntity);
            }
        }

        // Selects entities within the given circle.
        private void SelectEntitiesWithinCircle(Vector3 center, float radius)
        {
            EntitiesWithinSelection(center, radius);
        }

        // Deselects entities within the given circle.
        private void DeselectEntitiesWithinCircle(Vector3 center, float radius)
        {
            DeselectList(SelectedRoads, center, radius);
            DeselectList(SelectedBuildings, center, radius);
            DeselectList(SelectedTrees, center, radius);
            DeselectList(SelectedProps, center, radius);
            DeselectList(SelectedAreas, center, radius);
        }

        // Deselects entities in the given list that are within the circle defined by the center and radius.
        private void DeselectList(List<Entity> entityList, Vector3 center, float radius)
        {
            List<Entity> entitiesToRemove = new List<Entity>();

            for (int i = 0; i < entityList.Count; i++)
            {
                var entity = entityList[i];
                bool removeEntity = false;

                if (IsEntityWithinRadius(entity, center, radius))
                {
                    removeEntity = true;
                }

                if (removeEntity)
                {
                    entitiesToRemove.Add(entity);
                    entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.RemoveHighlight);
                }
            }

            // Remove the deselected entities from the list
            foreach (var entity in entitiesToRemove)
            {
                entityList.Remove(entity);
            }
        }

        // Checks if the given entity is within the specified radius from the center.
        private bool IsEntityWithinRadius(Entity entity, Vector3 center, float radius)
        {
            if (entityManager.HasComponent<Game.Objects.Transform>(entity))
            {
                var transform = entityManager.GetComponentData<Game.Objects.Transform>(entity);
                float distance = Vector3.Distance(transform.m_Position, center);
                if (distance <= radius) return true;
            }

            if (entityManager.HasComponent<Curve>(entity))
            {
                var curve = entityManager.GetComponentData<Curve>(entity);
                if (Vector3.Distance(curve.m_Bezier.a, center) <= radius ||
                    Vector3.Distance(curve.m_Bezier.b, center) <= radius ||
                    Vector3.Distance(curve.m_Bezier.c, center) <= radius ||
                    Vector3.Distance(curve.m_Bezier.d, center) <= radius)
                {
                    return true;
                }
            }

            if (entityManager.HasComponent<Game.Areas.Node>(entity))
            {
                var nodesBuffer = entityManager.GetBuffer<Game.Areas.Node>(entity);
                var nodesArray = nodesBuffer.ToNativeArray(Allocator.TempJob);
                for (int j = 0; j < nodesArray.Length; j++)
                {
                    if (CheckNodesWithinRadius(nodesArray[j], center, radius))
                    {
                        nodesArray.Dispose();
                        return true;
                    }
                }
                nodesArray.Dispose();
            }

            return false;
        }

        // Checks if a given area node is within the radius to be able to select Areas or "surfaces"
        private bool CheckNodesWithinRadius(Game.Areas.Node node, Vector3 center, float radius)
        {
            return Vector3.Distance(node.m_Position, center) <= radius;
        }
    }
}
