using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Common;
using ctrlC.Constants;
using ctrlC.Rendering;
using ctrlC.Systems.AssetManagement;
using ctrlC.Tools;
using ctrlC.Tools.Selection;
using Game;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using Game.UI.Menu;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace ctrlC
{
    public class Mod : IMod
    {
        #region Logger

        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(Mod)}").SetShowsErrorsInUI(false);

        #endregion Logger

        public const string kCopyActionName = "Copy Binding";
        public const string kMirrorActionName = "Mirror Binding";
        public const string kOpenModActionName = "Open Mod Binding";
        public const string MOD_NAME = nameof(ctrlC);

        public static bool AutoOpenPrefabMenu;
        public static ProxyAction m_CopyAction;
        public static ProxyAction m_MirrorAction;
        public static ProxyAction m_OpenModAction;
        public static string[] PrefabCategories = new string[4];

        internal static NotificationUISystem _NotificationUISystem;
        internal static ModUISystem m_ModUISystem;
        internal static Setting m_Setting;

        private static readonly string[] compatibleGameVersions = { "1.1.10f1", "1.1.11f1" };
        private const bool devMode = false;


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
            updateSystem.UpdateAt<OverlayCircleRenderer>(SystemUpdatePhase.ToolUpdate);
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            m_Setting?.UnregisterInOptionsUI();
            m_Setting = null;
        }

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));
            _NotificationUISystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NotificationUISystem>();
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                PathConstants.ModPath = asset.path.Replace("ctrlC.dll", "");
                log.Info($"Environment ModPath set to: {PathConstants.ModPath}");
            }

            string currentGameVersion = Game.Version.current.shortVersion;

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));
            m_Setting.RegisterKeyBindings();
            AssetDatabase.global.LoadSettings(nameof(ctrlC), m_Setting, new Setting(this));

            if (compatibleGameVersions.Contains(currentGameVersion) || devMode)
            {
                m_ModUISystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ModUISystem>();
                ReadCategoryNames(m_Setting.Category1Name, m_Setting.Category2Name, m_Setting.Category3Name, m_Setting.Category4Name);
                SetActions();
                OnCreateWorld(updateSystem);
                AssetLoadSystem.LoadCustomPrefabs();
            }
            else
            {
                log.Warn($"CtrlC is outdated! Current version of ctrlC is only compatible with game versions '{string.Join(", ", compatibleGameVersions)}' and the current game version is '{currentGameVersion}'");
                log.Info($"For more info, visit {PathConstants.XLink}");
                HandleOutdatedMod();
            }
        }


        private void HandleOutdatedMod()
        {
            _NotificationUISystem.AddOrUpdateNotification(
                "ctrlCOutDated",
                title: "CtrlC is outdated! ",
                text: "I will work on updating ctrlC ASAP. For more info, click on me",
                progressState: ProgressState.None,
                progress: 0,
                onClicked: OpenLink,
                thumbnail: PathConstants.ModPath + "/.BuildContent/Images/C.png"
            );
        }

        private void OpenLink()
        {
            Application.OpenURL(PathConstants.XLink);
        }

        private void SetActions()
        {
            m_OpenModAction = m_Setting.GetAction(kOpenModActionName);
            m_CopyAction = m_Setting.GetAction(kCopyActionName);
            m_MirrorAction = m_Setting.GetAction(kMirrorActionName);

            m_OpenModAction.shouldBeEnabled = true;
            m_OpenModAction.onInteraction += (_, _) => StartMod();

            AutoOpenPrefabMenu = m_Setting.AutoOpenPrefabMenu;
        }

        private void StartMod()
        {
            m_ModUISystem.StartMod();
        }

    }
}