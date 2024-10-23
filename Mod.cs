using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Common;
using ctrlC.AssetManagement;
using ctrlC.Data;
using ctrlC.Rendering;
using ctrlC.Systems;
using ctrlC.Tools;
using ctrlC.Tools.Selection;
using Game;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using Game.UI.Menu;
using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ctrlC
{
    public class Mod : IMod
    {
        public const string MOD_NAME = nameof(ctrlC);
        
        private static NotificationUISystem _notificationUISystem;
        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        internal Setting m_Setting;
        internal ModUISystem m_ModUISystem;

        public static ProxyAction m_OpenModAction;
        public static ProxyAction m_CopyAction;
        public static ProxyAction m_MirrorAction;


        public const string kOpenModActionName = "Open Mod Binding";
        public const string kCopyActionName = "Copy Binding";
        public const string kMirrorActionName = "Mirror Binding";

        public static bool AutoOpenPrefabMenu;
        public static string[] PrefabCategories = new string[4];

        private const string compatibleGameVersion = "1.1.10f1";
        private const bool devMode=false;

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));
            _notificationUISystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NotificationUISystem>();
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                EnvironmentConstants.ModPath = asset.path.Replace("ctrlC.dll", "");
                log.Info($"Environment ModPath set to: {EnvironmentConstants.ModPath}");
            }

            var currentGameVersion = Game.Version.current.shortVersion;
           

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));
            m_Setting.RegisterKeyBindings();
            AssetDatabase.global.LoadSettings(nameof(ctrlC), m_Setting, new Setting(this));

            if (currentGameVersion == compatibleGameVersion || devMode)
            {
                m_ModUISystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ModUISystem>();
                ReadCategoryNames(m_Setting.Category1Name, m_Setting.Category2Name, m_Setting.Category3Name, m_Setting.Category4Name);
                SetActions();
                OnCreateWorld(updateSystem);
                AssetLoadSystem.LoadCustomPrefabs();
            }
            else
            {
                log.Warn($"CtrlC is outdated! Current version of ctrlC is only compatible with game version '{compatibleGameVersion}' and the current game version is '{currentGameVersion}'");
                log.Info($"For more info, visit {EnvironmentConstants.XLink}");
                HandleOutdatedMod();
            }
        }

        void HandleOutdatedMod()
        {
            var apmNotLoadedNotification = _notificationUISystem.AddOrUpdateNotification(
               $"ctrlCOutDated",
               title: "CtrlC is outdated! ",
               text: "I will work on updating ctrlC ASAP. For mor info, click on me",
               progressState: ProgressState.None,
               progress: 0,
               onClicked: OpenLink,
               thumbnail: EnvironmentConstants.ModPath + "/images/C.png"
            );
        }

        void OpenLink()
        {
            Application.OpenURL(EnvironmentConstants.XLink);
        }

        public void SetActions()
        {
            m_OpenModAction = m_Setting.GetAction(kOpenModActionName);
            m_CopyAction = m_Setting.GetAction(kCopyActionName);
            m_MirrorAction = m_Setting.GetAction(kMirrorActionName);

            m_OpenModAction.shouldBeEnabled = true;
            m_OpenModAction.onInteraction += (_, phase) => StartMod();

            AutoOpenPrefabMenu = m_Setting.AutoOpenPrefabMenu;
        }
        private void StartMod()
        {
            m_ModUISystem.StartMod();
        }

        public static void ReadCategoryNames(string cat1, string cat2, string cat3, string cat4)
        {
            PrefabCategories[0] = cat1;
            PrefabCategories[1] = cat2;
            PrefabCategories[2] = cat3;
            PrefabCategories[3] = cat4;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ModUISystem>().PrefabCategoriesString = string.Join(", ", PrefabCategories);
        }

        public void OnCreateWorld(UpdateSystem updateSystem)
        {
            updateSystem.UpdateAt<ModUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<PlacementTool>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SelectionTool>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<CircleOverlaySystem>(SystemUpdatePhase.ToolUpdate);
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
        }
    }
}
