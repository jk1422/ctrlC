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

	public partial class SelectionTool : SelectionToolSystem
	{
        // Logging
        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(SelectionTool)}").SetShowsErrorsInUI(false);

        // Systems and EntityManager
        private EntityManager entityManager;
        private ModUISystem modUISystem;
        private StampPlacementTool stampPlacementTool;
        private ToolRaycastSystem toolRaycastSystem;

        // Selection lists
        public List<Entity> SelectedRoads = new List<Entity>();
        public List<Entity> SelectedBuildings = new List<Entity>();
        public List<Entity> SelectedTrees = new List<Entity>();
        public List<Entity> SelectedProps = new List<Entity>();
        public List<Entity> SelectedAreas = new List<Entity>();

        public PseudoRandomSeed seed = new PseudoRandomSeed();

        // Input and actions
        private ProxyAction _copyAction;
        private InputAction _altModifier;
        private InputAction _ApplyAction;
        private InputAction _SecondaryApplyAction;

        // State-vars
        private bool isSelecting = false;
        private bool isDeselecting = false;

        // Circle Selection 
        private Vector3 circleCenter;
        private float circleRadius;
        private Vector3 deselectCircleCenter;
        private float deselectCircleRadius;
        private Entity circleEntity;
        private Entity deselectCircleEntity;
        private Entity idleCircleEntity;

        // Ray vars
        private Entity lastSelectedEntity;
        public Entity HoveredEntity;
        public float3 LastPos;
        private Entity prev;
        private bool rayDefaultMode = true;

        // Filters
        public SelectableFilters activeFilters = SelectableFilters.None;
        private EntityQuery selectablesQuery;

        // This is called when the tool is created (Not the same as started)
        protected override void OnCreate()
		{
			base.OnCreate();

            // Make sure the tool isn't enabled when the game just started
            Enabled = false;
		}

        // A public function to toggle the tool on and off
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

        // This function is called on enabling the tool
        protected override void OnStartRunning()
        {
            // Call base implementation to ensure the necessary setup is done
            base.OnStartRunning();

            // Mark the tool as enabled and initialize required systems and tools
            Enabled = true;
            InitializeToolContext();

            // Enable input actions for user interactions
            EnableActions(true);

            // Notify the UI that this tool is now active
            modUISystem.sct_tool_enabled = true;
        }

        protected override void OnStopRunning()
        {
            // Call base implementation to ensure proper shutdown procedures are executed
            base.OnStopRunning();

            // Notify the UI that this tool is now disabled
            modUISystem.sct_tool_enabled = false;

            // Disable input actions to prevent further user interactions
            EnableActions(false);

            // Clean up resources and reset tool state
            CleanUp();

            // Mark the tool as disabled
            Enabled = false;
        }

        // Manually creating actions (This was the old way)
        private void SetActions()
		{
            _ApplyAction = new InputAction("SelectObject_Action", InputActionType.Button);
            _ApplyAction.AddBinding("<Mouse>/leftButton");
            _SecondaryApplyAction = new InputAction("DeselectObject_Action", InputActionType.Button);
            _SecondaryApplyAction.AddBinding("<Mouse>/rightButton");
            _altModifier = new InputAction("SelectObject_AltModifier", InputActionType.Button);
            _altModifier.AddBinding("<Keyboard>/alt");
        }
		private void EnableActions(bool x)
		{
            if (x) // Enable
			{
				// Defining actions
                SetActions();

                // Enabling actions to ensure we can use them
                _ApplyAction.Enable();
                _SecondaryApplyAction.Enable();
                _altModifier.Enable();

				// Updated actions to use bindings set in ctrlC's settings
                _copyAction = Mod.m_CopyAction;
                _copyAction.shouldBeEnabled = true;
            }
			else // Disable
			{
                // InputActions is Enabled/Disabled by calling Enable() or Disable();
                _ApplyAction.Disable();
                _SecondaryApplyAction.Disable();
                _altModifier.Disable();

                // ProxyActions is Enabled/Disabled by setting shouldBeEnabled to true/false
                _copyAction.shouldBeEnabled = false;
            }
		}
        
        // Remove all highlights on every selected object
		private void RemoveHighlights()
		{
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
        }

        // Cleans up the tool's resources and resets its state to prevent any residual data
        private void CleanUp()
        {
            // Remove highlights from all selected objects
            RemoveHighlights();

            // Remove components and destroy temporary entities
            DestroyEntity(circleEntity);
            DestroyEntity(deselectCircleEntity);
            DestroyEntity(idleCircleEntity);

            // Clear selection lists
            ClearSelectionLists();

            // Reset input actions to prevent unintended triggers
            ResetInputActions();

            // Reset state variables
            ResetStateVariables();

            // Reset system references and queries
            ResetSystemReferences();
        }

        // This function is called when tool is started and sets up all tools and systems we need
        private void InitializeToolContext()
		{
			entityManager = World.EntityManager;
			modUISystem = World.GetOrCreateSystemManaged<ModUISystem>();
            stampPlacementTool = World.GetOrCreateSystemManaged<StampPlacementTool>();
            toolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();

            selectablesQuery = GetEntityQuery( ComponentType.ReadOnly<Game.Objects.Transform>());

            circleEntity = EntityManager.CreateEntity(typeof(CircleOverlay));
            deselectCircleEntity = EntityManager.CreateEntity(typeof(DeselectCircleOverlay));
            idleCircleEntity = EntityManager.CreateEntity(typeof(CircleIdle));
        }

        // This function is for copying (Duuh)
        public void Copy()
        {
            // Creating an empty CtrlCStampPrefab
            AssetStampPrefab assetStamp = null;

            // To ensure the mod or the game wont crash if something goes wrong we're using a try-catch block
            try
            {
                // Calling the CopySystem to run some magic copy-logic and store the copied Prefab in our assetStamp
                assetStamp = CopySystem.CopyItems(entityManager, m_PrefabSystem, SelectedBuildings, SelectedRoads, SelectedProps, SelectedTrees, SelectedAreas);

                // When the copying is done, we call StampPlacementTool to be able to place our newly copied prefab
                stampPlacementTool.ActivateTool(assetStamp, this.m_PrefabSystem);
                assetStamp = null;
            }
            catch (Exception ex)
            {
                log.Info($"Not critical but something went wrong when copying: {ex.Message}."); 

                // Incase something goes horribly wrong, we deactivate the tool 
                m_ToolSystem.selected = Entity.Null;
                m_ToolSystem.activeTool = m_DefaultToolSystem;
            }
        }

		//TODO: Clean up this mess lazy ass 
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

			if (!_altModifier.IsPressed() && _ApplyAction.IsPressed())
			{
				RaycastSelect();
			}

			if (GetRaycastResult(out Entity e, out Game.Common.RaycastHit hit))
			{
				HandleHover(e, hit);
				if (!_altModifier.IsPressed() && _SecondaryApplyAction.IsPressed())
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
					
					All = new ComponentType[] { typeof(Edge), typeof(Curve), typeof(Aggregated) },
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