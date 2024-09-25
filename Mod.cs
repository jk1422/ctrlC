using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Common;
using ctrlC.Rendering;
using ctrlC.Systems;
using ctrlC.Tools.Selection;
using ctrlC.Tools;
using Game;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using Game.UI.Menu;
using Unity.Entities;
using UnityEngine;
using Game.UI;
using static Colossal.Win32.ProcessCommandLine;
using static Game.Tools.ValidationSystem;
using UnityEngine.InputSystem;
using ctrlC.Data;

namespace ctrlC
{
    //[SceneFlow] [ERROR]  Error when initializing prefab: ctrlC_new2 System.ArgumentNullException: Value cannot be null.
    //  Parameter name: key
    //    at System.Collections.Generic.Dictionary`2[TKey, TValue].FindEntry(TKey key) [0x00008] in <467a840a914a47078e4ae9b0b1e8779e>:0 
    //at System.Collections.Generic.Dictionary`2[TKey, TValue].TryGetValue (TKey key, TValue& value) [0x00000] in <467a840a914a47078e4ae9b0b1e8779e>:0 
    //at Game.Prefabs.PrefabSystem.IsUnlockable(Game.Prefabs.PrefabBase prefab) [0x00000] in <90828bfc0c9f40fd836083aaf60516d9>:0 
    //at Game.Prefabs.UnlockableBase.DefaultLateInitialize(Unity.Entities.EntityManager entityManager, Unity.Entities.Entity entity, System.Collections.Generic.List`1[T] dependencies) [0x00023] in <90828bfc0c9f40fd836083aaf60516d9>:0 
    //at Game.Prefabs.PrefabInitializeSystem.LateInitializePrefab(Unity.Entities.Entity entity, Game.Prefabs.PrefabBase prefab, System.Collections.Generic.List`1[T] dependencies, System.Collections.Generic.List`1[T] components) [0x000b1] in <90828bfc0c9f40fd836083aaf60516d9>:0 

    public class Mod : IMod
    {
        public const string MOD_NAME = nameof(ctrlC);

        private static NotificationUISystem _notificationUISystem;
        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        private Setting m_Setting;
        public static ProxyAction m_ButtonAction;
        public static ProxyAction m_AxisAction;
        public static ProxyAction m_VectorAction;

        public const string kButtonActionName = "ButtonBinding";
        public const string kAxisActionName = "FloatBinding";
        public const string kVectorActionName = "Vector2Binding";

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                log.Info($"Current mod asset at {asset.path}");
                EnvironmentConstants.ModPath = asset.path.Replace("ctrlC.dll", "");
                log.Info($"Environment ModPath set to: {EnvironmentConstants.ModPath}");
            }



            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

            m_Setting.RegisterKeyBindings();

            m_ButtonAction = m_Setting.GetAction(kButtonActionName);
            m_AxisAction = m_Setting.GetAction(kAxisActionName);
            m_VectorAction = m_Setting.GetAction(kVectorActionName);
            //hello
            m_ButtonAction.shouldBeEnabled = true;
            m_AxisAction.shouldBeEnabled = true;
            m_VectorAction.shouldBeEnabled = true;

            m_ButtonAction.onInteraction += (_, phase) => log.Info($"[{m_ButtonAction.name}] On{phase} {m_ButtonAction.ReadValue<float>()}");
            m_AxisAction.onInteraction += (_, phase) => log.Info($"[{m_AxisAction.name}] On{phase} {m_AxisAction.ReadValue<float>()}");
            m_VectorAction.onInteraction += (_, phase) => log.Info($"[{m_VectorAction.name}] On{phase} {m_VectorAction.ReadValue<Vector2>()}");

            AssetDatabase.global.LoadSettings(nameof(ctrlC), m_Setting, new Setting(this));

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

        private void OpenLink()
        {
            Application.OpenURL("https://jk142.se/downloads");
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
