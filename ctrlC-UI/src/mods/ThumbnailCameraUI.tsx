import React, { useState, useEffect, FC, useCallback } from 'react';
import mod from "../../mod.json";
import { bindValue, useValue, trigger } from 'cs2/api';
import style from "mods/Style/main.module.scss";
import camera from 'img/camera.png';
import { Icon, Button, FloatingButton, Panel, Tooltip } from "cs2/ui";
const showCameraUI = bindValue<boolean>(
    mod.id,
    "Show Camera UI",
    false
);

export const CameraUI: FC = () => {

    const cameraClick = () => {
        console.log("It's fucking working yay");
        trigger(mod.id, "Take picture");
    }
    
    return (
        <div className={style.CameraViewContainer}>
            <div className={style.CameraViewContentContainer}>
                <div className={style.CameraButtonRack}>
                    <button className={style.CameraButton} onClick={cameraClick} > <img className={style.cameraIcon } src={ camera}></img> </button>

                </div>
            </div>
        </div>
    )
}

export const CameraUIRoot: FC = () => {
    return (
        <>
            {useValue(showCameraUI) && <CameraUI/> }
        </>
    );
}