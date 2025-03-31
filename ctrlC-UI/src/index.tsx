import { ModRegistrar } from "cs2/modding";
import { UIRoot } from "mods/ModUI"
import { PrefabMenu, PrefabMenuButton } from "mods/PrefabMenu"
import { CameraUIRoot } from "mods/ThumbnailCameraUI"


const register: ModRegistrar = (moduleRegistry) => {
    //TODO: Make sure this only shows if correct version of the game.........
    moduleRegistry.append('GameTopLeft', PrefabMenu);
    moduleRegistry.append('GameTopLeft', UIRoot);
    moduleRegistry.append('Game', CameraUIRoot);
    moduleRegistry.extend("game-ui/game/components/toolbar/top/toggles.tsx", "PhotoModeToggle", PrefabMenuButton);
}

export default register;