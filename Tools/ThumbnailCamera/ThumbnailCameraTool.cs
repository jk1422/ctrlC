using Colossal.Logging;
using ctrlC.Systems.AssetManagement;
using Game;
using Game.Input;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;
using Game.UI.Menu;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using ctrlC.Systems.UISystem;

namespace ctrlC.Tools
{
    internal partial class ThumbnailCameraTool : ToolBaseSystem
    {
        public override string toolID => "ctrlC Thumbnail Camera";
        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(ThumbnailCameraTool)}").SetShowsErrorsInUI(false);

        public ProxyAction t_PhotoAction;

        private ToolSystem t_ToolSystem;

        private StorageObject t_StorageObject;

        private CameraUpdateSystem t_CameraUpdateSystem;

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            return false;
        }
        public override PrefabBase GetPrefab()
        {
            return null;
        }

        public void Enable(StorageObject obj)
        {
            log.Info($"Enable called with StorageObject: {obj?.PrefixedName}");
            this.t_StorageObject = obj;
            this.Enabled = true;

            if (t_ToolSystem == null) t_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();

            t_ToolSystem.activeTool = this;
        }
        public void Disable()
        {
            log.Info("Disable called, resetting state");
            this.t_StorageObject = null;

            this.Enabled = false;

            if (t_ToolSystem == null) t_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();

            t_ToolSystem.activeTool = World.GetOrCreateSystemManaged<DefaultToolSystem>();
        }
        protected override void OnCreate()
        {
            log.Info("Creating Thumbnail camera tool...");
            base.OnCreate();

            this.Enabled = false;

            t_PhotoAction = Mod.m_PhotoAction;
            t_CameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();

            log.Info("Thumbnail camera created");
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            log.Info("Thumbnail camera starting...");
            if (t_PhotoAction == null || t_PhotoAction != Mod.m_PhotoAction)
            {
                t_PhotoAction = Mod.m_PhotoAction;

                if (t_PhotoAction == null)
                {
                    log.Warn("t_PhotoAction is null");
                    Disable();
                    return;
                }
            }

            if (t_StorageObject == null)
            {
                log.Warn("Storage Object was null");
                Disable();
                return;
            }

            ModUISystem modUISystem = World.GetOrCreateSystemManaged<ModUISystem>();
            modUISystem._ui.ShowPrefabMenu = false;
            modUISystem._ui.ShowCameraUI = true;
            t_PhotoAction.shouldBeEnabled = true;
            log.Info("Thumbnail camera started with StorageObject: " + t_StorageObject.PrefixedName);
        }
        protected override void OnStopRunning()
        {
            log.Info("Thumbnail camera stopping...");

            t_PhotoAction.shouldBeEnabled = false;
            this.Enabled = false;
            ModUISystem modUISystem = World.GetOrCreateSystemManaged<ModUISystem>();
            modUISystem._ui.ShowPrefabMenu = true;
            modUISystem._ui.ShowCameraUI = false;
            base.OnStopRunning();
            log.Info("Thumbnail camera stopped");
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            log.Debug("OnUpdate called, checking PhotoAction state...");
            if (t_PhotoAction != null && t_PhotoAction.WasPerformedThisFrame())
            {
                log.Info("Photo action was performed");
                TakePhoto();
            }
            else
            {
                log.Debug("Photo action not performed this frame");
            }

            return base.OnUpdate(inputDeps);
        }

        public void TakePhoto()
        {
            log.Info("njfjjkkj gkk  k  hkfjkj hfkkhwhu yuyiyuu");

            if (t_StorageObject == null)
            {
                log.Error("Cannot take photo, StorageObject is null");
                return;
            }

            string thumbnailPath = t_StorageObject.ThumbnailPath;

            try
            {
                log.Info($"Creating RenderTexture for: {t_StorageObject.PrefixedName}");
                // Skapa en RenderTexture för att lagra bilden
                RenderTexture thumbnailTexture = ScreenCaptureHelper.CreateRenderTarget(t_StorageObject.PrefixedName, 540, 540);

                // Ställ in kameran och fånga bilden
                Camera activeCamera = t_CameraUpdateSystem.activeCamera;
                if (activeCamera == null)
                {
                    log.Error("No active camera found for thumbnail capture.");
                    return;
                }

                log.Info("Capturing screenshot...");
                ScreenCaptureHelper.CaptureScreenshot(activeCamera, thumbnailTexture, new MenuHelpers.SaveGamePreviewSettings
                {
                    stylized = false, // Använd standardinställningar utan stilisering
                    stylizedRadius = 0.0f // Ingen suddighet eller extra effekter
                });

                // Konvertera RenderTexture till Texture2D och spara som PNG
                RenderTexture.active = thumbnailTexture;
                Texture2D texture = new Texture2D(thumbnailTexture.width, thumbnailTexture.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, thumbnailTexture.width, thumbnailTexture.height), 0, 0);
                texture.Apply();

                log.Info("Saving thumbnail to path: " + thumbnailPath);
                byte[] imageBytes = texture.EncodeToPNG();
                System.IO.File.WriteAllBytes(thumbnailPath, imageBytes);
                System.IO.File.WriteAllBytes(t_StorageObject.CouiThumbnailPath, imageBytes);

                log.Info("Thumbnail saved to " + thumbnailPath);

                // Rensa resurser
                RenderTexture.active = null;
                UnityEngine.Object.Destroy(thumbnailTexture);
                UnityEngine.Object.Destroy(texture);

                World.GetOrCreateSystemManaged<ModUISystem>()._ui.UpdatePrefabList();

                Disable();
            }
            catch (Exception ex)
            {
                log.Error("Failed to take thumbnail photo: " + ex.Message);
            }
        }
    }
}
