using ctrlC.Systems.AssetManagement;
using System.Collections.Generic;

namespace ctrlC.Systems.UISystem
{
    public sealed class PrefabUiState
    {
        public bool ShowPrefabMenu;
        public bool ShowCameraUI;

        public List<List<string>> Prefabs = new();
        public int PrefabListRefreshSignal { get; set; }

        public StorageObject SelectedPrefab;
        public List<string> SelectedPrefabStringified = new() { "0", "standard", "", "" };
        public int SelectedPrefabRefreshSignal { get; set; }

        public void SetSelected(StorageObject prefab)
        {
            SelectedPrefab = prefab;
            SelectedPrefabStringified.Clear();
            SelectedPrefabStringified.AddRange(prefab.GetStringified());
            SelectedPrefabRefreshSignal++;
        }

        public void ResetSelected()
        {
            SelectedPrefab = null;
            SelectedPrefabStringified.Clear();
            SelectedPrefabStringified.AddRange(new[] { "0", "", "", "" });
            SelectedPrefabRefreshSignal++;
        }

        public void UpdatePrefabList()
        {
            Prefabs = PrefabStorageSystem.GetStringifiedPrefabs();
            PrefabListRefreshSignal++;
        }
    }
}
