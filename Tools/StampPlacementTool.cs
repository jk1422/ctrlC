using Colossal.Logging;
using ctrlC.Components;
using ctrlC.Systems;
using ctrlC.Tools.Selection;
using Game.Buildings;
using Game.Common;
using Game.Input;
using Game.Prefabs;
using Game.Tools;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace ctrlC.Tools
{
	public partial class StampPlacementTool : CustomOTS
	{
        // Properties
        public override string toolID => base.toolID;
        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(StampPlacementTool)}").SetShowsErrorsInUI(false);

        // Fields
        public PrefabBase _OriginalPre;
        public PrefabSystem _prefabSystem;
        internal CtrlCStampPrefab ctrlCStamp;

        // Input Actions
        public InputAction c_ApplyAction;
        public InputAction c_SecondaryApplyAction;
        public InputAction restoreDefault;

        public ProxyAction elevateUp;
        public ProxyAction elevateDown;
        public ProxyAction mirrorAction;

        // Other Systems
        public ModUISystem modUISystem;
        private SelectionTool _selectionTool;

        // State variables
        private bool prepareCameraMode = false;
        private bool isPlaceholderPaused = false;
        private bool schedulePlaceholderPaused = false;
        private ControlPoint lastControlPoint;
        private float controlPointTolerance = 0.01f;
        private float pauseDelay = 0.2f;
        private float pauseTimer = 0f;
        public float elevation { get; set; } = 2.5f;
		

		public override PrefabBase GetPrefab()
		{
			return this.prefab;
		}

		public override bool TrySetPrefab(PrefabBase prefab)
		{
			if (prefab is CtrlCStampPrefab assetStamp && this.Enabled)
			{
				Mode mode = Mode.Stamp;
				ctrlCStamp = assetStamp;
				this.prefab = assetStamp;
				this.mode = mode;
				return true;
			}
			return false;
		}

		protected override void OnCreate()
		{
			base.OnCreate();
			Enabled = false;
		}

		public void ActivateTool(CtrlCStampPrefab stampPrefab, PrefabSystem prefabSystem)
		{
			Enabled = true;
			if (TrySetPrefab(stampPrefab))
			{
				_prefabSystem = prefabSystem;
				modUISystem = World.GetOrCreateSystemManaged<ModUISystem>();
				modUISystem.place_tool_enabled = true;
				m_ToolSystem.activeTool = this;
            } 
			else{

				log.Info($"Cannot set prefab. ");
			}
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();

			InitializeInputActions();
			EnableInputActions(true);


            base.allowUnderground = false;
			allowUnderground = false;
		}

		protected override void OnStopRunning()
		{
            // Disable input actions
			EnableInputActions(false);

			modUISystem.place_tool_enabled = false;

			// Mark prefabs as obsolete and remove them
			if (_OriginalPre != null)
			{
				_OriginalPre.name = "obsolete";
				_OriginalPre.Remove<PrefabBase>();
			}

			if (this.prefab != null)
			{
				this.prefab.name = "obsolete";
				this.prefab.Remove<PrefabBase>();
			}

			// Update and clear prefabs
			try
			{
				_prefabSystem?.UpdatePrefab(this.prefab);
				_prefabSystem?.UpdatePrefab(_OriginalPre);
			}
			catch (Exception ex)
			{
				log.Info($"217: {ex.Message}");
			}

			_prefabSystem?.ClearAvailabilityCache();

			// Clean up references
			modUISystem = null;
			_prefabSystem = null;
			this.prefab = null;
			_OriginalPre = null;
			
			base.OnStopRunning();
			_selectionTool = World.GetOrCreateSystemManaged<SelectionTool>();
			m_ToolSystem.activeTool = _selectionTool;
			

		}


		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			bool forceCancel = m_ForceCancel;
			m_ForceCancel = false;

			HandleInputEvents();


            if (m_State == State.Adding || m_State == State.Removing)
			{
				if (c_ApplyAction.WasPressedThisFrame() || c_ApplyAction.WasReleasedThisFrame())
				{
					return Apply(inputDeps);
				}

				if (forceCancel || c_SecondaryApplyAction.WasPressedThisFrame() || c_SecondaryApplyAction.WasReleasedThisFrame())
				{
					return Cancel(inputDeps);
				}

				return Update(inputDeps);
			}
			if (c_ApplyAction.WasPressedThisFrame())
			{
				if (mode == Mode.Move)
				{
					m_ToolSystem.activeTool = m_DefaultToolSystem;
					m_TerrainSystem.OnBuildingMoved(m_MovingObject);
				}

				return Apply(inputDeps, c_ApplyAction.WasReleasedThisFrame());
			}


			if ((mode != Mode.Upgrade || (m_SnapOffMask & Snap.OwnerSide) != 0) && c_SecondaryApplyAction.WasPressedThisFrame())
			{
				return Cancel(inputDeps, c_SecondaryApplyAction.WasReleasedThisFrame());
			}
			if(m_State != State.Rotating && c_SecondaryApplyAction.IsPressed())
			{
				m_State = State.Rotating;
			}
			if (m_State == State.Rotating && c_SecondaryApplyAction.WasReleasedThisFrame())
			{
				StopRotating();
				return Update(inputDeps);
			}

			if (m_State != 0 && (c_ApplyAction.WasReleasedThisFrame() || c_SecondaryApplyAction.WasReleasedThisFrame()))
			{
				m_State = State.Default;
			}
			if (m_State == State.Rotating)
			{
				
					
				float3 @float = InputManager.instance.mousePosition;
				if (@float.x != m_RotationStartPosition.x)
				{
					Rotation value2 = m_Rotation.value;
					float angle = (@float.x - m_RotationStartPosition.x) * ((float)Math.PI * 2f) * 0.002f;

					
					value2.m_Rotation = math.normalizesafe(math.mul(m_StartRotation, quaternion.RotateY(angle)), quaternion.identity);
					

					m_RotationModified = true;
					value2.m_IsAligned = false;
					m_Rotation.value = value2;
				}

					
				
			}

			//TODO: create a new action for Apply. Make a function to disable m_ApplyAction when preparing Camera mode
			if (prepareCameraMode && c_ApplyAction.WasPerformedThisFrame())
			{
				
				if (!isPlaceholderPaused)
				{
					schedulePlaceholderPaused = true;

					pauseTimer = 0f; // Reset the timer when scheduling a pause
				}
				else
				{
					isPlaceholderPaused = false;
				}
				
			}
			if (schedulePlaceholderPaused)
			{
				HandleSchedulePlaceholderPaused();
			}

			if (isPlaceholderPaused)
			{

				//return inputDeps;

				return default;
			}
			else
			{
				return base.OnUpdate(inputDeps);
			}
			
		}


        // Initialize input actions
        private void InitializeInputActions()
        {
            c_ApplyAction = new InputAction("SelectObject_Action", InputActionType.Button);
            c_SecondaryApplyAction = new InputAction("SelectObject_SecondAction", InputActionType.Button);
            restoreDefault = new InputAction("restoreDefaultAction", InputActionType.Button);

            c_ApplyAction.AddBinding("<Mouse>/leftButton");
            c_SecondaryApplyAction.AddBinding("<Mouse>/rightButton");

            restoreDefault.AddCompositeBinding("ButtonWithOneModifier")
                .With("Modifier", "<Keyboard>/ctrl")
                .With("Button", "<Keyboard>/z");

            mirrorAction = Mod.m_MirrorAction;
            elevateUp = Mod.m_RaiseAction;
            elevateDown = Mod.m_FlattenAction;

        }

        // Enable or disable input actions
        private void EnableInputActions(bool enable)
        {
            if (enable)
            {
                restoreDefault.Enable();
                c_ApplyAction.Enable();
                c_SecondaryApplyAction.Enable();

                elevateUp.shouldBeEnabled = true;
                elevateDown.shouldBeEnabled = true;
                mirrorAction.shouldBeEnabled = true;
            }
            else
            {
                restoreDefault.Disable();
                c_ApplyAction.Disable();
                c_SecondaryApplyAction.Disable();

                elevateUp.shouldBeEnabled = false;
                elevateDown.shouldBeEnabled = false;
                mirrorAction.shouldBeEnabled = false;
            }
        }

        // Handle input events such as elevation changes and rotations
        private void HandleInputEvents()
        {
            if (elevateDown.WasPerformedThisFrame()) HandleElevationDown();
            if (elevateUp.WasPerformedThisFrame()) HandleElevationUp();
            if (mirrorAction.WasPerformedThisFrame()) MirrorPrefab();
        }

        private void HandleSchedulePlaceholderPaused()
		{
			if (GetRaycastResult(out ControlPoint currentControlPoint))
			{
				if (lastControlPoint.Equals(default(ControlPoint)))
				{
					lastControlPoint = currentControlPoint;
					pauseTimer = 0f; // Reset the timer
				}
				else
				{
					float distance = math.distance(lastControlPoint.m_Position, currentControlPoint.m_Position);
					if (distance < controlPointTolerance)
					{
						pauseTimer += SystemAPI.Time.DeltaTime;
						if (pauseTimer >= pauseDelay)
						{
							// Mouse is still for the duration of the delay, we can pause the placeholder
							isPlaceholderPaused = true;
							schedulePlaceholderPaused = false;
							// Optionally save the current state here if needed
						}
					}
					else
					{
						// Mouse moved, reset the timer and last control point
						lastControlPoint = currentControlPoint;
						pauseTimer = 0f;
					}
				}
			}
		}


		private void HandleElevationUp()
		{
			if (modUISystem.fullElevationStep)
			{
				elevation = 1.5f;
			}
			else
			{
				elevation = 0.5f;
			}

			foreach (var subnet in this.prefab.GetComponent<ObjectSubNets>().m_SubNets)
			{
				subnet.m_BezierCurve.a.y += elevation;
				subnet.m_BezierCurve.b.y += elevation;
				subnet.m_BezierCurve.c.y += elevation;
				subnet.m_BezierCurve.d.y += elevation;
			}

			_prefabSystem.UpdatePrefab(this.prefab);
			base.m_ForceUpdate = true;
		}

		private void HandleElevationDown()
		{
			if (modUISystem.fullElevationStep)
			{
				elevation = 1.5f;
			}
			else
			{
				elevation = 0.5f;
			}

			foreach (var subnet in this.prefab.GetComponent<ObjectSubNets>().m_SubNets)
			{
				subnet.m_BezierCurve.a.y -= elevation;
				subnet.m_BezierCurve.b.y -= elevation;
				subnet.m_BezierCurve.c.y -= elevation;
				subnet.m_BezierCurve.d.y -= elevation;

				if (subnet.m_BezierCurve.a.y < 0)
				{
					subnet.m_BezierCurve.a.y = 0;
				}
				if (subnet.m_BezierCurve.b.y < 0)
				{
					subnet.m_BezierCurve.b.y = 0;
				}
				if (subnet.m_BezierCurve.c.y < 0)
				{
					subnet.m_BezierCurve.c.y = 0;
				}
				if (subnet.m_BezierCurve.d.y < 0)
				{
					subnet.m_BezierCurve.d.y = 0;
				}
			}

			_prefabSystem.UpdatePrefab(this.prefab);
			base.m_ForceUpdate = true;
		}
		public void SavePrefab(string name, int category)
		{
			SaveSystem.SavePrefab(EntityManager, _prefabSystem, this.ctrlCStamp, name, category);
			
		}

		public void LoadStamp()
		{
			ObjectPrefab old = this.prefab;
			TrySetPrefab(_prefabSystem.DuplicatePrefab(_OriginalPre));
			old.name = "obsolete";
			old.Remove<PrefabBase>();
			_prefabSystem.UpdatePrefab(old);
			base.m_ForceUpdate = true;



		}

		public void MirrorPrefab()
		{
			foreach (var subnet in this.prefab.GetComponent<ObjectSubNets>().m_SubNets)
			{
				subnet.m_BezierCurve.a.x = invertedFloat(subnet.m_BezierCurve.a.x);
				subnet.m_BezierCurve.b.x = invertedFloat(subnet.m_BezierCurve.b.x);
				subnet.m_BezierCurve.c.x = invertedFloat(subnet.m_BezierCurve.c.x);
				subnet.m_BezierCurve.d.x = invertedFloat(subnet.m_BezierCurve.d.x);
			}

			foreach (var subobj in this.prefab.GetComponent<ObjectSubObjects>().m_SubObjects)
			{
				subobj.m_Position.x = invertedFloat(subobj.m_Position.x);
				subobj.m_Rotation = new quaternion(subobj.m_Rotation.value.x, invertedFloat(subobj.m_Rotation.value.y), subobj.m_Rotation.value.z, subobj.m_Rotation.value.w);
			}

			_prefabSystem.UpdatePrefab(this.prefab);

			base.m_ForceUpdate = true;
		}

		public float invertedFloat(float original)
		{
			return original - (original * 2);
		}
	}
}
