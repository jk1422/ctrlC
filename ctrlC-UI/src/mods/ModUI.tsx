import React, { useState, useEffect, FC, useCallback } from 'react';
import { LocalizedString, useLocalization } from "cs2/l10n";
import { Icon, Button, ConfirmationDialog, Panel, Portal, FloatingButton, PanelSection, PanelSectionRow, FormattedParagraphs, PanelFoldout, Dropdown, DropdownItem, DropdownToggle, FormattedText } from "cs2/ui";
import { bindValue, useValue,trigger } from 'cs2/api';
import mod from "../../mod.json";
import { UIBindingConstants } from "../helpers/Bindings"
import icon from "img/ctrlC_icon.svg";
import logo from "img/ctrlC_logo.png";
import jk142_logo from "img/jk142_2xBW.png"
import mirror from "img/mirror_icon.svg";
import reset from "img/reset_icon.svg";
import sct_road from "img/selection_tool/roads.svg";
import sct_buildings from "img/selection_tool/Buildings.svg";
import sct_trees from "img/selection_tool/trees.svg";
import sct_props from "img/selection_tool/props.svg";
import sct_area from "img/selection_tool/areas.svg";
import sct_all from "img/selection_tool/all.svg";
import styles from "./ModUI.module.scss";
import save from "img/save_icon.svg";
import PrefabMenu from "./PrefabMenu";



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

const fullElevation = bindValue<boolean>(
    mod.id,
    UIBindingConstants.PMT_ELEVATION_FULL,
    false
);

const sct_All = bindValue<boolean>(
    mod.id,
    UIBindingConstants.SCT_ALL,
    true
);
const sct_Roads = bindValue<boolean>(
    mod.id,
    UIBindingConstants.SCT_ROADS,
    true
);
const sct_Buildings = bindValue<boolean>(
    mod.id,
    UIBindingConstants.SCT_BUILDINGS,
    true
);
const sct_Props = bindValue<boolean>(
    mod.id,
    UIBindingConstants.SCT_PROPS,
    true
);
const sct_Trees = bindValue<boolean>(
    mod.id,
    UIBindingConstants.SCT_TREES,
    true
);
const sct_Areas = bindValue<boolean>(
    mod.id,
    UIBindingConstants.SCT_AREAS,
    true
);








const OnScreenUI: FC = () => {

    const [inputValue, setInputValue] = useState('');
    const [refreshSignal, setRefreshSignal] = useState(0);
    const [selectedCategoryIndex, setSelectedCategoryIndex] = useState(0);

    const updateCategoryIndex = (index: number) => {
        setSelectedCategoryIndex(index);
    };


    const click_save = useCallback(() => {
        trigger(mod.id, UIBindingConstants.ACTION_SAVE, inputValue, selectedCategoryIndex); // Använd selectedCategoryIndex
        setRefreshSignal(prev => prev + 1); // Uppdatera signalen efter att ha sparat
    }, [inputValue, selectedCategoryIndex]);

    // Mod triggers
    const click_patreon = useCallback(() => { trigger(mod.id, UIBindingConstants.PATREON_OPEN); }, []);
    const index = 2;

    // Selection Tool triggers
    const click_sct_tool_toggle = useCallback(() => { trigger(mod.id, UIBindingConstants.SELECTION_TOOL_TOGGLE); }, []);
    const click_sct_all = useCallback(() => { trigger(mod.id, UIBindingConstants.TOGGLE_SCT_ALL) }, []);
    const click_sct_roads = useCallback(() => { trigger(mod.id, UIBindingConstants.TOGGLE_SCT_ROADS) }, []);
    const click_sct_buildings = useCallback(() => { trigger(mod.id, UIBindingConstants.TOGGLE_SCT_BUILDINGS) }, []);
    const click_sct_props = useCallback(() => { trigger(mod.id, UIBindingConstants.TOGGLE_SCT_PROPS) }, []);
    const click_sct_trees = useCallback(() => { trigger(mod.id, UIBindingConstants.TOGGLE_SCT_TREES) }, []);
    const click_sct_areas = useCallback(() => { trigger(mod.id, UIBindingConstants.TOGGLE_SCT_AREAS) }, []);

    // Placement Tool triggers
    const click_placeTool_elevation_toggle = useCallback(() => { trigger(mod.id, UIBindingConstants.TOGGLE_PMT_ELEVATION); }, []);
    const click_placeTool_mirror = useCallback(() => { trigger(mod.id, UIBindingConstants.ACTION_PMT_MIRROR); }, []);
    const click_placeTool_reset = useCallback(() => { trigger(mod.id, UIBindingConstants.ACTION_PMT_RESET); }, []);


    const sct_elevation_full = useValue(fullElevation)
        ? `${styles.btn_toggle_fill_on}`
        : styles.btn_toggle_fill_off;
    const sct_all_buttonClass = useValue(sct_All)
        ? `${styles.btn_toolUtils}`
        : styles.btn_toolUtils_INACTIVE;
    const sct_roads_buttonClass = useValue(sct_Roads)
        ? `${styles.btn_toolUtils}`
        : styles.btn_toolUtils_INACTIVE;
    const sct_buildings_buttonClass = useValue(sct_Buildings)
        ? `${styles.btn_toolUtils}`
        : styles.btn_toolUtils_INACTIVE;
    const sct_props_buttonClass = useValue(sct_Props)
        ? `${styles.btn_toolUtils}`
        : styles.btn_toolUtils_INACTIVE;
    const sct_trees_buttonClass = useValue(sct_Trees)
        ? `${styles.btn_toolUtils}`
        : styles.btn_toolUtils_INACTIVE;
    const sct_areas_buttonClass = useValue(sct_Areas)
        ? `${styles.btn_toolUtils}`
        : styles.btn_toolUtils_INACTIVE;

    const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setInputValue(event.target.value);
    };


    return (
        <div>
            <Panel className={styles.panel}>
                <PanelSection className={styles.panel_panelSection_HEADER} >
                    <Icon className={styles.logo} src={logo}></Icon>
                    <Icon className={styles.jklogo} src={jk142_logo}></Icon>
                    

                </PanelSection>
                <PanelSection className={styles.panel_panelSection_BODY}>
                    <div className={styles.utils_holder}>
                        <FloatingButton src={sct_all} className={sct_all_buttonClass} onClick={click_sct_all}></FloatingButton>
                        <FloatingButton src={sct_road} className={sct_roads_buttonClass} onClick={click_sct_roads}></FloatingButton>
                        <FloatingButton src={sct_buildings} className={sct_buildings_buttonClass} onClick={click_sct_buildings}></FloatingButton>
                        <FloatingButton src={sct_props} className={sct_props_buttonClass} onClick={click_sct_props}></FloatingButton>
                        <FloatingButton src={sct_trees} className={sct_trees_buttonClass} onClick={click_sct_trees}></FloatingButton>
                        <FloatingButton src={sct_area} className={sct_areas_buttonClass} onClick={click_sct_areas}></FloatingButton>
                    </div>

                </PanelSection>

                <PanelSection className={styles.panel_panelSection_BODY}>
                    <div className={styles.utils_holder}>
                        <FloatingButton src={mirror} className={styles.btn_toolUtils} onClick={click_placeTool_mirror}></FloatingButton>
                        <FloatingButton src={reset} className={styles.btn_toolUtils} onClick={click_placeTool_reset}></FloatingButton>
                    </div>
                </PanelSection>
                <PanelFoldout header="Save">
                    <input
                        type="text"
                        placeholder="New Save"
                        value={inputValue}
                        onChange={handleInputChange}
                        className={styles.name_input}
                    />
                    <FloatingButton onClick={click_save} src={save} className={styles.save_btna}></FloatingButton>
                </PanelFoldout >
            </Panel>
            <PrefabMenu refreshSignal={refreshSignal} onCategoryChange={updateCategoryIndex} /> {/* Skicka callbacken */}
        </div>
    )
}


export const UIRoot: FC = () => {
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
            <div style={{ position: "relative",  pointerEvents: 'auto' }}>
                <CustomMenuButton toggleMenu={toggleMenu} />
                {shouldMenuBeVisible && <OnScreenUI />}
                
                <div className={styles.PrefabMenu}>
                    
                </div>
            </div>
        </div>
        
    );
}


export const ModButton = () => {
    const click_sct_tool_toggle = useCallback(() => { trigger(mod.id, UIBindingConstants.SELECTION_TOOL_TOGGLE); }, []);
    const menuVisible = useValue(selectionToolEnabled);
    const toggleMenu = () => {

        click_sct_tool_toggle();
    };
    return (
        <div>
            <div>
                <CustomMenuButton toggleMenu={toggleMenu} />
                {menuVisible && <OnScreenUI />}
            </div>
        </div>
    );
}

