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
		private void UpdateCircleSelection()
		{
			if (_altModifier.IsPressed() || isSelecting || isDeselecting)
			{
				if (TryGetMouseWorldPosition(out Vector3 position) && !m_ApplyAction.IsPressed() && !m_SecondaryApplyAction.IsPressed())
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

				if (m_ApplyAction.WasPressedThisFrame() && !isDeselecting)
				{
					StartCircleSelection();
				}
				else if (isSelecting && m_ApplyAction.IsPressed())
				{
					UpdateCircleSelectionRadius();
				}
				else if (m_ApplyAction.WasReleasedThisFrame() && isSelecting)
				{
					FinalizeCircleSelection();
				}
				else if (m_ApplyAction.IsPressed() && m_SecondaryApplyAction.WasPressedThisFrame() && isSelecting)
				{
					AbortCircleSelection();
				}

				if (_altModifier.IsPressed() && m_SecondaryApplyAction.WasPressedThisFrame() && !isSelecting)
				{
					StartCircleDeselection();
				}
				else if (m_SecondaryApplyAction.IsPressed() && isDeselecting)
				{
					UpdateCircleDeselectionRadius();
				}
				else if (m_SecondaryApplyAction.WasReleasedThisFrame() && isDeselecting)
				{
					FinalizeCircleDeselection();
				}
				else if (m_SecondaryApplyAction.IsPressed() && m_ApplyAction.WasPressedThisFrame() && isDeselecting)
				{
					AbortCircleDeselection();
				}
			}
		}

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

		private void UpdateCircleSelectionRadius()
		{
			if (TryGetMouseWorldPosition(out Vector3 currentMousePosition))
			{
				circleRadius = Vector3.Distance(circleCenter, currentMousePosition);
				EntityManager.SetComponentData(circleEntity, new CircleOverlay { center = circleCenter, radius = circleRadius });
			}
		}

		private void FinalizeCircleSelection()
		{
			activeFilters = GetActiveFilters();
			SelectEntitiesWithinCircle(circleCenter, circleRadius);
			activeFilters = SelectableFilters.None;
			EntityManager.RemoveComponent<CircleOverlay>(circleEntity);
			isSelecting = false;
		}

		private void AbortCircleSelection()
		{
			isSelecting = false;
			if (EntityManager.HasComponent<CircleOverlay>(circleEntity))
			{
				EntityManager.RemoveComponent<CircleOverlay>(circleEntity);
			}
		}

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

		private void UpdateCircleDeselectionRadius()
		{
			if (TryGetMouseWorldPosition(out Vector3 currentMousePosition))
			{
				deselectCircleRadius = Vector3.Distance(deselectCircleCenter, currentMousePosition);
				EntityManager.SetComponentData(deselectCircleEntity, new DeselectCircleOverlay { center = deselectCircleCenter, radius = deselectCircleRadius });
			}
		}

		private void FinalizeCircleDeselection()
		{
			DeselectEntitiesWithinCircle(deselectCircleCenter, deselectCircleRadius);
			EntityManager.RemoveComponent<DeselectCircleOverlay>(deselectCircleEntity);
			isDeselecting = false;
		}

		private void AbortCircleDeselection()
		{
			isDeselecting = false;
			if (EntityManager.HasComponent<DeselectCircleOverlay>(deselectCircleEntity))
			{
				EntityManager.RemoveComponent<DeselectCircleOverlay>(deselectCircleEntity);
			}
		}

		private void SelectEntitiesWithinCircle(Vector3 center, float radius)
		{
			entitiesWithinSelection(center, radius);


			log.Info($"Selection is done, this is the result:");
			foreach(var road in SelectedRoads)
			{
				log.Info($"SelectedRoad: {road}");
			}
			foreach (var building in SelectedBuildings)
			{
				log.Info($"Selected Building: {building}");
			}
			foreach (var tree in SelectedTrees)
			{
				log.Info($"Selected Tree: {tree}");
			}
			foreach (var prop in SelectedProps)
			{
				log.Info($"Selected Prop: {prop}");
			}
		}

        private void DeselectEntitiesWithinCircle(Vector3 center, float radius)
        {
            DeselectList(SelectedRoads, center, radius);
            DeselectList(SelectedBuildings, center, radius);
            DeselectList(SelectedTrees, center, radius);
            DeselectList(SelectedProps, center, radius);
        }

        private void DeselectList(List<Entity> entityList, Vector3 center, float radius)
        {
            List<Entity> entitiesToRemove = new List<Entity>();

            for (int i = 0; i < entityList.Count; i++)
            {
                var entity = entityList[i];
                bool removeEntity = false;

                if (entityManager.HasComponent<Game.Objects.Transform>(entity))
                {
                    var transform = entityManager.GetComponentData<Game.Objects.Transform>(entity);
                    float distance = Vector3.Distance(transform.m_Position, center);
                    if (distance <= radius)
                    {
                        removeEntity = true;
                    }
                }

                if (entityManager.HasComponent<Curve>(entity))
                {
                    var curve = entityManager.GetComponentData<Curve>(entity);
                    if (Vector3.Distance(curve.m_Bezier.a, center) <= radius ||
                        Vector3.Distance(curve.m_Bezier.b, center) <= radius ||
                        Vector3.Distance(curve.m_Bezier.c, center) <= radius ||
                        Vector3.Distance(curve.m_Bezier.d, center) <= radius)
                    {
                        removeEntity = true;
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
                            removeEntity = true;
                            break;
                        }
                    }
                    nodesArray.Dispose();
                }

                if (removeEntity)
                {
                    entitiesToRemove.Add(entity);
                    entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.RemoveHighlight);
                }
            }

            foreach (var entity in entitiesToRemove)
            {
                entityList.Remove(entity);
            }
        }

        private bool CheckNodesWithinRadius(Game.Areas.Node node, Vector3 center, float radius)
		{
			if (Vector3.Distance(node.m_Position, center) <= radius) return true;
			return false;
		}
	}
}