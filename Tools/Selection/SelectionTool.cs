using Colossal.Logging;
using ctrlC.Components;
using ctrlC.Rendering;
using ctrlC.Systems;
using ctrlC.Utils;
using Game.Buildings;
using Game.Common;
using Game.Creatures;
using Game.Input;
using Game.Net;
using Game.Notifications;
using Game.Objects;
using Game.Prefabs;
using Game.Tools;
using Game.UI.InGame;
using Game.Vehicles;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ctrlC.Tools.Selection
{
	public struct SelectableTag : IComponentData
	{ }

	public partial class SelectionTool : SelectionToolSystem
	{
		public List<Entity> Selected = new List<Entity>();
		public List<Entity> SelectedRoads = new List<Entity>();
		public List<Entity> SelectedBuildings = new List<Entity>();
		public List<Entity> SelectedTrees = new List<Entity>();
		public List<Entity> SelectedProps = new List<Entity>();

		public PseudoRandomSeed seed = new PseudoRandomSeed();

		private Dictionary<Entity, int> SeedDictionary = new Dictionary<Entity, int>();



		private InputAction _copyAction;
		private InputAction _altModifier;
		private InputAction m_ApplyAction;
		private InputAction m_SecondaryApplyAction;
		private StampPlacementTool stampPlacementTool;
		private bool isSelecting = false;
		private bool isDeselecting = false;

		private Vector3 circleCenter;
		private float circleRadius;
		private Vector3 deselectCircleCenter;
		private float deselectCircleRadius;

		// ray
		private Entity lastSelectedEntity;

		public Entity HoveredEntity;
		public float3 LastPos;
		private Entity prev;
		private bool rayDefaultMode = true;

		// New filter system
		public SelectableFilters activeFilters = SelectableFilters.None;

		private EntityQuery selectablesQuery;
		private Entity circleEntity;
		private Entity deselectCircleEntity;
		private Entity idleCircleEntity;
		private ToolRaycastSystem toolRaycastSystem;

		private ModUISystem modUISystem => World.GetOrCreateSystemManaged<ModUISystem>();

		public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(SelectionTool)}").SetShowsErrorsInUI(false);

		private EntityManager entityManager => World.EntityManager;




		protected override void OnCreate()
		{
			base.OnCreate();

			Enabled = false;
			m_ApplyAction = new InputAction("SelectObject_Action", InputActionType.Button);
			m_ApplyAction.AddBinding("<Mouse>/leftButton");
			m_SecondaryApplyAction = new InputAction("DeselectObject_Action", InputActionType.Button);
			m_SecondaryApplyAction.AddBinding("<Mouse>/rightButton");
			_altModifier = new InputAction("SelectObject_AltModifier", InputActionType.Button);
			_altModifier.AddBinding("<Keyboard>/alt");

			_copyAction = new InputAction("SelectObject_Copy", InputActionType.Button);
			_copyAction.AddCompositeBinding("ButtonWithOneModifier")
					   .With("Modifier", "<Keyboard>/ctrl")
					   .With("Button", "<Keyboard>/c");

			stampPlacementTool = World.GetOrCreateSystemManaged<StampPlacementTool>();

			selectablesQuery = GetEntityQuery(
				ComponentType.ReadOnly<Game.Objects.Transform>()
			);

			circleEntity = EntityManager.CreateEntity(typeof(CircleOverlay));
			deselectCircleEntity = EntityManager.CreateEntity(typeof(DeselectCircleOverlay));
			idleCircleEntity = EntityManager.CreateEntity(typeof(CircleIdle));
		}

		public void ToggleTool(bool enable)
		{
			if (enable && m_ToolSystem.activeTool != this)
			{
				Enabled = true;
				m_ToolSystem.selected = Entity.Null;
				m_ToolSystem.activeTool = this;

				UIObjectHelper.FindPrefabsInGroup(EntityManager);
			}
			else if (!enable && m_ToolSystem.activeTool == this)
			{
				this.Enabled = false;
				m_ToolSystem.selected = Entity.Null;
				m_ToolSystem.activeTool = m_DefaultToolSystem;
			}
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			Enabled = true;
			_copyAction.Enable();
			toolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();
			m_ApplyAction.Enable();
			
			m_SecondaryApplyAction.Enable();
			
			_altModifier.Enable();
			modUISystem.sct_tool_enabled = true;
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();

			// Disable input actions
			_copyAction.Disable();
			m_ApplyAction.Disable();
			m_SecondaryApplyAction.Disable();
			_altModifier.Disable();

			// Remove components from entities
			if (EntityManager.Exists(circleEntity))
			{
				EntityManager.RemoveComponent<CircleOverlay>(circleEntity);
				EntityManager.RemoveComponent<CircleIdle>(circleEntity);
			}

			if (EntityManager.Exists(deselectCircleEntity))
			{
				EntityManager.RemoveComponent<DeselectCircleOverlay>(deselectCircleEntity);
			}

			// Update UI and state
			modUISystem.sct_tool_enabled = false;
			Enabled = false;

			// Clear highlighting from selected entities
			foreach (var entity in SelectedBuildings)
			{
				if (entityManager.Exists(entity))
				{
					entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.RemoveHighlight);
				}
			}
            foreach (var entity in SelectedProps)
            {
                if (entityManager.Exists(entity))
                {
                    entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.RemoveHighlight);
                }
            }
            foreach (var entity in SelectedRoads)
            {
                if (entityManager.Exists(entity))
                {
                    entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.RemoveHighlight);
                }
            }
            foreach (var entity in SelectedTrees)
            {
                if (entityManager.Exists(entity))
                {
                    entityManager.ChangeHighlighting_MainThread(entity, Highlighter.ChangeMode.RemoveHighlight);
                }
            }
			SelectedBuildings.Clear();
			SelectedProps.Clear();
			SelectedRoads.Clear();
			SelectedTrees.Clear();

            lastSelectedEntity = Entity.Null;

			// Reset active tool to default tool system if necessary
			try
			{
				if (m_ToolSystem.activeTool == this)
				{
					m_ToolSystem.selected = Entity.Null;
					m_ToolSystem.activeTool = m_DefaultToolSystem;
				}
			}
			catch (Exception) { }
		}

        public void Copy()
        {

            log.Info($"Copy shall we?");
            CtrlCStampPrefab assetStamp = null;
            try
            {

                log.Info($"calling copySystem to create an asset stamp");
                assetStamp = CopySystem.CopyItems(entityManager, m_PrefabSystem, SelectedBuildings, SelectedRoads, SelectedProps, SelectedTrees);

                log.Info($"Created assetstamp: {assetStamp.name}");
                stampPlacementTool.ActivateTool(assetStamp, this.m_PrefabSystem);
                assetStamp = null;

            }
            catch (Exception ex)
            {
                log.Info($"Not critical but something went wrong when copying: {ex.Message}.");

                m_ToolSystem.selected = Entity.Null;
                m_ToolSystem.activeTool = m_DefaultToolSystem;
            }



        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (_copyAction.WasPressedThisFrame())
			{
				
				Copy();
				
			}
			if (_altModifier.WasPerformedThisFrame() && rayDefaultMode == true)
			{
				toolRaycastSystem.typeMask = TypeMask.All;
				toolRaycastSystem.collisionMask = (CollisionMask.OnGround | CollisionMask.Overground);
				toolRaycastSystem.iconLayerMask = IconLayerMask.None;
				toolRaycastSystem.utilityTypeMask = UtilityTypes.None;
				rayDefaultMode = false;
			}
			else if (!_altModifier.WasReleasedThisFrame() && rayDefaultMode == false &&toolRaycastSystem != null)
			{
				toolRaycastSystem.typeMask = TypeMask.All;
				toolRaycastSystem.collisionMask = CollisionMask.OnGround | CollisionMask.Overground;
				toolRaycastSystem.netLayerMask = Layer.Road | Layer.Pathway | Layer.TramTrack | Layer.TrainTrack;
				toolRaycastSystem.iconLayerMask = IconLayerMask.Default;
				toolRaycastSystem.utilityTypeMask = UtilityTypes.Fence;
				toolRaycastSystem.raycastFlags = RaycastFlags.SubElements | RaycastFlags.Cargo | RaycastFlags.Passenger | RaycastFlags.EditorContainers | RaycastFlags.Decals;
				rayDefaultMode = true;
			}
			if (!_altModifier.IsPressed() && EntityManager.HasComponent<CircleIdle>(circleEntity))
			{
				EntityManager.RemoveComponent<CircleIdle>(circleEntity);
			}

			if (!_altModifier.IsPressed() && m_ApplyAction.IsPressed())
			{
				RaycastSelect();
			}

			if (GetRaycastResult(out Entity e, out Game.Common.RaycastHit hit))
			{
				HandleHover(e, hit);
				if (!_altModifier.IsPressed() && m_SecondaryApplyAction.IsPressed())
				{
					if (SelectedBuildings.Contains(e))
					{
						SelectedBuildings.Remove(e);
                        EntityManager.ChangeHighlighting_MainThread(e, Highlighter.ChangeMode.RemoveHighlight);
                        lastSelectedEntity = Entity.Null;
                    }
                    if (SelectedProps.Contains(e))
                    {
                        SelectedProps.Remove(e);
                        EntityManager.ChangeHighlighting_MainThread(e, Highlighter.ChangeMode.RemoveHighlight);
                        lastSelectedEntity = Entity.Null;
                    }
                    if (SelectedRoads.Contains(e))
                    {
                        SelectedRoads.Remove(e);
                        EntityManager.ChangeHighlighting_MainThread(e, Highlighter.ChangeMode.RemoveHighlight);
                        lastSelectedEntity = Entity.Null;
                    }
                    if (SelectedTrees.Contains(e))
                    {
                        SelectedTrees.Remove(e);
                        EntityManager.ChangeHighlighting_MainThread(e, Highlighter.ChangeMode.RemoveHighlight);
                        lastSelectedEntity = Entity.Null;
                    }
				}
			}
			else
			{
				HandleHoverClear();
			}

			UpdateCircleSelection();

			return base.OnUpdate(inputDeps);
		}

		public override void InitializeRaycast()
		{
			try
			{
				base.InitializeRaycast();
				toolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();

				toolRaycastSystem.typeMask = TypeMask.All;
				toolRaycastSystem.collisionMask = CollisionMask.OnGround | CollisionMask.Overground;
				toolRaycastSystem.netLayerMask = Layer.Road | Layer.Pathway | Layer.TramTrack | Layer.TrainTrack;
				toolRaycastSystem.iconLayerMask = IconLayerMask.Default;
				toolRaycastSystem.utilityTypeMask = UtilityTypes.None;
				toolRaycastSystem.raycastFlags = RaycastFlags.SubElements | RaycastFlags.Cargo | RaycastFlags.Passenger | RaycastFlags.EditorContainers | RaycastFlags.Decals;
				rayDefaultMode = true;
			}
			catch (Exception ex)
			{
				log.Error($"Error in InitializeRaycast: {ex.Message}");
			}
		}

		private bool TryGetMouseWorldPosition(out Vector3 position)
		{
			if (GetRaycastResult(out Entity e, out Game.Common.RaycastHit result))
			{
				position = result.m_HitPosition;
				return true;
			}
			position = Vector3.zero;
			return false;
		}

		[Flags]
		public enum SelectableFilters
		{
			None = 0,
			Road = 1 << 0,
			Building = 1 << 1,
			Tree = 1 << 2,
			Prop = 1 << 3,
			Area = 1 << 4
		}

		public SelectableFilters GetActiveFilters()
		{
			SelectableFilters filters = SelectableFilters.None;

			if (modUISystem.sct_roads)
			{
				filters |= SelectableFilters.Road;
			}
			if (modUISystem.sct_buildings)
			{
				filters |= SelectableFilters.Building;
			}
			if (modUISystem.sct_trees)
			{
				filters |= SelectableFilters.Tree;
			}
			if (modUISystem.sct_props)
			{
				filters |= SelectableFilters.Prop;
			}
			if (modUISystem.sct_areas)
			{
				filters |= SelectableFilters.Area;
			}

			return filters;
		}

		public EntityQuery GetAllSelectebles()
		{
			List<EntityQueryDesc> queryDescs = new List<EntityQueryDesc>();

			if ((activeFilters & SelectableFilters.Road) != 0)
			{
				queryDescs.Add(new EntityQueryDesc
				{
					
					All = new ComponentType[] { typeof(Edge), typeof(Curve) },
					None = new ComponentType[] { typeof(Owner) }
				});
			}

			if ((activeFilters & SelectableFilters.Building) != 0)
			{
				queryDescs.Add(new EntityQueryDesc
				{
					All = new ComponentType[] { typeof(Game.Objects.Transform), typeof(Building) },
					None = new ComponentType[] { typeof(Owner) }
				});
			}

			if ((activeFilters & SelectableFilters.Tree) != 0)
			{
				queryDescs.Add(new EntityQueryDesc
				{
					All = new ComponentType[] { typeof(Game.Objects.Transform), typeof(Plant) },
					None = new ComponentType[] { typeof(Owner), typeof(Overridden) }
				});
			}

			if ((activeFilters & SelectableFilters.Prop) != 0)
			{
				queryDescs.Add(new EntityQueryDesc
				{
					All = new ComponentType[] { typeof(Game.Objects.Transform), typeof(Game.Objects.Object) },
					None = new ComponentType[] { typeof(Owner), typeof(Plant), typeof(Building), typeof(Edge), typeof(Curve), typeof(Game.Creatures.Resident), typeof(Creature), typeof(Game.Creatures.Pet), typeof(Animal), typeof(Car), typeof(Vehicle) }
				});
			}

			if ((activeFilters & SelectableFilters.Area) != 0)
			{
				queryDescs.Add(new EntityQueryDesc
				{
					All = new ComponentType[] { typeof(Game.Areas.Surface), typeof(Game.Areas.Node) },
					None = new ComponentType[] { typeof(Owner) }
				});
			}

			EntityQuery m_Group = GetEntityQuery(queryDescs.ToArray());
			return m_Group;
		}
	}
}