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
		private void RaycastSelect()
		{
			if (GetRaycastResult(out Entity e, out Game.Common.RaycastHit result))
			{
				if (e != Entity.Null)
				{
					if (e != lastSelectedEntity)
					{
						if (!Selected.Contains(e))
						{
							// Classify the entity and add it to the appropriate list
							if (EntityManager.HasComponent<Curve>(e))
							{
								SelectedRoads.Add(e);
							}
							else if (EntityManager.HasComponent<Building>(e))
							{
								SelectedBuildings.Add(e);
								if(EntityManager.HasComponent<PseudoRandomSeed>(e))
								{
									seed = EntityManager.GetComponentData<PseudoRandomSeed>(e);
								}
							}
							else if (EntityManager.HasComponent<Plant>(e))
							{
								SelectedTrees.Add(e);
							}
							else if (EntityManager.HasComponent<Game.Objects.Object>(e))
							{
								SelectedProps.Add(e);
							}

							Selected.Add(e);
							EntityManager.ChangeHighlighting_MainThread(e, Highlighter.ChangeMode.AddHighlight);
							lastSelectedEntity = e;
						}
					}
				}
				else
				{
				}
			}
		}

		private void HandleHover(Entity e, Game.Common.RaycastHit rc)
		{
			prev = HoveredEntity;
			HoveredEntity = e;
			LastPos = rc.m_HitPosition;

			if (prev != HoveredEntity)
			{
				if (!SelectedBuildings.Contains(prev) && !SelectedProps.Contains(prev) && !SelectedRoads.Contains(prev) && !SelectedTrees.Contains(prev))
				{
					EntityManager.ChangeHighlighting_MainThread(prev, Highlighter.ChangeMode.RemoveHighlight);
				}
				EntityManager.ChangeHighlighting_MainThread(HoveredEntity, Highlighter.ChangeMode.AddHighlight);
			}
		}

		private void HandleHoverClear()
		{
			if (!SelectedBuildings.Contains(prev) && !SelectedProps.Contains(prev) && !SelectedRoads.Contains(prev) && !SelectedTrees.Contains(prev))
			{
				EntityManager.ChangeHighlighting_MainThread(prev, Highlighter.ChangeMode.RemoveHighlight);
			}
			if (!SelectedBuildings.Contains(HoveredEntity) && !SelectedProps.Contains(HoveredEntity) && !SelectedRoads.Contains(HoveredEntity) && !SelectedTrees.Contains(HoveredEntity))
			{
				EntityManager.ChangeHighlighting_MainThread(HoveredEntity, Highlighter.ChangeMode.RemoveHighlight);
			}
			HoveredEntity = Entity.Null;
		}
	}
}