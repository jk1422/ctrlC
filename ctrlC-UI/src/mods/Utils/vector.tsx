import React from 'react';
import style from "mods/Style/main.module.scss"

export const LogoText = () => {
  return (
    <>
          <svg id="Lager_1_kopia" data-name="Lager 1 kopia" viewBox="0 0 548.31 142.42" className={style.logoText} >
            <path d="M85.6,78.32v7.42a2.29,2.29,0,0,0,.06.64.33.33,0,0,0,.23.23,2.24,2.24,0,0,0,.64.06h43.84v42.45H44.3A8.62,8.62,0,0,1,42,127.38c-1.31-1.16-2.9-2.62-4.75-4.4s-3.68-3.58-5.46-5.4-3.26-3.38-4.46-4.7a8.31,8.31,0,0,1-1.8-2.32V54a8.31,8.31,0,0,1,1.8-2.32c1.2-1.32,2.68-2.88,4.46-4.7s3.6-3.62,5.46-5.4,3.44-3.24,4.75-4.4A8.62,8.62,0,0,1,44.3,35.4h86.07v42H86.53a2.24,2.24,0,0,0-.64.06.32.32,0,0,0-.23.23A2.24,2.24,0,0,0,85.6,78.32Z" className={style.LogoFill1} />
            <path d="M190.46,76.23v7.42a2.29,2.29,0,0,0,.06.64.33.33,0,0,0,.23.23,2.29,2.29,0,0,0,.64.06H219v44.54H158.44a8.62,8.62,0,0,1-2.32-1.74c-1.31-1.16-2.9-2.62-4.75-4.4s-3.68-3.58-5.45-5.4-3.27-3.38-4.47-4.7a8.31,8.31,0,0,1-1.8-2.32V11.73h53.59V35.4H219V75.3H191.39a2.29,2.29,0,0,0-.64.06.33.33,0,0,0-.23.23A2.29,2.29,0,0,0,190.46,76.23Z" className={style.LogoFill2} />
            <path d="M333.14,35.4v61h-51v32.71H228.27V35.4h42q3.84,7.53,6.27,12.41t6.26,12.41l6.5-12.41,6.5-12.41Z" className={style.LogoFill3} />
            <path d="M396,11.73V129.12h-34.8a8.3,8.3,0,0,1-2.32-1.74c-1.32-1.16-2.9-2.62-4.76-4.4s-3.67-3.58-5.45-5.4-3.27-3.38-4.47-4.7a9.32,9.32,0,0,1-1.79-2.28.08.08,0,0,1,0,0V11.73Z" className={style.LogoFill4} />
            <path d="M464.45,66.25v7.43a2.27,2.27,0,0,0,.05.63.36.36,0,0,0,.24.24,2.27,2.27,0,0,0,.63,0h44.78v54.52H424.08a8.46,8.46,0,0,1-2.32-1.74c-1.32-1.16-2.9-2.62-4.76-4.4s-3.67-3.58-5.45-5.4-3.27-3.38-4.47-4.7a8.15,8.15,0,0,1-1.79-2.32V30.29A8.46,8.46,0,0,1,407.08,28q1.8-2,4.47-4.7T417,17.88c1.86-1.78,3.44-3.25,4.76-4.41a8.46,8.46,0,0,1,2.32-1.74h86.07V65.32H465.37a2.16,2.16,0,0,0-.63.06.37.37,0,0,0-.24.23A2.4,2.4,0,0,0,464.45,66.25Z" className={style.LogoFill5} />
        </svg>
    </>
  );
}

export const SelectionToolBG = () => {
    return (
        <>

            <svg id="art" viewBox="0 0 249.24 123.87" className={style.selToolBG }>
                <g id="wires">
                    <circle cx="157.55" cy="9.81" r="2" className={style.wireCircle}/>
                    <polyline points="147.98 5.64 152.14 9.81 155.55 9.81" className={style.wireLines} />
                    <line x1="139.49" y1="11.81" x2="139.49" y2="6.06" className={style.wireLines} />
                    <line x1="143.43" y1="11.81" x2="143.43" y2="6.06" className={style.wireLines} />
                    <polyline points="151.46 21.09 161.14 21.09 166.46 15.78 166.46 6.06" className={style.wireLines} />     //
                    <polyline points="151.46 24.91 162.16 24.91 170.25 16.82 170.25 6.06" className={style.wireLines} />     // R from slt-btn to top
                    <polyline points="184.67 18.36 176.18 18.36 173.7 18.36 163.22 28.84 151.81 28.84" className={[style.wireLines].join(' ')} />  //
                    <polyline points="151.81 32.77 164.27 32.77 174.66 22.38 184.1 22.38" className={[style.wireLines].join(' ')} />               // R from slt-btn To O-LED
                    <line x1="203.26" y1="11.81" x2="203.26" y2="6.06" className={style.wireLines} />
                    <line x1="207.19" y1="11.81" x2="207.19" y2="6.06" className={style.wireLines} />
                    <line x1="211.12" y1="11.81" x2="211.12" y2="6.06" className={style.wireLines} />
                    <polyline points="114.89 49.84 114.89 53.27 113.43 54.73 36.26 54.72 26.3 64.68 26.3 73.95" className={style.wireLines} />
                    <polyline points="104.95 70.26 104.95 64.68 107.07 62.57 115.54 62.59 122.76 55.38 122.76 49.84" className={style.wireLines} />
                    <polyline points="65.63 73.5 65.63 64.68 71.67 58.64 114.49 58.65 118.83 54.32 118.83 49.84" className={style.wireLines} />
                    <polyline points="134.92 49.9 134.92 53.33 136.38 54.78 213.56 54.78 223.52 64.74 223.52 74.01" className={style.wireLines} />
                    <polyline points="144.86 70.32 144.86 64.74 142.75 62.63 134.27 62.65 127.06 55.43 127.06 49.9" className={style.wireLines} />
                    <polyline points="184.19 73.56 184.19 64.74 178.15 58.7 135.32 58.71 130.99 54.38 130.99 49.9" className={style.wireLines} />
                </g>
                <g id="pins">
                    <rect id="BTN_Con" x="98.23" y="23.93" width="5.9" height="1.97" className={style.wireCon} />
                    <rect id="BTN_Con-2" data-name="BTN_Con" x="98.23" y="27.29" width="5.9" height="1.97" className={style.wireCon} />
                    <g id="Connector_2pin"><rect id="BTN_Con_var2" x="181.13" y="17.16" width="5.94" height="2.62" rx="1" className={style.conPin} />
                        <rect id="BTN_Con_var2-2" data-name="BTN_Con_var2" x="181.13" y="21.09" width="5.94" height="2.62" rx="1" className={style.conPin} />
                        <rect id="DSPY_Con" x="183.94" y="16.34" width="30.98" height="8.57" className={style.DSPYCon} />
                        <polyline points="97.78 21.26 88.09 21.26 82.77 15.95 82.77 6.23" className={style.wireLines} />
                        <polyline points="97.78 25.08 87.07 25.08 78.98 16.99 78.98 6.23" className={style.wireLines} />
                        <polyline points="64.56 18.53 73.05 18.53 75.54 18.53 86.02 29.01 97.43 29.01" className={style.wireLines} />
                        <polyline points="97.43 32.94 84.96 32.94 74.57 22.55 65.13 22.55" className={style.wireLines} />
                        <line x1="45.97" y1="11.98" x2="45.97" y2="6.23" className={style.wireLines} />
                        <line x1="42.04" y1="11.98" x2="42.04" y2="6.23" className={style.wireLines} />
                        <line x1="38.11" y1="11.98" x2="38.11" y2="6.23" className={style.wireLines} />
                        <rect id="BTN_Con_var2-3" data-name="BTN_Con_var2" x="62.16" y="17.33" width="5.94" height="2.62" rx="1" transform="translate(130.26 37.28) rotate(180)" className={style.conPin} />
                        <rect id="BTN_Con_var2-4" data-name="BTN_Con_var2" x="62.16" y="21.26" width="5.94" height="2.62" rx="1" transform="translate(130.26 45.15) rotate(180)" className={style.conPin} />
                        <rect id="DSPY_Con-2" data-name="DSPY_Con" x="61.31" y="16.51" width="3.98" height="8.57" transform="translate(126.6 41.59) rotate(180)" className={style.DSPYCon} />
                        <rect id="BTN_Con_var2-5" data-name="BTN_Con_var2" x="35.14" y="11.64" width="5.94" height="2.62" rx="1" transform="translate(51.06 -25.16) rotate(90)" className={style.conPin} />
                        <rect id="BTN_Con_var2-6" data-name="BTN_Con_var2" x="39.07" y="11.64" width="5.94" height="2.62" rx="1" transform="translate(54.99 -29.09) rotate(90)" className={style.conPin} />
                        <rect id="BTN_Con_var2-7" data-name="BTN_Con_var2" x="43" y="11.64" width="5.94" height="2.62" rx="1" transform="translate(58.93 -33.02) rotate(90)" className={style.conPin} />
                        <rect id="DSPY_Con-3" data-name="DSPY_Con" x="40.93" y="6.34" width="2.24" height="15.47" transform="translate(56.12 -27.97) rotate(90)" className={style.DSPYCon} />
                        <rect id="BTN_Con_var2-8" data-name="BTN_Con_var2" x="92.7" y="19.83" width="5.94" height="2.62" rx="1" className={style.conPin} />
                        <rect id="BTN_Con_var2-9" data-name="BTN_Con_var2" x="92.7" y="23.77" width="5.94" height="2.62" rx="1" className={style.conPin} />
                        <rect id="BTN_Con_var2-10" data-name="BTN_Con_var2" x="92.7" y="27.7" width="5.94" height="2.62" rx="1" className={style.conPin} />
                        <rect id="BTN_Con_var2-11" data-name="BTN_Con_var2" x="92.7" y="31.63" width="5.94" height="2.62" rx="1" className={style.conPin} />
                        <rect id="DSPY_Con-4" data-name="DSPY_Con" x="95.67" y="17.35" width="15.81" height="19.4" className={style.DSPYCon} />
                    </g>
                    <g id="Connector_pin">
                        <rect id="BTN_Con_var2-12" data-name="BTN_Con_var2" x="181.21" y="69.35" width="5.94" height="2.62" rx="1" transform="translate(254.85 -113.52) rotate(90)" className={style.conPin} />
                        <rect id="DSPY_Con-5" data-name="DSPY_Con" x="177.54" y="73.7" width="13.29" height="4.28" transform="matrix(0, 1, -1, 0, 260.03, -108.35)" className={style.DSPYCon} />
                        <rect id="BTN_Con_var2-13" data-name="BTN_Con_var2" x="141.89" y="69.35" width="5.94" height="2.62" rx="1" transform="translate(215.52 -74.2) rotate(90)" className={style.conPin} />
                        <rect id="DSPY_Con-6" data-name="DSPY_Con" x="138.22" y="73.7" width="13.29" height="4.28" transform="matrix(0, 1, -1, 0, 220.7, -69.02)" className={style.DSPYCon} />
                        <rect id="BTN_Con_var2-14" data-name="BTN_Con_var2" x="101.98" y="69.35" width="5.94" height="2.62" rx="1" transform="translate(175.62 -34.29) rotate(90)" className={style.conPin} />
                        <rect id="DSPY_Con-7" data-name="DSPY_Con" x="98.31" y="73.7" width="13.29" height="4.28" transform="matrix(0, 1, -1, 0, 180.79, -29.12)" className={style.DSPYCon} />
                        <rect id="BTN_Con_var2-15" data-name="BTN_Con_var2" x="62.65" y="69.35" width="5.94" height="2.62" rx="1" transform="translate(136.29 5.04) rotate(90)" className={style.conPin} />
                        <rect id="DSPY_Con-8" data-name="DSPY_Con" x="58.98" y="73.7" width="13.29" height="4.28" transform="matrix(0, 1, -1, 0, 141.46, 10.21)" className={style.DSPYCon} />
                        <rect id="BTN_Con_var2-16" data-name="BTN_Con_var2" x="23.32" y="69.35" width="5.94" height="2.62" rx="1" transform="translate(96.96 44.37) rotate(90)" className={style.conPin} />
                        <rect id="DSPY_Con-9" data-name="DSPY_Con" x="19.65" y="73.7" width="13.29" height="4.28" transform="matrix(0, 1, -1, 0, 102.13, 49.54)" className={style.DSPYCon} />
                        <rect id="BTN_Con_var2-17" data-name="BTN_Con_var2" x="220.55" y="69.35" width="5.94" height="2.62" rx="1" transform="translate(294.18 -152.86) rotate(90)" className={style.conPin} />
                        <rect id="DSPY_Con-10" data-name="DSPY_Con" x="216.87" y="73.7" width="13.29" height="4.28" transform="matrix(0, 1, -1, 0, 299.36, -147.68)" className={style.DSPYCon} />
                    </g>
                    <g id="Connector_2pin_kopia" data-name="Connector_2pin kopia">
                        <rect id="BTN_Con_var2-18" data-name="BTN_Con_var2" x="140.45" y="12.7" width="5.94" height="2.62" rx="1" transform="translate(157.43 -129.42) rotate(90)" className={style.conPin} />
                        <rect id="BTN_Con_var2-19" data-name="BTN_Con_var2" x="136.52" y="12.7" width="5.94" height="2.62" rx="1" transform="translate(153.5 -125.48) rotate(90)" className={style.conPin} />
                        <rect id="DSPY_Con-11" data-name="DSPY_Con" x="136.4" y="14.44" width="9.75" height="8.57" transform="translate(160 -122.55) rotate(90)" className={style.DSPYCon} />
                    </g>
                    <g id="Connector_3pin">
                        <rect id="BTN_Con_var2-20" data-name="BTN_Con_var2" x="208.15" y="11.47" width="5.94" height="2.62" rx="1" transform="translate(223.9 -198.34) rotate(90)" className={style.conPin} />
                        <rect id="BTN_Con_var2-21" data-name="BTN_Con_var2" x="204.22" y="11.47" width="5.94" height="2.62" rx="1" transform="translate(219.97 -194.41) rotate(90)" className={style.conPin} />
                        <rect id="BTN_Con_var2-22" data-name="BTN_Con_var2" x="200.29" y="11.47" width="5.94" height="2.62" rx="1" transform="translate(216.04 -190.48) rotate(90)" className={style.conPin} />
                        <rect id="DSPY_Con-12" data-name="DSPY_Con" x="200.38" y="11.85" width="13.61" height="15.47" transform="translate(226.77 -187.6) rotate(90)" className={style.DSPYCon} />
                    </g>
                    <g id="Connector_4pin"><rect id="BTN_Con_var2-23" data-name="BTN_Con_var2" x="150.59" y="19.66" width="5.94" height="2.62" rx="1" transform="translate(307.12 41.95) rotate(-180)" className={style.conPin} />
                        <rect id="BTN_Con_var2-24" data-name="BTN_Con_var2" x="150.59" y="23.6" width="5.94" height="2.62" rx="1" transform="translate(307.12 49.81) rotate(180)" className={style.conPin} />
                        <rect id="BTN_Con_var2-25" data-name="BTN_Con_var2" x="150.59" y="27.53" width="5.94" height="2.62" rx="1" transform="translate(307.12 57.68) rotate(180)" className={style.conPin} />
                        <rect id="BTN_Con_var2-26" data-name="BTN_Con_var2" x="150.59" y="31.46" width="5.94" height="2.62" rx="1" transform="translate(307.12 65.54) rotate(180)" className={style.conPin} />
                        <rect id="DSPY_Con-13" data-name="DSPY_Con" x="137.1" y="17.18" width="16.46" height="19.4" transform="translate(290.65 53.75) rotate(180)" className={style.DSPYCon} />
                    </g>
                    <g id="Connector_6pin"><rect id="BTN_Con_var2-27" data-name="BTN_Con_var2" x="127.65" y="47.66" width="5.94" height="2.62" rx="1" transform="translate(81.65 179.6) rotate(-90)" className={style.conPin} />
                        <rect id="BTN_Con_var2-28" data-name="BTN_Con_var2" x="131.58" y="47.66" width="5.94" height="2.62" rx="1" transform="translate(85.58 183.53) rotate(-90)" className={style.conPin} />
                        <rect id="BTN_Con_var2-29" data-name="BTN_Con_var2" x="123.72" y="47.66" width="5.94" height="2.62" rx="1" transform="translate(77.72 175.66) rotate(-90)" className={style.conPin} />
                        <rect id="BTN_Con_var2-30" data-name="BTN_Con_var2" x="119.79" y="47.66" width="5.94" height="2.62" rx="1" transform="translate(73.78 171.73) rotate(-90)" className={style.conPin} />
                        <rect id="BTN_Con_var2-31" data-name="BTN_Con_var2" x="115.85" y="47.66" width="5.94" height="2.62" rx="1" transform="translate(69.85 167.8) rotate(-90)" className={style.conPin} />
                        <rect id="BTN_Con_var2-32" data-name="BTN_Con_var2" x="111.92" y="47.66" width="5.94" height="2.62" rx="1" transform="translate(65.92 163.87) rotate(-90)" className={style.conPin} />
                        <rect id="DSPY_Con-14" data-name="DSPY_Con" x="118.59" y="31.63" width="12.28" height="24.74" transform="translate(80.72 168.73) rotate(-90)" className={style.DSPYCon} />
                    </g>
                </g>
            </svg>
        </>
    );

}

