using Colossal.IO.AssetDatabase;
using Colossal.Logging;
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
using Unity.Entities;
using UnityEngine;

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
        public static ProxyAction m_RaiseAction;
        public static ProxyAction m_FlattenAction;

        public const string kOpenModActionName = "Open Mod Binding";
        public const string kCopyActionName = "Copy Binding";
        public const string kMirrorActionName = "Mirror Binding";
        public const string kRaiseActionName = "Raise Binding";
        public const string kFlattenActionName = "Flatten Binding";

        public static string[] PrefabCategories = new string[4];

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                EnvironmentConstants.ModPath = asset.path.Replace("ctrlC.dll", "");
                log.Info($"Environment ModPath set to: {EnvironmentConstants.ModPath}");
            }

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));
            m_Setting.RegisterKeyBindings();

            AssetDatabase.global.LoadSettings(nameof(ctrlC), m_Setting, new Setting(this));

            m_ModUISystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ModUISystem>();
            ReadCategoryNames(m_Setting.Category1Name, m_Setting.Category2Name, m_Setting.Category3Name, m_Setting.Category4Name);
            SetActions();

            OnCreateWorld(updateSystem);
            
            _notificationUISystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NotificationUISystem>();

            //var apmNotLoadedNotification = _notificationUISystem.AddOrUpdateNotification(
            //   $"ctrlCOutDated",
            //   title: "ctrlC: New update available!",
            //   text: "Click here to get latest update",
            //   progressState: ProgressState.None,
            //   progress: 0,
            //
            //
            //   onClicked: OpenLink,
            //   thumbnail: ""
            //);
        }

        public void SetActions()
        {
            m_OpenModAction = m_Setting.GetAction(kOpenModActionName);
            m_CopyAction = m_Setting.GetAction(kCopyActionName);
            m_MirrorAction = m_Setting.GetAction(kMirrorActionName);
            m_RaiseAction = m_Setting.GetAction(kRaiseActionName);
            m_FlattenAction = m_Setting.GetAction(kFlattenActionName);

            m_OpenModAction.shouldBeEnabled = true;
            m_OpenModAction.onInteraction += (_, phase) => m_ModUISystem.StartMod();
        }
        
        public static void ReadCategoryNames(string cat1, string cat2, string cat3, string cat4)
        {
            PrefabCategories[0] = cat1;
            PrefabCategories[1] = cat2;
            PrefabCategories[2] = cat3;
            PrefabCategories[3] = cat4;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ModUISystem>().PrefabCategoriesStringed = string.Join(", ", PrefabCategories);
        }

        public void OnCreateWorld(UpdateSystem updateSystem)
        {
            updateSystem.UpdateAt<ModUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<StampPlacementTool>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ctrlC.Systems.ctrlC>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SelectionTool>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<AssetLoadSystem>(SystemUpdatePhase.Modification1);
            updateSystem.UpdateAt<CircleOverlaySystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<CustomOTS>(SystemUpdatePhase.ApplyTool);
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
