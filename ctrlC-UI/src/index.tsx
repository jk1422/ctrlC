import { ModRegistrar } from "cs2/modding";
import { UIRoot } from "mods/ModUI"
import { PrefabMenu, PrefabMenuButton } from "mods/PrefabMenu"
import { CameraUIRoot } from "mods/ThumbnailCameraUI"


const register: ModRegistrar = (moduleRegistry) => {

    console.log("Registering ModUI modules...");

    //TODO: Make sure this only shows if correct version of the game.........
    moduleRegistry.append('GameTopLeft', PrefabMenu);
    console.log("Prefabmenu registered...");

    moduleRegistry.append('GameTopLeft', UIRoot); // <-- Causes crash
    console.log("UIRoot registered...");

    moduleRegistry.append('Game', CameraUIRoot);
    console.log("CameraUIRoot registered...");

    moduleRegistry.extend("game-ui/game/components/toolbar/top/toggles.tsx", "PhotoModeToggle", PrefabMenuButton);
    console.log("PrefabMenuButton registered...");
}

export default register;