import React, { useState, useEffect, FC, useCallback } from 'react';
import { bindValue, trigger, useValue } from 'cs2/api';
import mod from "mod.json";
import { UIBindingConstants } from "helpers/Bindings";
import style from 'mods/Style/PlacementUI.module.scss';
import { ToolButton } from './Components';
import { Button } from 'cs2/ui';
import plc_mirror from "img/filterIcons/mirrorW.png";

const prefabCategories = bindValue<string>(
    mod.id,
    UIBindingConstants.PREFAB_ENV,
    "Category 1, Category 2, Category 3, Category 4"
);

const isSavedPrefab = bindValue<boolean>(mod.id, "IsSavedPrefab", false);
const selected_ID = bindValue<string>(mod.id, "Selected ID", "");
const selected_Name = bindValue<string>(mod.id, "Selected Name", "Error lol");
const selected_Category = bindValue<number>(mod.id, "Selected Category", -1);

interface Prefab {
    ID: string;
    Name: string;
    Category: number;
    isSaved: boolean;
}

interface MenuItem {
    id: number;
    name: string;
    element: React.ReactNode;
    SetSubMenuIndex: React.Dispatch<React.SetStateAction<number>>;
}

interface LCDMenuInterface {
    SelectedPrefab: Prefab;
    SubMenu: MenuItem;
    StatusMessage: string;
    setStatusMessage: React.Dispatch<React.SetStateAction<string>>;
    setInputValue: React.Dispatch<React.SetStateAction<string>>;
}

interface LCDDeleteInterface {
    currentMenuIndex: number;
    setMenuIndex: React.Dispatch<React.SetStateAction<number>>;
}
interface LCDMainInterface {
    currentMenuIndex: number;
    setMenuIndex: React.Dispatch<React.SetStateAction<number>>;
}
interface LCDCategoryInterface {
    selectedCategoryIndex: number;
    setSelectedCategoryIndex: React.Dispatch<React.SetStateAction<number>>;
    setStatusMessage: React.Dispatch<React.SetStateAction<string>>;
}

export const LCDView: React.FC<LCDMenuInterface> = ({ SelectedPrefab, SubMenu, setInputValue, StatusMessage, setStatusMessage }) => {
    const [name, setName] = useState<string>("");

    useEffect(() => {
        setName(selected_Name.value);
    }, [useValue(selected_ID)]);

    const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setName(event.target.value);
        setInputValue(event.target.value);

        setStatusMessage("Unsaved changes");
    };

    return (
        <>
            <div className={style.parentContainer}>
                <div>
                    <div className={style.LCDInputGroup}>
                        <input
                            className={style.LCDTextInput}
                            value={name}
                            onChange={handleInputChange}
                        />
                    </div>

                    <div>{SubMenu.element}</div>
                </div>


                <div className={style.statusHolder}>
                    <p className={style.statusMessage}>{ StatusMessage}</p>
                </div>
            </div>
        </>
    );
};

export const SubMenu_DeleteConfirm: React.FC<LCDDeleteInterface> = ({ setMenuIndex }) => {

    const deletePrefab = useCallback(() => {
        trigger(mod.id, "Delete Prefab");
    }, []);
    const handleClick = (ShouldDelete: boolean) => {
        if (ShouldDelete) {
            deletePrefab();
        }
        else {
            setMenuIndex(0);
        }
    }

    return (
        <>
            <div className={style.LCDDialog}>
                <label className={style.LCDDialogLabel}>Are you sure?</label>
            </div>


            <button className={style.LCDMenuItem} onClick={() => handleClick(true)}>Yes</button>
            <button className={style.LCDMenuItem} onClick={() => handleClick(false)}>No</button>
        </>
    );
};

export const SubMenu_Main: React.FC<LCDMainInterface> = ({ currentMenuIndex, setMenuIndex}) => {
    const switchMenu = (target: number) => {
        setMenuIndex(target);
    };

    return (
        <>
            <button className={style.LCDMenuItem}>Thumbnail Camera</button>
            <button className={style.LCDMenuItem} onClick={() => switchMenu(1)}>
                Change Category
            </button>
            <button className={style.LCDMenuItem} onClick={() => switchMenu(2)}>Delete</button>
        </>
    );
};

export const SubMenu_Category: React.FC<LCDCategoryInterface> = ({
    selectedCategoryIndex,
    setSelectedCategoryIndex,
    setStatusMessage
}) => {
    const [categories, setCategories] = useState<string[]>([]);

    useEffect(() => {
        const value = prefabCategories.value;
        setCategories(value?.split(", ") || ["error 1", "error 2", "error 3", "error 4"]);
    }, [prefabCategories.value]);
    const handleChange = (index: number) => {
        setSelectedCategoryIndex(index)
        setStatusMessage("Unsaved changes");
    }
    return (
        <>
            {categories.map((category, index) => (
                <button
                    key={category}
                    className={`${style.LCDCategoryInput} ${selectedCategoryIndex === index ? style.LCDCategoryInputSelected : ""
                        }`}
                    onClick={() => handleChange(index)}
                >
                    {category}
                </button>
            ))}
        </>
    );
};

export const PlacementToolUI = () => {
    const [showMessage, setShowMessage] = useState(false);
    const [inputValue, setInputValue] = useState('');
    const [refreshSignal, setRefreshSignal] = useState(0);
    const [selectedCategoryIndex, setSelectedCategoryIndex] = useState(0);
    const [subMenuIndex, setSubMenuIndex] = useState(0);

    const [statusMessage, setStatusMessage] = useState('');

    useEffect(() => {
        setSubMenuIndex(isSavedPrefab.value ? 0 : 1);
    }, [isSavedPrefab.value]); 

    const subMenus = [
        { id: 0, name: "Main Menu", element: <SubMenu_Main currentMenuIndex={subMenuIndex} setMenuIndex={ setSubMenuIndex } />, SetSubMenuIndex: setSubMenuIndex },
        {
            id: 1,
            name: "Category Menu",
            element: (
                <SubMenu_Category
                    selectedCategoryIndex={selectedCategoryIndex}
                    setSelectedCategoryIndex={setSelectedCategoryIndex}
                    setStatusMessage={ setStatusMessage}
                />
            ),
            SetSubMenuIndex: setSubMenuIndex,
        },
        { id: 2, name: "Delete Confirmation", element: <SubMenu_DeleteConfirm currentMenuIndex={subMenuIndex} setMenuIndex={setSubMenuIndex} />, SetSubMenuIndex: setSubMenuIndex }
    ];

    const click_save = useCallback(() => {
        trigger(mod.id, UIBindingConstants.ACTION_SAVE, inputValue, selectedCategoryIndex);
        
        setShowMessage(true);
        setRefreshSignal((prev) => prev + 1);
        setStatusMessage('Prefab Saved');
    }, [inputValue, selectedCategoryIndex]);

    const click_placeTool_mirror = useCallback(() => {
        trigger(mod.id, UIBindingConstants.ACTION_PMT_MIRROR);
    }, []);

    return (
        <>
            <div className={style.toolRack}>
                <div className={style.toolButtonsRack}>
                    <ToolButton icon={plc_mirror} onClick={click_placeTool_mirror} />
                </div>
            </div>

            <div className={style.Module}>
                <div className={style.LCDScreen}>
                    <LCDView
                        SelectedPrefab={{
                            ID: "",
                            Name: inputValue,
                            Category: selectedCategoryIndex,
                            isSaved: false,
                        }}
                        StatusMessage={statusMessage}
                        setStatusMessage={ setStatusMessage}
                        SubMenu={subMenus[subMenuIndex]}
                        setInputValue={setInputValue}
                    />
                </div>

                <div className={style.ButtonRack}>
                    <div className={style.NavigationGroup}>
                        <Button className={style.Button} onClick={() => setSubMenuIndex((prev) => Math.max(0, prev - 1))}>
                            <label className={style.ButtonLabel}>&lt;</label>
                        </Button>

                        <Button
                            className={style.Button}
                            onClick={() => setSubMenuIndex((prev) => Math.min(subMenus.length - 1, prev + 1))}
                        >
                            <label className={style.ButtonLabel}>&gt;</label>
                        </Button>
                    </div>
                    <div className={style.ButtonFrame}>
                        <Button className={style.Button} onClick={click_save}>
                            <label className={style.ButtonLabel}>SAVE</label>
                        </Button>
                    </div>
                </div>
            </div>
        </>
    );
};
