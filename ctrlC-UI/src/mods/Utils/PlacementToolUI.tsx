import React, { useState, useEffect, FC, useCallback } from 'react';
import { bindValue, trigger } from 'cs2/api';
import mod from "mod.json";
import { UIBindingConstants } from "helpers/Bindings";
import style from 'mods/Style/main.module.scss'
import { ToolButton} from './Components';

import { Button } from 'cs2/ui';
import plc_mirror from "img/filterIcons/mirrorW.png";

const prefabCategories = bindValue<string>(
    mod.id,
    UIBindingConstants.PREFAB_ENV,
    "Category 1, Category 2, Category 3, Category 4"
);
interface LCDSaveMenuProps {
    inputValue: string;
    setInputValue: React.Dispatch<React.SetStateAction<string>>;
    selectedCategoryIndex: number;
    setSelectedCategoryIndex: React.Dispatch<React.SetStateAction<number>>;
}

export const LCDSaveMenu: React.FC<LCDSaveMenuProps> = ({
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
                <a className={style.LCDMessageTitle} >{ title}</a>
            </div>

            <a className={style.LCDCategoryInput} >{message}</a>
            <a className={style.LCDCategoryInput} ></a>
            <a className={style.LCDCategoryInput} ></a>
            <a className={style.LCDCategoryInput} ></a>
            <a className={style.LCDCategoryInput} ></a>
        </>
    );
}


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

    const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setInputValue(event.target.value);
    };

    return (
        <>
            <div className={style.toolRack}>
                <div className={style.toolButtonsRack}>
                    <ToolButton icon={plc_mirror} />
                </div>
            </div>

            <div className={style.saveMenu}>
                <div className={style.saveModule}>
                    <div className={style.LCDScreen}>
                        {showMessage ? <LCDSaveMessage type={'success'} message={"Prefab saved."} /> : <LCDSaveMenu
                            inputValue={inputValue}
                            setInputValue={setInputValue}
                            selectedCategoryIndex={selectedCategoryIndex}
                            setSelectedCategoryIndex={setSelectedCategoryIndex}
                        />}
                        
                    </div>
                    
                    <div className={style.saveButtonFrame}>
                        <Button className={style.saveButton}>
                            <label className={style.saveButtonLabel} onClick={click_save }>SAVE</label>
                        </Button>
                    </div>

                </div>
                
            </div>
        </>
    );
};