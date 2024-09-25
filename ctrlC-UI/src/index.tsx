import { ModRegistrar } from "cs2/modding";
import { HelloWorldComponent } from "mods/hello-world"
import { UIRoot } from "mods/ModUI"
import PrefabMenu from "mods/PrefabMenu"


const register: ModRegistrar = (moduleRegistry) => {

    moduleRegistry.append('GameTopLeft', UIRoot);
}

export default register;