import React, { useState, useEffect, FC, useCallback } from 'react';
import style from "mods/Style/main.module.scss";
import { Icon, Button, FloatingButton } from "cs2/ui";
import { bindValue, useValue, trigger } from 'cs2/api';

import mod from "../../mod.json";
import { UIBindingConstants } from "../helpers/Bindings";
import thumbnailPlaceholder from "img/prefabThumbnail.png";
import icon from "img/logo.svg";

import { LogoText as ImportedLogo} from 'mods/Utils/vector';
import { LEDSign as ImportedLEDSign } from 'mods/Utils/LEDSign';

import { SelectionToolUI as ImportedSelectionToolUI } from 'mods/Utils/SelectionToolUI';
import { PlacementToolUI as ImportedPlacementToolUI } from 'mods/Utils/PlacementToolUI2';


//Bindings values for checking tool state
const placementToolEnabled = bindValue<boolean>(mod.id,
    UIBindingConstants.PLACEMENT_TOOL_ENABLED,
    false
);
const selectionToolEnabled = bindValue<boolean>(
    mod.id,
    UIBindingConstants.SELECTION_TOOL_ENABLED,
    false
);


export const Menu: FC = () => {
    return (
        <div className={style.card}>
            <img src={thumbnailPlaceholder} className={style.Hidden} hidden></img>
            {/* Header section with logo and LED sign */}
            <div className={style.cardHeader}>
                <div className={style.row}>
                    <ImportedLogo />
                </div>
                <div className={style.row}>
                    <ImportedLEDSign /> {/* Krasch */}
                </div>
            </div>
            {/* Body section containing the Selection Tool UI */}
            <div className={style.cardBody}>
                <div className={style.bodyContent}>
                    {useValue(selectionToolEnabled) ? <ImportedSelectionToolUI /> : ""}
                    {useValue(placementToolEnabled) ? <ImportedPlacementToolUI /> : ""}
                </div>
            </div>
        </div>
    );
};


export const UIRoot: FC = () => {
    const click_sct_tool_toggle = useCallback(() => { trigger(mod.id, UIBindingConstants.SELECTION_TOOL_TOGGLE); }, []);
    const [shouldMenuBeVisible, setShouldMenuBeVisible] = useState(false);

    const selectionToolEnabledValue = useValue(selectionToolEnabled);
    const placementToolEnabledValue = useValue(placementToolEnabled);

    useEffect(() => {
        setShouldMenuBeVisible(selectionToolEnabledValue || placementToolEnabledValue);
    }, [selectionToolEnabledValue, placementToolEnabledValue]);
    // Function to toggle menu visibility state
    const toggleMenu = () => {
        click_sct_tool_toggle();
    };

    return (
        <div>
            <div style={{ position: "relative", pointerEvents: 'auto' }}>
                {/* Floating button to toggle the visibility of the menu */}
                <FloatingButton src={icon} onClick={toggleMenu} />
                {shouldMenuBeVisible && <Menu />}
            </div>
        </div>
    );
};