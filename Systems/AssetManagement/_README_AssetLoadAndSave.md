# Save and load assets
Here's a brief overview that explains the saving and loading process for ctrlC assets.

## Save Assets
The `SaveSystem` class provides functionality for saving prefabs and is pretty straight forward. 

When saving a prefab, the `SavePrefab` method ensures that each saved prefab has a unique name by appending a number to the name if one with the same name already exists. The prefab is saved in the folder `/.ctrlC~` with its name (`ctrlC_{name}`). It also creates metadata, including the name, description, and image path, and stores these details in the `CtrlCPrefabComponent` class.

The method also creates a placeholder thumbnail that will be used in the UI using the `CreateThumbnail` method. This thumbnail is saved to the prefab's directory for easy reference and for easily changing the thumbnail manually until we have the thumbnail camera(comming soon™).

## Load Assets
When saving an AssetStamp with ctrlC, the prefab is stored in the folder located at "AppData\LocalLow\Colossal Order\Cities Skylines II\.ctrlC~".

### Prevent The Game from loading our assets
If we were to let the game load the prefab automatically, it would throw an error. This is because ctrlC Prefabs have custom-made components that need to be declared and initialized first. To prevent the game from automatically loading the saved Prefabs before the mod is loaded and triggering an error, we prefix the folder name with a dot (.), making it hidden from the game’s normal loading process.

### Loading Custom Assets
Once the mod is loaded, the Asset Load System takes charge of loading the hidden prefabs. It begins by scanning for '.Prefab' files that start with "ctrlC" and adds them to an internal list. Next, a coroutine is initiated to register these assets in the AssetDatabase, followed by another coroutine that loads them as PrefabBase objects, integrating them into the PrefabSystem. This ensures that the custom prefabs are properly loaded and usable within the game.

To improve performance, the loading process is done asynchronously, ensuring that the game does not experience significant lags while the assets are being loaded.
