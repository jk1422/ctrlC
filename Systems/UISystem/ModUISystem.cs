using Colossal.Logging;
using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using ctrlC.Constants;
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
using static ctrlC.Tools.Selection.SelectionTool;

namespace ctrlC.Systems.UISystem
{
    public partial class ModUISystem : UISystemBase
    {
        private static new readonly ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(ModUISystem)}").SetShowsErrorsInUI(false);


        // Tools and Systems
        private SelectionTool selectionTool; 

        private PlacementTool placementTool;

        private ThumbnailCameraTool thumbnailCamera;

        // Flags
        public bool SelectionToolEnabled { get; set; } = false;

        public bool PlacementToolEnabled { get; set; } = false;
        public bool CircleSelectionEnabled { get; set; } = false;


        // selected prefab
        public bool IsSavedPrefab { get; set; } = false;


        public bool UpdatePrefabs { get; set; } = false;

        public string PrefabCategoriesString = "";


        public readonly PrefabUiState _ui = new();
        private readonly InputConflictGuard _conflictGuard = new();

        private PrefabCommandHandler _commandHandler;


        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            if (mode == GameMode.Game || mode == GameMode.Editor)
            {
                _ui.UpdatePrefabList();
                AddUpdateBinding(new GetterValueBinding<string>(Mod.MOD_NAME, UIBindingConstants.PREFAB_ENV, () => PrefabCategoriesString));
                log.Info("created");
            }
        }



        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            if (mode == GameMode.Game)
            {
                _conflictGuard.Initialize();
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            _conflictGuard.Tick(selectionTool.Enabled || placementTool.Enabled);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            InitializeTools();
        
            _commandHandler = new PrefabCommandHandler(_ui, placementTool, selectionTool, thumbnailCamera);
            
            InitializeBindings();
        }

        protected override void OnStopRunning()
        {
            _conflictGuard.Dispose();
            base.OnStopRunning();
        }
        protected override void OnDestroy()
        {
            _conflictGuard.Dispose();
            base.OnDestroy();
        }

        private void InitializeBindings()
        {
            try
            {
                // General Actions
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_PREFABMENU, TogglePrefabMenu));
                AddBinding(new TriggerBinding<string, string, int>(Mod.MOD_NAME, UIBindingConstants.ACTION_SAVE, _commandHandler.Save));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.PREFABS_UPDATE_CALLBACK, ConfirmUpdate));
                AddBinding(new TriggerBinding<string>(Mod.MOD_NAME, UIBindingConstants.PREFAB_INSTANCIATE, _commandHandler.Instantiate));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.ACTION_PMT_RESET, ResetPrefab));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.ACTION_PMT_MIRROR, MirrorPrefab));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, "Delete Prefab", _commandHandler.DeleteSelected));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, "Enable Thumbnail Camera", _commandHandler.EnableThumbnailCamera));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, "Take picture", _commandHandler.TakePicture));

                // Selection Tool Actions
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.SELECTION_TOOL_TOGGLE, ToggleSelectionTool));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_CIRCLE_SELECTION, ToggleCircleSelection));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_ALL, selectionTool.Filters.SetAll));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_ROADS, () => selectionTool.Filters.Toggle(SelectableFilters.Road)));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_BUILDINGS, () => selectionTool.Filters.Toggle(SelectableFilters.Building)));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_TREES, () => selectionTool.Filters.Toggle(SelectableFilters.Tree)));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_PROPS, () => selectionTool.Filters.Toggle(SelectableFilters.Prop)));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_AREAS, () => selectionTool.Filters.Toggle(SelectableFilters.Area)));

                // Update Bindings
                AddUpdateBinding(new GetterValueBinding<List<List<string>>>(Mod.MOD_NAME, UIBindingConstants.PREFABS_GET, () => _ui.Prefabs, new ListListStringWriter()));
                AddUpdateBinding(new GetterValueBinding<List<string>>(Mod.MOD_NAME, "Get Selected Prefab", () => _ui.SelectedPrefabStringified, new ListStringWriter()));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SHOW_PREFABMENU, () => _ui.ShowPrefabMenu));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, "Show Camera UI", () => _ui.ShowCameraUI));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.PLACEMENT_TOOL_ENABLED, () => PlacementToolEnabled));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.PREFABS_UPDATE, () => UpdatePrefabs));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SELECTION_CIRCLE_ENABLED, () => CircleSelectionEnabled));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SELECTION_TOOL_ENABLED, () => SelectionToolEnabled));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_ALL, () => selectionTool.Filters.All));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_ROADS, () => selectionTool.Filters.Roads));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_BUILDINGS, () => selectionTool.Filters.Buildings));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_TREES, () => selectionTool.Filters.Trees));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_PROPS, () => selectionTool.Filters.Props));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_AREAS, () => selectionTool.Filters.Areas));

                AddUpdateBinding(new GetterValueBinding<int>(Mod.MOD_NAME, "refreshSignal", () => _ui.PrefabListRefreshSignal));
                AddUpdateBinding(new GetterValueBinding<int>(Mod.MOD_NAME, "Selected refreshSignal", () => _ui.SelectedPrefabRefreshSignal));

            }
            catch (Exception ex)
            {
                log.Error("ModUISystem: Error when adding bindings: " + ex.Message);
            }
        }
        
        private void InitializeTools()
        {
            selectionTool = World.GetOrCreateSystemManaged<SelectionTool>();
            placementTool = World.GetOrCreateSystemManaged<PlacementTool>();
            thumbnailCamera = World.GetOrCreateSystemManaged<ThumbnailCameraTool>();
        }

        public void ConfirmUpdate()
        {
            UpdatePrefabs = false;
        }
        
        internal void StartMod()
        {
            try
            {
                if (!selectionTool.Enabled && !placementTool.Enabled)
                {
                    selectionTool.ToggleTool(true);
                    SelectionToolEnabled = true;
                    _ui.ShowPrefabMenu = Mod.AutoOpenPrefabMenu;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error when toggling tool: " + ex);
            }
        }

        internal void ToggleSelectionTool()
        {
            try
            {
                SelectionToolEnabled = !selectionTool.Enabled;
                selectionTool.ToggleTool(SelectionToolEnabled);
                _ui.ShowPrefabMenu = SelectionToolEnabled && Mod.AutoOpenPrefabMenu;
            }
            catch (Exception ex)
            {
                log.Error("Error when toggling tool: " + ex);
            }
        }

        internal void TogglePrefabMenu()
        {
            _ui.ShowPrefabMenu = !_ui.ShowPrefabMenu;
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
            selectionTool.ToggleSelectionMode();
            CircleSelectionEnabled = !selectionTool.standardToolMode;
        }
    }
}