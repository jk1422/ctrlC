import mod from "../../../mod.json";
import style from "mods/Style/main.module.scss";
import React, { useState, useEffect, FC, useCallback } from 'react';
import { Icon, Button, FloatingButton } from "cs2/ui";
import { bindValue, useValue, trigger } from 'cs2/api';
import { UIBindingConstants } from "../../helpers/Bindings";

import { ToolButton, LEDSignSmall } from 'mods/Utils/Components'

import {SelectionToolBG } from 'mods/Utils/vector';

import sct_road from "img/filterIcons/roadW.png";
import sct_buildings from "img/filterIcons/buildingW.png";
import sct_trees from "img/filterIcons/treeW.png";
import sct_props from "img/filterIcons/propW.png";
import sct_area from "img/filterIcons/areaW_DMG.png";

const sct_CircleSelection = bindValue<boolean>(mod.id, UIBindingConstants.SELECTION_CIRCLE_ENABLED, false);
const sct_All = bindValue<boolean>(mod.id, UIBindingConstants.SCT_ALL, true);
const sct_Roads = bindValue<boolean>(mod.id, UIBindingConstants.SCT_ROADS, true);
const sct_Buildings = bindValue<boolean>(mod.id, UIBindingConstants.SCT_BUILDINGS, true);
const sct_Props = bindValue<boolean>(mod.id, UIBindingConstants.SCT_PROPS, true);
const sct_Trees = bindValue<boolean>(mod.id, UIBindingConstants.SCT_TREES, true);
const sct_Areas = bindValue<boolean>(mod.id, UIBindingConstants.SCT_AREAS, true);



export const SelectionToolUI = () => {

    // Callbacks to trigger different filter options
    const click_tgl_circle = useCallback(() => {
        trigger(mod.id, UIBindingConstants.TOGGLE_CIRCLE_SELECTION)
    }, []);
    const click_sct_all = useCallback(() => {
        trigger(mod.id, UIBindingConstants.TOGGLE_SCT_ALL);
    }, []);
    const click_sct_roads = useCallback(() => {
        trigger(mod.id, UIBindingConstants.TOGGLE_SCT_ROADS);
    }, []);
    const click_sct_buildings = useCallback(() => {
        trigger(mod.id, UIBindingConstants.TOGGLE_SCT_BUILDINGS);
    }, []);
    const click_sct_props = useCallback(() => {
        trigger(mod.id, UIBindingConstants.TOGGLE_SCT_PROPS);
    }, []);
    const click_sct_trees = useCallback(() => {
        trigger(mod.id, UIBindingConstants.TOGGLE_SCT_TREES);
    }, []);
    const click_sct_areas = useCallback(() => {
        trigger(mod.id, UIBindingConstants.TOGGLE_SCT_AREAS);
    }, []);



    return (
        <>
            <div className={style.selToolBGContainer}>
                <SelectionToolBG />
            </div>
            <div className={style.toolRack}>
                {/* Tool section for showing LED indicators and toggling between tools */}
                <div className={[style.col3, style.dFlex, style.ps].join(' ')}>
                    <div className={style.LEDSmall}>
                        <LEDSignSmall ray={!useValue(sct_CircleSelection)} />
                    </div>
                </div>
                <div className={[style.col3, style.dFlex].join(' ')}>
                    <div className={style.buttonGroup}>
                        {/* Buttons to toggle tool state between two modes */}
                        <button
                            className={`${style.button} ${style.leftButton} ${!useValue(sct_CircleSelection) ? style.active : ''}`}
                            onClick={click_tgl_circle}
                        />
                        <button
                            className={`${style.button} ${style.rightButton} ${useValue(sct_CircleSelection) ? style.active : ''}`}
                            onClick={click_tgl_circle}
                        />
                    </div>
                </div>
                <div className={[style.col3, style.dFlex, style.pe].join(' ')}>
                    <div className={style.LEDSmall}>
                        <LEDSignSmall circle={useValue(sct_CircleSelection)} />
                    </div>
                </div>
            </div>
            {/* Filter buttons for selecting different categories */}
            <div className={style.filterRack}>
                <div className={style.toolButtonsRack}>
                    <ToolButton icon="" state={useValue(sct_All)} onClick={click_sct_all} />
                    <ToolButton icon={sct_road} state={useValue(sct_Roads)} onClick={click_sct_roads} />
                    <ToolButton icon={sct_buildings} state={useValue(sct_Buildings)} onClick={click_sct_buildings} />
                    <ToolButton icon={sct_trees} state={useValue(sct_Trees)} onClick={click_sct_trees} />
                    <ToolButton icon={sct_props} state={useValue(sct_Props)} onClick={click_sct_props} />
                    <ToolButton icon={sct_area} state={useValue(sct_Areas)} onClick={click_sct_areas} />
                </div>
            </div>
        </>
    );
};