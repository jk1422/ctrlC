using ctrlC.Systems.AssetManagement;
using ctrlC.Systems.UISystem;
using ctrlC.Tools;
using ctrlC.Tools.Selection;
using Game.Prefabs;

internal sealed class PrefabCommandHandler
{
    private readonly PrefabUiState _ui;
    private readonly PlacementTool _placementTool;
    private readonly SelectionTool _selectionTool;
    private readonly ThumbnailCameraTool _thumbnailCamera;

    public PrefabCommandHandler(PrefabUiState ui, PlacementTool placementTool, SelectionTool selectionTool, ThumbnailCameraTool thumbnailCamera)
    {
        _ui = ui;
        _placementTool = placementTool;
        _selectionTool = selectionTool;
        _thumbnailCamera = thumbnailCamera;
    }

    public void Instantiate(string id)
    {
        if (PrefabStorageSystem.TryGetPrefab(id, out var result))
        {
            _ui.SetSelected(result);
            _placementTool.ActivateTool(result.Prefab as AssetStampPrefab, true);
        }
    }

    public void DeleteSelected()
    {
        if (_ui.SelectedPrefab == null) return;

        if (PrefabStorageSystem.TryRemovePrefab(_ui.SelectedPrefab))
        {
            _ui.ResetSelected();
            _placementTool.DeactivateTool();
            _selectionTool.ToggleTool(true);
        }

        _ui.UpdatePrefabList();
    }

    public void Save(string id, string name, int category)
    {
        if (string.IsNullOrEmpty(id) || id == "0")
        {
            if (_placementTool.SavePrefab(name, category, out var result))
                _ui.SetSelected(result);

            _ui.UpdatePrefabList();
            return;
        }

        var newName = string.IsNullOrEmpty(name) ? _ui.SelectedPrefab?.Name : name;
        if (!string.IsNullOrEmpty(newName))
            PrefabStorageSystem.TryUpdatePrefab(id, newName, category);

        _ui.UpdatePrefabList();
    }

    public void EnableThumbnailCamera() => _thumbnailCamera.Enable(_ui.SelectedPrefab);
    public void TakePicture() => _thumbnailCamera.TakePhoto();
}
