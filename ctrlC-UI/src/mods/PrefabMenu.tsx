import React, { useState, useEffect, FC, useCallback} from 'react';
import {  Panel } from "cs2/ui";
import { bindValue, trigger } from 'cs2/api';
import mod from "../../mod.json";
import { UIBindingConstants } from "../helpers/Bindings";
import styles from "./PrefabMenu.module.scss";
import thumbnailPlaceholder from "img/prefabThumbnail.png";


interface PrefabData {
    name: string;
    id: string;
    description: string;
    imagePath: string;
    category: string;
}

interface PrefabMenuProps {
    refreshSignal: number;
    onCategoryChange: (index: number) => void; // Lägg till prop för callback
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

export const PrefabMenu: FC<PrefabMenuProps> = ({ refreshSignal, onCategoryChange }) => {
    const [prefabID, getPrefabID] = useState('');
    const [prefabList, setPrefabList] = useState<PrefabData[]>([]);
    const [selectedCategoryIndex, setSelectedCategoryIndex] = useState(0); // Sparar kategori som index
    const [categories, setCategories] = useState<string[]>([]); 

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
    // Uppdatera kategorilistan baserat på prefabCategories när komponenten laddas eller vid förändring
    useEffect(() => {
        if (prefabCategories.value && prefabCategories.value.length > 0) {
            const categoriesArray = prefabCategories.value.split(", ");
            setCategories(categoriesArray);
        } else {
            setCategories(["error 1", "error 2", "error 3", "error 4"]); // Fallback om inga kategorier finns
        }
    }, [prefabCategories.value]);


    // Uppdatera listan vid komponentmontering och varje gång refreshSignal ändras
    useEffect(() => {
        updatePrefabList();
    }, [refreshSignal]);

    // Hantera kategoriändring
    const handleCategoryChange = (index: number) => {
        setSelectedCategoryIndex(index); // Uppdaterar valt kategoriindex
        onCategoryChange(index); // Anropa callback för att meddela ModUI.tsx om ändringen
    };

    const handleClick = (id: string) => {
        trigger(mod.id, UIBindingConstants.PREFAB_INSTANCIATE, id);
    };

    // Filtrera prefabs baserat på valt kategoriindex
    const filteredPrefabList = prefabList.filter(prefab => parseInt(prefab.category) === selectedCategoryIndex);

    return (
        <Panel className={styles.MainPanel}>
            <div className={styles.CategoryPanel}>
                <div className={styles.CategoryButtons}>
                    <img src={thumbnailPlaceholder} className={styles.Hidden} hidden></img>
                    {categories.map((category, index) => (
                        <button
                            key={category}
                            className={`${styles.CategoryButton} ${selectedCategoryIndex === index ? styles.CategoryButtonSelected : ''}`}
                            onClick={() => handleCategoryChange(index)} // Använd index för att hålla reda på kategori
                        >
                            {category}
                        </button>
                    ))}
                </div>
            </div>
            <div className={styles.Body}>
                <div className={styles.ItemRow}>
                    <div className={styles.ItemGrid}>
                        {filteredPrefabList.map((prefab, index) => (
                            <a id={prefab.id} key={index} className={styles.ItemBox} onClick={() => handleClick(prefab.id)}>
                                <div className={styles.ImageHolder}>
                                    <img src={prefab.imagePath} alt={prefab.name} className={styles.Thumbnail} />
                                    
                                </div>
                                <div className={styles.ItemBoxInfo}>
                                    <h5 className={styles.ItemTitle}>{prefab.name}</h5>
                                </div>
                            </a>
                        ))}
                    </div>
                </div>
            </div>
        </Panel>
    );
};

export default PrefabMenu;


