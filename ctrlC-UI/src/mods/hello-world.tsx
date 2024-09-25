import React, { useState, useEffect, FC, useCallback } from 'react';
import { bindValue, useValue, trigger } from 'cs2/api';
import { Icon, Button, ConfirmationDialog, Panel, Portal, FloatingButton, PanelSection, PanelSectionRow, FormattedParagraphs, PanelFoldout, Dropdown, DropdownItem, DropdownToggle, FormattedText } from "cs2/ui";
import icon from "img/ctrlC_icon.svg";
import mod from "../../mod.json";
import styles from "./ModUI.module.scss";
import { UIBindingConstants } from "../helpers/Bindings"
interface CustomMenuButtonProps {
    toggleMenu: () => void;
}
const CustomMenuButton: FC<CustomMenuButtonProps> = ({ toggleMenu }) => (
    <FloatingButton className={styles.mainButton} src={icon} onClick={toggleMenu}>
    </FloatingButton >
);


const placementToolEnabled = bindValue<boolean>(
    mod.id,
    UIBindingConstants.PLACEMENT_TOOL_ENABLED,
    false
);
const selectionToolEnabled = bindValue<boolean>(
    mod.id,
    UIBindingConstants.SELECTION_TOOL_ENABLED,
    false
);
const OnScreenUI: FC = () => {



    return (
        <div>
            <Panel className={styles.panel}>
                Hello Worlddddddsss

            </Panel>

        </div>
    )
}
export const HelloWorldComponent: FC = () => {
    // Selection Tool triggers
    const click_sct_tool_toggle = useCallback(() => { trigger(mod.id, UIBindingConstants.SELECTION_TOOL_TOGGLE); }, []);
    const [shouldMenuBeVisible, setShouldMenuBeVisible] = useState(false);

    // Use values from bindings
    const selectionToolEnabledValue = useValue(selectionToolEnabled);
    const placementToolEnabledValue = useValue(placementToolEnabled);



    // Update `shouldMenuBeVisible` based on tool states
    useEffect(() => {
        setShouldMenuBeVisible(selectionToolEnabledValue || placementToolEnabledValue);
    }, [selectionToolEnabledValue, placementToolEnabledValue]);


    const toggleMenu = () => {

        click_sct_tool_toggle();
    };
    return (

        <div>
            <div style={{ position: "relative", pointerEvents: 'auto' }}>
                <CustomMenuButton toggleMenu={toggleMenu} />
                {shouldMenuBeVisible && <OnScreenUI />}

                <div className={styles.PrefabMenu}>

                </div>
            </div>
        </div>

    );
}