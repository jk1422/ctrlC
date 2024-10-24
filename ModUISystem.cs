﻿using Colossal.Logging;
using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using ctrlC.Constants;
using ctrlC.Systems.AssetManagement;
using ctrlC.Tools;
using ctrlC.Tools.Selection;
using ctrlC.Utils;
using Game;
using Game.Prefabs;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace ctrlC
{
    public partial class ModUISystem : UISystemBase
    {
        #region Logger

        private static readonly ILog Log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(ModUISystem)}").SetShowsErrorsInUI(false);

        #endregion Logger

        // Tools and Systems
        private SelectionTool selectionTool;

        private PlacementTool placementTool;
        private PrefabSystem prefabSystem;

        // Flags
        public bool SelectionToolEnabled { get; set; } = false;

        public bool PlacementToolEnabled { get; set; } = false;
        public bool CircleSelectionEnabled { get; set; } = false;

        // Selection Options
        public bool SelectAll { get; set; } = true;
        public bool SelectRoads { get; set; } = true;
        public bool SelectBuildings { get; set; } = true;
        public bool SelectTrees { get; set; } = true;
        public bool SelectProps { get; set; } = true;
        public bool SelectAreas { get; set; } = true;

        // Prefabs and Environment
        public List<PrefabBase> Prefabs { get; set; }
        public string EnvironmentString { get; set; } = PathConstants.PrefabStorage;
        public bool UpdatePrefabs { get; set; } = false;
        public string PrefabCategoriesString = "";

        public bool ShowPrefabMenu { get; set; } = false;
        public int refreshSignal { get; set; } = 0;

        public static List<InputAction> conflictingInputs = new List<InputAction>();
        private InputAction _CBtn;

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            if (mode == GameMode.Game || mode == GameMode.Editor)
            {
                prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
                ctrlCPrefabStorage.LoadAssetsToStorage();
                AddUpdateBinding(new GetterValueBinding<string>(Mod.MOD_NAME, UIBindingConstants.PREFAB_ENV, () => PrefabCategoriesString));
                Log.Info("created");
            }
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            if (mode == GameMode.Game)
            {
                CheckBindingsForCKey();
                _CBtn = new InputAction("cbtn", InputActionType.Button);
                _CBtn.AddBinding("<Keyboard>/C");
                _CBtn.Enable();
            }
        }

        private void CheckBindingsForCKey()
        {
            var inputActions = InputSystem.ListEnabledActions(); // Get all enabled actions
            conflictingInputs.Clear();
            foreach (var action in inputActions)
            {
                foreach (var binding in action.bindings)
                {
                    if (binding.effectivePath.Contains("<Keyboard>/c"))  // Check if "C" is bound
                    {
                        if (action.name != "Open Mod Binding" && action.name != "cbtn")
                        {
                            conflictingInputs.Add(action);
                        }
                    }
                }
            }
        }

        private void DisableConflictingInputs()
        {
            foreach (var input in conflictingInputs)
            {
                if (input.enabled)
                {
                    input.Disable();
                }
            }
        }

        private void EnableConflictingInputs()
        {
            foreach (var input in conflictingInputs)
            {
                if (!input.enabled)
                {
                    input.Enable();
                }
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (selectionTool.Enabled || placementTool.Enabled)
            {
                if (_CBtn.WasPerformedThisFrame())
                {
                    log.Info("cbtn performed");
                    DisableConflictingInputs();
                }
                else if (_CBtn.WasReleasedThisFrame())
                {
                    log.Info("cbtn released");
                    EnableConflictingInputs();
                }
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            InitializeBindings();
            InitializeTools();
        }

        private void InitializeBindings()
        {
            try
            {
                // General Actions
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_PREFABMENU, TogglePrefabMenu));
                AddBinding(new TriggerBinding<string, int>(Mod.MOD_NAME, UIBindingConstants.ACTION_SAVE, Save));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.PREFABS_UPDATE_CALLBACK, ConfirmUpdate));
                AddBinding(new TriggerBinding<string>(Mod.MOD_NAME, UIBindingConstants.PREFAB_INSTANCIATE, InstantiatePrefab));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.ACTION_PMT_RESET, ResetPrefab));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.ACTION_PMT_MIRROR, MirrorPrefab));

                // Selection Tool Actions
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.SELECTION_TOOL_TOGGLE, ToggleSelectionTool));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_CIRCLE_SELECTION, ToggleCircleSelection));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_ALL, ToggleSelectAll));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_ROADS, () => ToggleSelectionOption(nameof(SelectRoads))));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_BUILDINGS, () => ToggleSelectionOption(nameof(SelectBuildings))));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_TREES, () => ToggleSelectionOption(nameof(SelectTrees))));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_PROPS, () => ToggleSelectionOption(nameof(SelectProps))));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_AREAS, () => ToggleSelectionOption(nameof(SelectAreas))));

                // Update Bindings
                AddUpdateBinding(new GetterValueBinding<List<List<string>>>(Mod.MOD_NAME, UIBindingConstants.PREFABS_GET, () => ctrlCPrefabStorage.PrefabList, new StringListWriter()));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SHOW_PREFABMENU, () => ShowPrefabMenu));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.PLACEMENT_TOOL_ENABLED, () => PlacementToolEnabled));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.PREFABS_UPDATE, () => UpdatePrefabs));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SELECTION_CIRCLE_ENABLED, () => CircleSelectionEnabled));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SELECTION_TOOL_ENABLED, () => SelectionToolEnabled));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_ALL, () => SelectAll));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_ROADS, () => SelectRoads));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_BUILDINGS, () => SelectBuildings));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_TREES, () => SelectTrees));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_PROPS, () => SelectProps));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_AREAS, () => SelectAreas));

                AddUpdateBinding(new GetterValueBinding<int>(Mod.MOD_NAME, "refreshSignal", () => refreshSignal));
            }
            catch (Exception ex)
            {
                Log.Error("ModUISystem: Error when adding bindings: " + ex.Message);
            }
        }

        private void InitializeTools()
        {
            selectionTool = World.GetOrCreateSystemManaged<SelectionTool>();
            placementTool = World.GetOrCreateSystemManaged<PlacementTool>();
        }

        public void InstantiatePrefab(string id)
        {
            var prefab = ctrlCPrefabStorage.PrefabDict[id];
            placementTool.ActivateTool(prefab as AssetStampPrefab);
        }

        public void ConfirmUpdate()
        {
            UpdatePrefabs = false;
        }

        public void Save(string name, int category)
        {
            try
            {
                placementTool.SavePrefab(name, category);
                ctrlCPrefabStorage.LoadAssetsToStorage();
                UpdatePrefabs = true;
                ++refreshSignal;
            }
            catch (Exception ex)
            {
                Log.Error($"Error when trying to save: {ex.Message}");
                throw;
            }
        }

        internal void StartMod()
        {
            try
            {
                if (!selectionTool.Enabled && !placementTool.Enabled)
                {
                    selectionTool.ToggleTool(true);
                    SelectionToolEnabled = true;
                    ShowPrefabMenu = Mod.AutoOpenPrefabMenu;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error when toggling tool: " + ex);
            }
        }

        internal void ToggleSelectionTool()
        {
            try
            {
                SelectionToolEnabled = !selectionTool.Enabled;
                selectionTool.ToggleTool(SelectionToolEnabled);
                ShowPrefabMenu = SelectionToolEnabled && Mod.AutoOpenPrefabMenu;
            }
            catch (Exception ex)
            {
                Log.Error("Error when toggling tool: " + ex);
            }
        }

        internal void TogglePrefabMenu()
        {
            ShowPrefabMenu = !ShowPrefabMenu;
        }

        public void SetToolEnabled(bool enabled)
        {
            SelectionToolEnabled = enabled;
        }

        public void ResetPrefab()
        {
            Entity entity = selectionTool.SelectedBuildings.FirstOrDefault();
            if (entity != null)
            {
                var data = EntityManager.GetComponentData<PrefabRef>(entity);
                var index = EntityManager.GetComponentData<PrefabData>(data);
            }
        }

        public void MirrorPrefab()
        {
            placementTool.MirrorPrefab();
        }

        public void ToggleCircleSelection()
        {
            log.Info($"Toggling the selectionMode..");
            selectionTool.ToggleSelectionMode();
            CircleSelectionEnabled = !selectionTool.standardToolMode;
            log.Info($"CircleEnabled is {CircleSelectionEnabled}");
        }

        public void ToggleSelectAll()
        {
            SelectAll = !SelectAll;
            SetAllSelectionOptions(SelectAll);
        }

        private void ToggleSelectionOption(string propertyName)
        {
            var property = GetType().GetProperty(propertyName);
            if (property != null && property.PropertyType == typeof(bool))
            {
                bool currentValue = (bool)property.GetValue(this);
                property.SetValue(this, !currentValue);
                CheckAll();
            }
        }

        public void CheckAll()
        {
            SelectAll = SelectRoads && SelectBuildings && SelectTrees && SelectProps && SelectAreas;
        }

        private void SetAllSelectionOptions(bool value)
        {
            SelectRoads = value;
            SelectBuildings = value;
            SelectTrees = value;
            SelectProps = value;
            SelectAreas = value;
        }
    }
}