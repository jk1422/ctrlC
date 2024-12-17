import React, { useState, useEffect, FC, useCallback } from 'react';
import { bindValue, trigger, useValue } from 'cs2/api';
import mod from "mod.json";
import { UIBindingConstants } from "helpers/Bindings";
import style from 'mods/Style/main.module.scss'
import { ToolButton } from './Components';

import { Button } from 'cs2/ui';
import plc_mirror from "img/filterIcons/mirrorW.png";

const prefabCategories = bindValue<string>(
    mod.id,
    UIBindingConstants.PREFAB_ENV,
    "Category 1, Category 2, Category 3, Category 4"
);

const isSavedPrefab = bindValue<boolean>(
    mod.id,
    "IsSavedPrefab",
    false
);

const selected_ID = bindValue<string>(
    mod.id,
    "Selected ID",
    ""
);
const selected_Name = bindValue<string>(
    mod.id,
    "Selected Name",
    "Error lol"
);
const selected_Category = bindValue<number>(
    mod.id,
    "Selected Category",
    -1
);

interface LCDSaveMenuProps {
    inputValue: string;
    setInputValue: React.Dispatch<React.SetStateAction<string>>;
    selectedCategoryIndex: number;
    setSelectedCategoryIndex: React.Dispatch<React.SetStateAction<number>>;
}


export const LCD_SavedPrefab: React.FC = () => {

    const [name, setName] = useState(useValue(selected_Name));
    
    const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setName(event.target.value);
    };
    return (
        <>
            <div className={style.LCDInputGroup}>
                <input className={style.LCDTextInput2}
                    value={name}
                    onChange={handleInputChange}
                />
            </div>




        </>
    );
}
interface MenuItem {
    index: number;
    element: void;
}
interface SubMenuInterface {
    previousMenu: MenuItem;
    targetMenu: MenuItem;
}


export const SubMenu: React.FC<SubMenuInterface> = ({ previousMenu, targetMenu} ) => {

    return (
        <>

        </>
    )
}

export const MainMenu = () => {
    return(
        <>
            <button className={`${style.LCDPrefabMenuItem}`}> Thumbnail Camera</button>
            <button className={`${style.LCDPrefabMenuItem}`}> Change Category</button>
            <button className={`${style.LCDPrefabMenuItem}`}> Delete </button>
        </>
    )
}

export const CategoryMenu: React.FC<LCDSaveMenuProps> = ({
    inputValue,
    setInputValue,
    selectedCategoryIndex,
    setSelectedCategoryIndex,
}) => {
    const [categories, setCategories] = useState<string[]>([]);

    useEffect(() => {
        if (prefabCategories.value && prefabCategories.value.length > 0) {
            const categoriesArray = prefabCategories.value.split(", ");
            setCategories(categoriesArray);
        } else {
            setCategories(["error 1", "error 2", "error 3", "error 4"]); // Fallback om inga kategorier finns
        }
    }, [prefabCategories.value]);

    const handleCategoryChange = (index: number) => {
        setSelectedCategoryIndex(index); // Uppdaterar valt kategoriindex
    };
    return (
        <>
            {categories.map((category, index) => (
                <button
                    key={category}
                    className={`${style.LCDCategoryInput} ${selectedCategoryIndex === index ? style.LCDCategoryInputSelected : ''}`}
                    onClick={() => handleCategoryChange(index)} // Använd index för att hålla reda på kategori
                >
                    {category}
                </button>
            ))}
        </>
    )
}

export const LCD_Default: React.FC<LCDSaveMenuProps> = ({
    inputValue,
    setInputValue,
    selectedCategoryIndex,
    setSelectedCategoryIndex,
}) => {

    const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setInputValue(event.target.value);
    };
    return (
        <>
            <div className={style.LCDInputGroup}>
                <label className={style.LCDTextInputLabel}>NAME:</label>
                <input className={style.LCDTextInput}
                    value={inputValue}
                    onChange={handleInputChange}
                />
            </div>
            <CategoryMenu
                inputValue={inputValue}
                setInputValue={setInputValue}
                selectedCategoryIndex={selectedCategoryIndex}
                setSelectedCategoryIndex={setSelectedCategoryIndex}
            />
        </>
    );
}

interface message {
    type: 'info' | 'success' | 'error'
    message: string;
}

export const LCDSaveMessage: FC<message> = ({ type, message }) => {

    const title: string = type.toUpperCase();
    return (
        <>
            <div className={style.LCDInputGroup}>
                <a className={style.LCDMessageTitle} >{title}</a>
            </div>

            <a className={style.LCDCategoryInput} >{message}</a>
            <a className={style.LCDCategoryInput} ></a>
            <a className={style.LCDCategoryInput} ></a>
            <a className={style.LCDCategoryInput} ></a>
            <a className={style.LCDCategoryInput} ></a>
        </>
    );
}

const LCDView: React.FC<{ isSaved: boolean } & LCDSaveMenuProps> = ({ isSaved, ...props }) => {

    console.log(`lcd view: isSaved = ${isSaved}`);
    return isSaved ? (
        <LCD_SavedPrefab />
    ) : (
        <LCD_Default {...props} />
    );
};

export const PlacementToolUI = () => {
    const [showMessage, setShowMessage] = useState(false);
    const [inputValue, setInputValue] = useState('');
    const [refreshSignal, setRefreshSignal] = useState(0);
    const [selectedCategoryIndex, setSelectedCategoryIndex] = useState(0);

    const click_save = useCallback(() => {
        trigger(mod.id, UIBindingConstants.ACTION_SAVE, inputValue, selectedCategoryIndex); // Använd selectedCategoryIndex
        setShowMessage(prev => !prev);
        setRefreshSignal(prev => prev + 1);
    }, [inputValue, selectedCategoryIndex]);
    const click_placeTool_mirror = useCallback(() => { trigger(mod.id, UIBindingConstants.ACTION_PMT_MIRROR); }, []);
    const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setInputValue(event.target.value);
    };

    return (
        <>
            <div className={style.toolRack}>
                <div className={style.toolButtonsRack}>
                    <ToolButton icon={plc_mirror} onClick={click_placeTool_mirror} />
                </div>
            </div>

            <div className={style.saveMenu}>
                <div className={style.saveModule}>
                    <div className={style.LCDScreen}>
                        {showMessage ? <LCDSaveMessage type={'success'} message={"Prefab saved."} /> : <LCDView
                            isSaved={useValue(isSavedPrefab)}
                            inputValue={inputValue}
                            setInputValue={setInputValue}
                            selectedCategoryIndex={selectedCategoryIndex}
                            setSelectedCategoryIndex={setSelectedCategoryIndex}
                        />}

                    </div>

                    <div className={style.saveButtonFrame}>
                        <Button className={style.saveButton}>
                            <label className={style.saveButtonLabel} onClick={click_save}>SAVE</label>
                        </Button>
                        <Button className={style.saveButton}>
                            <label className={style.saveButtonLabel} onClick={click_save}>SAVE</label>
                        </Button>
                    </div>

                </div>

            </div>
        </>
    );
};
