using Colossal.Logging;
using ctrlC.AssetManagement;
using Game.Input;
using Game.Prefabs;
using Game.Tools;
using System;
using Unity.Jobs;
using Unity.Mathematics;

namespace ctrlC.Tools
{
    public partial class PlacementTool : ObjectToolSystem
    {
        public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(PlacementTool)}").SetShowsErrorsInUI(false);
        ModUISystem _modUISystem;
        public ProxyAction mirrorAction;
        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = false;
            _modUISystem = World.GetOrCreateSystemManaged<ModUISystem>();
            
        }
        public void ActivateTool(AssetStampPrefab stampPrefab)
        {
            Enabled = true;
            if (TrySetPrefab(stampPrefab))
            {
                _modUISystem.PlacementToolEnabled = true;
                m_ToolSystem.activeTool = this;
                base.mode = Mode.Stamp;
            }
            else
            {
                log.Warn("Cannot set prefab.");
                Enabled = false;
            }
        }
        public void SavePrefab(string name, int category)
        {
            AssetSaveSystem.SavePrefab(EntityManager, m_PrefabSystem, this.GetPrefab() as AssetStampPrefab, name, category);
        }
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            if(mirrorAction != Mod.m_MirrorAction) mirrorAction = Mod.m_MirrorAction;
            mirrorAction.shouldBeEnabled = true;
        }
        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            mirrorAction.shouldBeEnabled = false;
            _modUISystem.PlacementToolEnabled = false;
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

            UpdatePrefabSafe(this.prefab);
            base.m_ForceUpdate = true;
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (mirrorAction.WasPerformedThisFrame()) MirrorPrefab();
            return base.OnUpdate(inputDeps);
        }

        public float invertedFloat(float original)
        {
            return original * -1;
        }
        private void UpdatePrefabSafe(PrefabBase prefab)
        {
            try
            {
                m_PrefabSystem?.UpdatePrefab(prefab);
            }
            catch (Exception)
            {
                // Handle error if needed
            }
        }
    }
}
