import React, { useState, useEffect, FC, useCallback } from 'react';
import mod from "mod.json";
import { UIBindingConstants } from "helpers/Bindings"
import { bindValue, trigger, useValue } from 'cs2/api';
import style from "mods/Style/prefabMenu.module.scss";
import { Icon, Button, FloatingButton, Panel } from "cs2/ui";
import { getModule, ModuleRegistryExtend } from "cs2/modding";

import { Theme } from "cs2/bindings";

import closeIcon from "img/filterIcons/closeW.png"
import backpackIcon from "img/C.png"
import editIcon from "img/pen.png"
import deleteIcon from "img/delete.png"
import logo from "img/LogoVar2.png"
const ToolBarButtonTheme: Theme | any = getModule(
    "game-ui/game/components/toolbar/components/feature-button/toolbar-feature-button.module.scss",
    "classes"
);
// Getting the vanilla theme css for compatibility
const ToolBarTheme: Theme | any = getModule("game-ui/game/components/toolbar/toolbar.module.scss", "classes");




interface PrefabData {
    name: string;
    id: string;
    description: string;
    imagePath: string;
    category: string;
}

const prefabs = bindValue<string[][]>(
    mod.id,
    UIBindingConstants.PREFABS_GET,
    []
);
const prefabCategories = bindValue<string>(
    mod.id,
    UIBindingConstants.PREFAB_ENV,
    "Category 1, Category 2, Category 3, Category 4"
);
const showPrefabMenu = bindValue<boolean>(
    mod.id,
    UIBindingConstants.SHOW_PREFABMENU,
    false
);

const refreshSignal = bindValue<number>(
    mod.id,
    "refreshSignal",
    0
)


export const PrefabMenu: FC = () => {
    const [prefabID, getPrefabID] = useState('');
    const [prefabList, setPrefabList] = useState<PrefabData[]>([]);
    const [selectedCategoryIndex, setSelectedCategoryIndex] = useState(0);
    const [categories, setCategories] = useState<string[]>([]);
    const refreshSignalValue = useValue(refreshSignal);
    // Uppdateringsfunktion
    const updatePrefabList = () => {
        try {
            if (prefabs) {
                const formattedPrefabs = prefabs.value.map((prefab) => {
                    if (prefab && prefab.length >= 5) {
                        return {
                            id: prefab[0],
                            name: prefab[1],
                            description: prefab[2],
                            imagePath: prefab[3],
                            category: prefab[4] // category är en sträng som representerar ett index
                        };
                    } else {
                        return null;
                    }
                }).filter(prefab => prefab !== null) as PrefabData[];
                setPrefabList(formattedPrefabs);
            }
        } catch (error) {
            console.error("Failed to update prefab list", error);
        }
    };
    useEffect(() => {
        if (prefabCategories.value && prefabCategories.value.length > 0) {
            const categoriesArray = prefabCategories.value.split(", ");
            setCategories(categoriesArray);
        } else {
            setCategories(["error 1", "error 2", "error 3", "error 4"]); // Fallback om inga kategorier finns
        }
    }, [prefabCategories.value]);

    useEffect(() => {
        updatePrefabList();
    }, [refreshSignalValue]);

    const handleCategoryChange = (index: number) => {
        setSelectedCategoryIndex(index);
    };
    const handleClick = (id: string) => {
        trigger(mod.id, UIBindingConstants.PREFAB_INSTANCIATE, id);
    };

    const closeMenu = () => {
        trigger(mod.id, UIBindingConstants.TOGGLE_PREFABMENU);
    };
    const filteredPrefabList = prefabList.filter(prefab => parseInt(prefab.category, 10) === selectedCategoryIndex);



    const menuVisible = useValue(showPrefabMenu);

    return (
        <>
            {menuVisible && <div className={style.PrefabMenu}>
                <div className={style.header}>
                    <div className={style.headerRow}>
                        <Icon src={logo} className={style.prefabMenuLogo}></Icon>

                        <Button src={closeIcon} className={style.closeButton} onClick={closeMenu}>
                        
                        </Button>
                    </div>
                    <div className={style.categoryList}>
                        {categories.map((category, index) => (
                            <Button
                                key={category}
                                className={`${style.categoryButton} ${selectedCategoryIndex === index ? style.categoryBtnActive : ''}`}
                                onClick={() => handleCategoryChange(index)} // Använd index för att hålla reda på kategori
                            >
                                {category}
                            </Button>
                        ))}
                    </div>
                </div>
                <Panel className={style.body}>
                    <div className={style.ItemRow}>
                        <div className={style.ItemGrid}>
                            {filteredPrefabList.map((prefab, index) => (
                                <div >
                                    <a id={prefab.id} key={index} className={style.ItemBox} onClick={() => handleClick(prefab.id)}>
                                        <div className={style.ImageHolder}>
                                            <img src={prefab.imagePath} alt={prefab.name} className={style.Thumbnail} />
                                        </div>
                                        <div className={style.ItemBoxInfo}>
                                            <h5 className={style.ItemTitle}>{prefab.name}</h5>
                                        </div>
                                    </a>
                                </div>
                            ))}
                        </div>
                    </div>
                </Panel>
            </div>}
        </>

    );
};

export const PrefabMenuButton: ModuleRegistryExtend = (Component) => {
    return (props) => {
        const { children, ...otherProps } = props || {};

        const handleClick = () => {
            trigger(mod.id, UIBindingConstants.TOGGLE_PREFABMENU);
        };

        return (
            <>
                <Button src={backpackIcon} className={ToolBarButtonTheme.button} variant="round" onClick={handleClick}></Button>
                <div className={ToolBarTheme.divider}></div>
                <Component {...otherProps}></Component>
            </>
        );
    }
}