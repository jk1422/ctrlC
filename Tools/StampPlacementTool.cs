using ctrlC.AssetManagement;
using ctrlC.Systems;
using ctrlC.Tools.Selection;
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

        // Fields
        public PrefabBase _OriginalPre;
        public PrefabSystem _prefabSystem;
        internal AssetStampPrefab assetStampPrefab;

        // Input Actions
        public InputAction c_ApplyAction;
        public InputAction c_SecondaryApplyAction;
        public InputAction restoreDefault;

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
            if (prefab is AssetStampPrefab assetStamp && this.Enabled)
            {
                assetStampPrefab = assetStamp;
                this.prefab = assetStamp;
                this.mode = Mode.Stamp;
                return true;
            }
            return false;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = false;
        }

        public void ActivateTool(AssetStampPrefab stampPrefab, PrefabSystem prefabSystem)
        {
            Enabled = true;
            if (TrySetPrefab(stampPrefab))
            {
                _prefabSystem = prefabSystem;
                modUISystem = World.GetOrCreateSystemManaged<ModUISystem>();
                modUISystem.PlacementToolEnabled = true;
                m_ToolSystem.activeTool = this;
            }
            else
            {
                log.Info("Cannot set prefab.");
            }
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            // Initialize input actions before enabling them
            InitializeInputActions();

            // Check if input actions are initialized properly
            if (c_ApplyAction != null && c_SecondaryApplyAction != null && restoreDefault != null)
            {
                EnableInputActions(true);
            }
            else
            {
                log.Error("Input actions are not initialized correctly.");
            }

            // Ensure modUISystem is available
            modUISystem = World.GetOrCreateSystemManaged<ModUISystem>();
            if (modUISystem != null)
            {
                modUISystem.PlacementToolEnabled = true;
            }
            else
            {
                log.Error("ModUISystem is not available.");
            }

            allowUnderground = false;
        }

        protected override void OnStopRunning()
        {
            // Check if modUISystem is not null before using it
            if (modUISystem != null)
            {
                modUISystem.PlacementToolEnabled = false;
            }

            // Disable input actions safely
            if (c_ApplyAction != null && c_SecondaryApplyAction != null && restoreDefault != null)
            {
                EnableInputActions(false);
            }



            UpdatePrefabSafe(_prefabSystem, this.prefab);
            UpdatePrefabSafe(_prefabSystem, _OriginalPre);

            _prefabSystem?.ClearAvailabilityCache();

            CleanUpReferences();

            base.OnStopRunning();

            _selectionTool = World.GetOrCreateSystemManaged<SelectionTool>();
            if (_selectionTool != null)
            {
                m_ToolSystem.activeTool = _selectionTool;
            }
            else
            {
                log.Error("SelectionTool is not available.");
            }
        }

        private void MarkPrefabAsObsolete(PrefabBase prefab)
        {
            if (prefab != null)
            {
                prefab.name = "obsolete";
                prefab.Remove<PrefabBase>();
            }
        }

        private void UpdatePrefabSafe(PrefabSystem prefabSystem, PrefabBase prefab)
        {
            try
            {
                prefabSystem?.UpdatePrefab(prefab);
            }
            catch (Exception)
            {
                // Handle error if needed
            }
        }

        private void CleanUpReferences()
        {
            modUISystem = null;
            _prefabSystem = null;
            this.prefab = null;
            _OriginalPre = null;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_ForceCancel = false;
            HandleInputEvents();

            if (m_State == State.Adding || m_State == State.Removing)
            {
                if (c_ApplyAction.WasPressedThisFrame() || c_ApplyAction.WasReleasedThisFrame())
                {
                    return Apply(inputDeps);
                }

                if (m_ForceCancel || c_SecondaryApplyAction.WasPressedThisFrame() || c_SecondaryApplyAction.WasReleasedThisFrame())
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

            if (m_State != State.Rotating && c_SecondaryApplyAction.IsPressed())
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
                float3 mousePosition = InputManager.instance.mousePosition;
                if (mousePosition.x != m_RotationStartPosition.x)
                {
                    Rotation value2 = m_Rotation.value;
                    float angle = (mousePosition.x - m_RotationStartPosition.x) * ((float)Math.PI * 2f) * 0.002f;
                    value2.m_Rotation = math.normalizesafe(math.mul(m_StartRotation, quaternion.RotateY(angle)), quaternion.identity);
                    m_RotationModified = true;
                    value2.m_IsAligned = false;
                    m_Rotation.value = value2;
                }
            }

            if (prepareCameraMode && c_ApplyAction.WasPerformedThisFrame())
            {
                if (!isPlaceholderPaused)
                {
                    schedulePlaceholderPaused = true;
                    pauseTimer = 0f;
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
                return default;
            }

            return base.OnUpdate(inputDeps);
        }

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
        }

        private void EnableInputActions(bool enable)
        {
            if (enable)
            {
                restoreDefault.Enable();
                c_ApplyAction.Enable();
                c_SecondaryApplyAction.Enable();

                mirrorAction.shouldBeEnabled = true;
            }
            else
            {
                restoreDefault.Disable();
                c_ApplyAction.Disable();
                c_SecondaryApplyAction.Disable();

                mirrorAction.shouldBeEnabled = false;
            }
        }

        private void HandleInputEvents()
        {
            if (mirrorAction.WasPerformedThisFrame()) MirrorPrefab();
        }

        private void HandleSchedulePlaceholderPaused()
        {
            if (GetRaycastResult(out ControlPoint currentControlPoint))
            {
                if (lastControlPoint.Equals(default(ControlPoint)))
                {
                    lastControlPoint = currentControlPoint;
                    pauseTimer = 0f;
                }
                else
                {
                    float distance = math.distance(lastControlPoint.m_Position, currentControlPoint.m_Position);
                    if (distance < controlPointTolerance)
                    {
                        pauseTimer += SystemAPI.Time.DeltaTime;
                        if (pauseTimer >= pauseDelay)
                        {
                            isPlaceholderPaused = true;
                            schedulePlaceholderPaused = false;
                        }
                    }
                    else
                    {
                        lastControlPoint = currentControlPoint;
                        pauseTimer = 0f;
                    }
                }
            }
        }

        public void SavePrefab(string name, int category)
        {
            AssetSaveSystem.SavePrefab(EntityManager, _prefabSystem, this.assetStampPrefab, name, category);
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

            UpdatePrefabSafe(_prefabSystem, this.prefab);
            base.m_ForceUpdate = true;
        }

        public float invertedFloat(float original)
        {
            return original * -1;
        }
    }
}