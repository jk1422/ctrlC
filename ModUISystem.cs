using Colossal.Logging;
using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using ctrlC.Components;
using ctrlC.Data;
using ctrlC.Tools;
using ctrlC.Tools.Selection;
using Game;
using Game.Debug.Tests;
using Game.Prefabs;
using Game.Serialization;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace ctrlC
{

    public class ListOfListStringWriter : IWriter<List<List<string>>>
    {
        public void Write(IJsonWriter writer, List<List<string>> value)
        {
            writer.ArrayBegin(value.Count);
            foreach (var list in value)
            {
                writer.ArrayBegin(list.Count);
                foreach (var item in list)
                {
                    writer.Write(item);
                }
                writer.ArrayEnd();
            }
            writer.ArrayEnd();
        }
    }

    public class StringArrayWriter : IWriter<string[]>
    {
        public void Write(IJsonWriter writer, string[] value)
        {
            writer.ArrayBegin(value.Length); // Börjar en array 
            foreach (var item in value)
            {
                writer.Write(item); // Skriver varje string
            }
            writer.ArrayEnd(); // Avslutar arrayen
        }
    }
    public partial class ModUISystem : UISystemBase, IPreDeserialize
    {
        public static ILog _log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(Mod)}").SetShowsErrorsInUI(false);


        private SelectionTool selectionTool;
        private StampPlacementTool stampPlacementTool;

        public bool sct_tool_enabled { get; set; } = false;
        public bool place_tool_enabled { get; set; } = false;
        public bool fullElevationStep { get; set; } = true;

        public bool sct_all { get; set; } = true;
        public bool sct_roads { get; set; } = true;
        public bool sct_buildings { get; set; } = true;
        public bool sct_trees { get; set; } = true;
        public bool sct_props { get; set; } = true;
        public bool sct_areas { get; set; } = true;
        private PrefabSystem _prefabSystem;
        public List<PrefabBase> prefabs { get; set; }

        public string environmentString { get; set; } = EnvironmentConstants.PrefabStorage;

        public bool UpdatePrefabs { get; set; } = false;
        public string PrefabCategoriesStringed = "";

        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            if (mode == GameMode.Game || mode == GameMode.Editor)
            {
                _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
                ctrlCPrefabStorage.LoadAssetsToStorage();
                AddUpdateBinding(new GetterValueBinding<string>(Mod.MOD_NAME, UIBindingConstants.PREFAB_ENV, () => PrefabCategoriesStringed));
            }
        }



        public void PreDeserialize(Context context)
        {

        }




        protected override void OnCreate()
        {
            base.OnCreate();
            try
            {
                // Mod Actions
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.PATREON_OPEN, openPatreon));
                AddBinding(new TriggerBinding<string, int>(Mod.MOD_NAME, UIBindingConstants.ACTION_SAVE, Save));

                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.PREFABS_UPDATE_CALLBACK, ConfirmUpdate));
                AddBinding(new TriggerBinding<string>(Mod.MOD_NAME, UIBindingConstants.PREFAB_INSTANCIATE, instanciatePrefab));

                // Placement Tool Actions
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_PMT_ELEVATION, toggleElevationStep));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.ACTION_PMT_RESET, resetPrefab));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.ACTION_PMT_MIRROR, mirrorPrefab));

                //Selection Tool Actions
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.SELECTION_TOOL_TOGGLE, OnSelectButtonClicked));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_ALL, toggleSCTAll));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_ROADS, toggleSCTRoads));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_BUILDINGS, toggleSCTBuildings));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_TREES, toggleSCTTrees));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_PROPS, toggleSCTProps));
                AddBinding(new TriggerBinding(Mod.MOD_NAME, UIBindingConstants.TOGGLE_SCT_AREAS, toggleSCTAreas));

                try
                {
                    AddUpdateBinding(new GetterValueBinding<List<List<string>>>(
                        Mod.MOD_NAME,
                        UIBindingConstants.PREFABS_GET,
                        () => ctrlCPrefabStorage._prefabList,
                        new ListOfListStringWriter()
                    ));


                }
                catch (Exception ex)
                {
                    _log.Error("ModUISysyem: Error when adding bindings: " + ex.Message);
                }

                // Placement tool senders
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.PMT_ELEVATION_FULL, () => fullElevationStep));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.PLACEMENT_TOOL_ENABLED, () => place_tool_enabled));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.PREFABS_UPDATE, () => UpdatePrefabs));
                // Selection Tool senders
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SELECTION_TOOL_ENABLED, () => sct_tool_enabled));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_ALL, () => sct_all));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_ROADS, () => sct_roads));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_BUILDINGS, () => sct_buildings));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_TREES, () => sct_trees));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_PROPS, () => sct_props));
                AddUpdateBinding(new GetterValueBinding<bool>(Mod.MOD_NAME, UIBindingConstants.SCT_AREAS, () => sct_areas));
                
                
            }
            catch (Exception ex)
            {
                _log.Error("ModUISysyem:  Error when adding bindings: " + ex.Message);
            }

            selectionTool = World.GetOrCreateSystemManaged<SelectionTool>();
            stampPlacementTool = World.GetOrCreateSystemManaged<StampPlacementTool>();
        }

        public void instanciatePrefab(string id)
        {

            var prefab = ctrlCPrefabStorage._prefabDict[id];
            stampPlacementTool.ActivateTool(prefab as AssetStampPrefab, _prefabSystem);
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
                new GetterValueBinding<List<List<string>>>(
                        Mod.MOD_NAME,
                        UIBindingConstants.PREFABS_GET,
                        () => ctrlCPrefabStorage._prefabList,
                        new ListOfListStringWriter()
                    );

                UpdatePrefabs = true;
            }
            catch (Exception ex)
            {

                log.Error($"Error when trying to save: {ex.Message}");
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
                    sct_tool_enabled = true;
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error when toggling tool: " + ex);
            }
        }
        internal void OnSelectButtonClicked()
        {
            try
            {
                if (selectionTool.Enabled)
                {
                    selectionTool.ToggleTool(false);
                    sct_tool_enabled = false;
                    //Set modbinding till true

                }
                else
                {
                    selectionTool.ToggleTool(true);
                    sct_tool_enabled = true;
                   //set modbinding till false

                }
            }
            catch (Exception ex)
            {
                _log.Error("Error when toggling tool: " + ex);
            }
        }

        public void setToolEnabled(bool enabled)
        {
            sct_tool_enabled = enabled;
        }

        public void toggleElevationStep()
        {
            fullElevationStep = !fullElevationStep;
        }

        public void resetPrefab()
        {
            Entity ent = selectionTool.SelectedBuildings.First<Entity>();
            if (ent != null)
            {
                var data = EntityManager.GetComponentData<PrefabRef>(ent);
                var mindex = EntityManager.GetComponentData<PrefabData>(data);
                
            }

        }
        public void openPatreon()
        {
            string url = "https://www.patreon.com/jk142/membership";
            Application.OpenURL(url);
        }
        public void mirrorPrefab()
        {
            stampPlacementTool.MirrorPrefab();
        }

        public void toggleSCTAll()
        {
            sct_all = !sct_all;
            if (sct_all)
            {
                sct_roads = true;
                sct_buildings = true;
                sct_trees = true;
                sct_props = true;
                sct_areas = true;
            }
            else
            {
                sct_roads = false;
                sct_buildings = false;
                sct_trees = false;
                sct_props = false;
                sct_areas = false;
            }
        }
        public void toggleSCTRoads()
        {
            sct_roads = !sct_roads;
            checkAll();
        }
        public void toggleSCTBuildings()
        {
            sct_buildings = !sct_buildings;
            checkAll();
        }
        public void toggleSCTTrees()
        {
            sct_trees = !sct_trees;
            checkAll();
        }
        public void toggleSCTProps()
        {
            sct_props = !sct_props;
            checkAll();
        }
        public void toggleSCTAreas()
        {
            sct_areas = !sct_areas;
            checkAll();
        }
        public void checkAll()
        {
            sct_all = sct_roads && sct_buildings && sct_trees && sct_props && sct_areas;
        }
    }
}