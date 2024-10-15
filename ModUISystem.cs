using Colossal.Logging;
using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using ctrlC.Data;
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

namespace ctrlC
{
    public partial class ModUISystem : UISystemBase
    {
        // Logger
        private static readonly ILog Log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(ModUISystem)}").SetShowsErrorsInUI(false);

        // Tools and Systems
        private SelectionTool selectionTool;
        private StampPlacementTool stampPlacementTool;
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
        public string EnvironmentString { get; set; } = EnvironmentConstants.PrefabStorage;
        public bool UpdatePrefabs { get; set; } = false;
        public string PrefabCategoriesString = "";

        public bool ShowPrefabMenu { get; set; } = false;
        public int refreshSignal { get; set; } = 0;

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            if (mode == GameMode.Game || mode == GameMode.Editor)
            {
                prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
                ctrlCPrefabStorage.LoadAssetsToStorage();
                AddUpdateBinding(new GetterValueBinding<string>(Mod.MOD_NAME, UIBindingConstants.PREFAB_ENV, () => PrefabCategoriesString));
                log.Info("created");
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
                AddUpdateBinding(new GetterValueBinding<List<List<string>>>(Mod.MOD_NAME, UIBindingConstants.PREFABS_GET, () => ctrlCPrefabStorage._prefabList, new StringListWriter()));
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
            stampPlacementTool = World.GetOrCreateSystemManaged<StampPlacementTool>();
        }

        public void InstantiatePrefab(string id)
        {
            var prefab = ctrlCPrefabStorage._prefabDict[id];
            stampPlacementTool.ActivateTool(prefab as AssetStampPrefab, prefabSystem);
        }

        public void ConfirmUpdate()
        {
            UpdatePrefabs = false;
        }

        public void Save(string name, int category)
        {
            try
            {
                stampPlacementTool.SavePrefab(name, category);
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
                if (!selectionTool.Enabled && !stampPlacementTool.Enabled)
                {
                    selectionTool.ToggleTool(true);
                    SelectionToolEnabled = true;
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
            stampPlacementTool.MirrorPrefab();
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