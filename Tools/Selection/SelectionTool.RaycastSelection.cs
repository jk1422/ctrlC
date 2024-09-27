using ctrlC.Utils;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.Objects;
using Unity.Entities;

namespace ctrlC.Tools.Selection
{
	public partial class SelectionTool
	{
        /// <summary>
        /// This performs a raycast selection and adds the selected entity to the appropriate list if not already selected
        /// </summary>
        private void RaycastSelect()
        {
            if (GetRaycastResult(out Entity entity, out RaycastHit result))
            {
                if (entity != Entity.Null && entity != lastSelectedEntity && !IsEntityAlreadySelected(entity))
                {
                    ClassifyAndSelectEntity(entity);
                    lastSelectedEntity = entity;
                }
            }
        }

        // Handles hover highlighting when the raycast hits a new entity
        private void HandleHover(Entity entity, RaycastHit hit)
        {
            var previousHoveredEntity = HoveredEntity;
            HoveredEntity = entity;
            LastPos = hit.m_HitPosition;

            if (previousHoveredEntity != HoveredEntity)
            {
                UpdateEntityHighlighting(previousHoveredEntity, Highlighter.ChangeMode.RemoveHighlight);
                UpdateEntityHighlighting(HoveredEntity, Highlighter.ChangeMode.AddHighlight);
            }
        }

        // Clears hover highlighting when the raycast no longer hits an entity
        private void HandleHoverClear()
        {
            UpdateEntityHighlighting(HoveredEntity, Highlighter.ChangeMode.RemoveHighlight);
            HoveredEntity = Entity.Null;
        }

        // Classifies an entity and adds it to the appropriate selection list
        private void ClassifyAndSelectEntity(Entity entity)
        {
            if (EntityManager.HasComponent<Curve>(entity))
            {
                SelectedRoads.Add(entity);
            }
            else if (EntityManager.HasComponent<Building>(entity))
            {
                SelectedBuildings.Add(entity);
                UpdatePseudoRandomSeed(entity);
            }
            else if (EntityManager.HasComponent<Plant>(entity))
            {
                SelectedTrees.Add(entity);
            }
            else if (EntityManager.HasComponent<Game.Objects.Object>(entity))
            {
                SelectedProps.Add(entity);
            }
            else if (EntityManager.HasComponent<Game.Areas.Area>(entity))
            {
                SelectedAreas.Add(entity);
            }

            // Highlight the newly selected entity
            EntityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.AddHighlight);
        }

        private void UpdatePseudoRandomSeed(Entity entity)
        {
            if (EntityManager.HasComponent<PseudoRandomSeed>(entity))
            {
                seed = EntityManager.GetComponentData<PseudoRandomSeed>(entity);
            }
        }

        private void UpdateEntityHighlighting(Entity entity, Highlighter.ChangeMode mode)
        {
            if (entity != Entity.Null && !IsEntityAlreadySelected(entity))
            {
                EntityManager.ChangeHighlighting_MainThread(entity, mode);
            }
        }

        private bool IsEntityAlreadySelected(Entity entity)
        {
            return SelectedBuildings.Contains(entity) ||
                   SelectedProps.Contains(entity) ||
                   SelectedRoads.Contains(entity) ||
                   SelectedTrees.Contains(entity) ||
                   SelectedAreas.Contains(entity);
        }
    }
}