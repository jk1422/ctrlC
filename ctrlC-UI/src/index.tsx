import { ModRegistrar } from "cs2/modding";
import { UIRoot } from "mods/NewUI"
import { UIRoot as old } from 'mods/ModUI'
import { PrefabMenu, PrefabMenuButton } from "mods/Utils/SavedPrefabMenu"


const register: ModRegistrar = (moduleRegistry) => {
    moduleRegistry.append('GameTopLeft', PrefabMenu);
    moduleRegistry.append('GameTopLeft', UIRoot);
    moduleRegistry.extend("game-ui/game/components/toolbar/top/toggles.tsx", "PhotoModeToggle", PrefabMenuButton);
}

export default register;