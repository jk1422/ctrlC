import React, { useState, useEffect, FC, useCallback } from 'react';
import { Icon, Button, ConfirmationDialog, Panel, Portal, FloatingButton, PanelSection, PanelSectionRow, FormattedParagraphs, PanelFoldout, Dropdown, DropdownItem, DropdownToggle, FormattedText } from "cs2/ui";
import icon from "img/ctrlC_icon.svg";

export const UIMenu: FC = () => {
    return (
        <div>
            <Panel>
                <PanelSection>
                hej
                </PanelSection>

                <PanelSection>
                        <div >
                            <FloatingButton></FloatingButton>
                            <FloatingButton></FloatingButton>
                            <FloatingButton></FloatingButton>
                            <FloatingButton></FloatingButton>
                            <FloatingButton></FloatingButton>
                            <FloatingButton></FloatingButton>
                        </div>
                </PanelSection>
            </Panel>
        </div>
    );
}

export const UIButton: FC = () => {

    const [shouldMenuBeVisible, setShouldMenuBeVisible] = useState(false);
    const toggleMenu = () => {

        setShouldMenuBeVisible(!shouldMenuBeVisible)
    };

    return (

        <div>
            <div style={{ position: "relative", pointerEvents: 'auto' }}>
                <FloatingButton src={icon} onClick={toggleMenu}></FloatingButton>
                {shouldMenuBeVisible && <UIMenu/>}
            </div>
        </div>

    );
}