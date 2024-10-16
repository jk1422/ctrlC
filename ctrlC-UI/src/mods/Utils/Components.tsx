import style from "mods/Style/main.module.scss";
import React, { FC } from 'react';

import { Button, Icon } from 'cs2/ui';

import { SelectionToolUI as ImportedSelectionToolUI } from 'mods/Utils/SelectionToolUI';
import { PlacementToolUI as ImportedPlacementToolUI } from 'mods/Utils/PlacementToolUI';
import { LEDSignSmall as ImportedLEDSignSmall, LEDSign as ImportedLEDSign } from 'mods/Utils/LEDSign';
import { LogoText as ImportedLogo, SelectionToolBG as ImportedSelectionToolBG} from 'mods/Utils/vector';



export const SelectionToolUI = ImportedSelectionToolUI;
export const PlacementToolUI = ImportedPlacementToolUI;
export const LEDSignSmall = ImportedLEDSignSmall;
export const LEDSign = ImportedLEDSign;
export const LogoText = ImportedLogo;
export const SelectionToolBG = ImportedSelectionToolBG;


interface buttonInterface {
    icon?: string;
    label?: string;
    state?: boolean;
    onClick?: () => void;
}

// FilterButton component, represents each filter option with an icon and selection state
export const ToolButton: FC<buttonInterface> = ({ icon, label, state, onClick }) => {
    return (
        <div className={style.filterWrapper}>
            <div className={style.toolButtonFrame}>
                <Button className={state ? style.toolButtonActive : style.toolButtonInactive} onClick={onClick}>
                    {icon && <Icon src={icon} className={style.icon}></Icon>}
                    {label && <label className={style.toolButtonLabel }>{ label }</label> }
                </Button>
            </div>
        </div>
    );
};




