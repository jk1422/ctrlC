import React, { useState, useEffect, FC, useCallback } from 'react';
import style from "mods/Style/main.module.scss"
import { ValueBinding } from 'cs2/api';

export const LEDSign: FC = () => {
    const SCROLLER_LENGTH = 120;
    const HEIGHT = 8;
    const fps = 2;
    const [lights, setLights] = useState<boolean[][]>(Array(HEIGHT).fill(Array(SCROLLER_LENGTH).fill(false)));
    const [leftPointer, setLeftPointer] = useState(SCROLLER_LENGTH + 1);

    // Uppdaterad funktion för att konvertera text till LED-format
    const textToLED = (theWord: string): boolean[][] => {
        const theMessage: boolean[][] = [];
        theWord = theWord.toUpperCase();

        for (let i = 0; i < theWord.length; i++) {
            const charLED = charToLED(theWord.charAt(i)); // Returnerar boolean[][]
            theMessage.push(...charLED); // Lägg till hela LED-representationen för tecknet

            // Lägg till en extra kolumn av släckta lampor som mellanrum mellan bokstäver
            const spaceColumn = Array(HEIGHT).fill(false); // Släckt kolumn
            theMessage.push(spaceColumn);
        }

        return theMessage; // Behåll tvådimensionell array
    };

    // Exempel på charToLED för ett mellanslag
    const charToLED = (theChar: string): boolean[][] => {
        var theLed = [];
        switch (theChar) {
            case 'A':
                theLed = [[false, false, true, true, true, true, true],
                [false, true, false, false, true, false, false],
                [true, false, false, false, true, false, false],
                [false, true, false, false, true, false, false],
                [false, false, true, true, true, true, true]];
                break;
            case 'B':
                theLed = [[true, true, true, true, true, true, true],
                [true, false, false, true, false, false, true],
                [true, false, false, true, false, false, true],
                [true, false, false, true, false, false, true],
                [false, true, true, false, true, true, false]];
                break;
            case 'C':
                theLed = [[false, true, true, true, true, true, false],
                [true, false, false, false, false, false, true],
                [true, false, false, false, false, false, true],
                [true, false, false, false, false, false, true],
                [false, true, false, false, false, true, false]];
                break;
            case 'D':
                theLed = [[true, true, true, true, true, true, true],
                [true, false, false, false, false, false, true],
                [true, false, false, false, false, false, true],
                [true, false, false, false, false, false, true],
                [false, true, true, true, true, true, false]];
                break;
            case 'E':
                theLed = [[true, true, true, true, true, true, true],
                [true, false, false, true, false, false, true],
                [true, false, false, true, false, false, true],
                [true, false, false, true, false, false, true],
                [true, false, false, false, false, false, true]];
                break;
            case 'F':
                theLed = [[true, true, true, true, true, true, true],
                [true, false, false, true, false, false, false],
                [true, false, false, true, false, false, false],
                [true, false, false, true, false, false, false],
                [true, false, false, false, false, false, false]];
                break;
            case 'G':
                theLed = [[false, true, true, true, true, true, false],
                [true, false, false, false, false, false, true],
                [true, false, false, false, false, false, true],
                [true, false, false, false, true, false, true],
                [true, true, false, false, true, true, true]];
                break;
            case 'H':
                theLed = [[true, true, true, true, true, true, true],
                [false, false, false, true, false, false, false],
                [false, false, false, true, false, false, false],
                [false, false, false, true, false, false, false],
                [true, true, true, true, true, true, true]];
                break;
            case 'I':
                theLed = [[false, false, false, false, false, false, false],
                [true, false, false, false, false, false, true],
                [true, true, true, true, true, true, true],
                [true, false, false, false, false, false, true],
                [false, false, false, false, false, false, false]];
                break;
            case 'J':
                theLed = [[false, false, false, false, false, true, false],
                [false, false, false, false, false, false, true],
                [true, false, false, false, false, false, true],
                [true, true, true, true, true, true, false],
                [true, false, false, false, false, false, false]];
                break;
            case 'K':
                theLed = [[true, true, true, true, true, true, true],
                [false, false, false, true, false, false, false],
                [false, false, true, false, true, false, false],
                [false, true, false, false, false, true, false],
                [true, false, false, false, false, false, true]];
                break;
            case 'L':
                theLed = [[true, true, true, true, true, true, true],
                [false, false, false, false, false, false, true],
                [false, false, false, false, false, false, true],
                [false, false, false, false, false, false, true],
                [false, false, false, false, false, false, true]];
                break;
            case 'M':
                theLed = [[true, true, true, true, true, true, true],
                [false, true, false, false, false, false, false],
                [false, false, true, false, false, false, false],
                [false, true, false, false, false, false, false],
                [true, true, true, true, true, true, true]];
                break;
            case 'N':
                theLed = [[true, true, true, true, true, true, true],
                [false, false, true, false, false, false, false],
                [false, false, false, true, false, false, false],
                [false, false, false, false, true, false, false],
                [true, true, true, true, true, true, true]];
                break;
            case 'O':
                theLed = [[false, true, true, true, true, true, false],
                [true, false, false, false, false, false, true],
                [true, false, false, false, false, false, true],
                [true, false, false, false, false, false, true],
                [false, true, true, true, true, true, false]];
                break;
            case 'P':
                theLed = [[true, true, true, true, true, true, true],
                [true, false, false, true, false, false, false],
                [true, false, false, true, false, false, false],
                [true, false, false, true, false, false, false],
                [false, true, true, false, false, false, false]];
                break;
            case 'Q':
                theLed = [[false, true, true, true, true, true, false],
                [true, false, false, false, false, false, true],
                [true, false, false, false, true, false, true],
                [true, false, false, false, false, true, false],
                [false, true, true, true, true, false, true]];
                break;
            case 'R':
                theLed = [[true, true, true, true, true, true, true],
                [true, false, false, true, false, false, false],
                [true, false, false, true, false, false, false],
                [true, false, false, true, false, false, false],
                [false, true, true, false, true, true, true]];
                break;
            case 'S':
                theLed = [[false, true, true, false, false, false, true],
                [true, false, false, true, false, false, true],
                [true, false, false, true, false, false, true],
                [true, false, false, true, false, false, true],
                [true, false, false, false, true, true, false]];
                break;
            case 'T':
                theLed = [[true, false, false, false, false, false, false],
                [true, false, false, false, false, false, false],
                [true, true, true, true, true, true, true],
                [true, false, false, false, false, false, false],
                [true, false, false, false, false, false, false]];
                break;
            case 'U':
                theLed = [[true, true, true, true, true, true, false],
                [false, false, false, false, false, false, true],
                [false, false, false, false, false, false, true],
                [false, false, false, false, false, false, true],
                [true, true, true, true, true, true, false]];
                break;
            case 'V':
                theLed = [[true, true, true, true, true, false, false],
                [false, false, false, false, false, true, false],
                [false, false, false, false, false, false, true],
                [false, false, false, false, false, true, false],
                [true, true, true, true, true, false, false]];
                break;
            case 'W':
                theLed = [[true, true, true, true, true, true, false],
                [false, false, false, false, false, false, true],
                [false, false, false, false, true, true, false],
                [false, false, false, false, false, false, true],
                [true, true, true, true, true, true, false]];
                break;
            case 'X':
                theLed = [[true, false, false, false, false, false, true],
                [false, true, true, false, true, true, false],
                [false, false, false, true, false, false, false],
                [false, true, true, false, true, true, false],
                [true, false, false, false, false, false, true]];
                break;
            case 'Y':
                theLed = [[true, false, false, false, false, false, false],
                [false, true, false, false, false, false, false],
                [false, false, true, true, true, true, true],
                [false, true, false, false, false, false, false],
                [true, false, false, false, false, false, false]];
                break;
            case 'Z':
                theLed = [[true, false, false, false, false, true, true],
                [true, false, false, false, true, false, true],
                [true, false, false, true, false, false, true],
                [true, false, true, false, false, false, true],
                [true, true, false, false, false, false, true]];
                break;
            case '1':
                theLed = [
                    [false, true, false, false, false, false, true],
                    [true, true, true, true, true, true, true],
                    [false, false, false, false, false, false, true]];
                break;
            case '2':
                theLed = [
                    [false, true, false, false, false, false, true],
                    [true, false, false, false, false, true, true],
                    [true, false, false, false, true, false, true],
                    [true, false, false, true, false, false, true],
                    [false, true, true, false, false, false, true]];
                break;
            case '3':
                theLed = [
                    [true, false, false, false, false, false, true],
                    [true, false, false, false, false, false, true],
                    [true, false, false, true, false, false, true],
                    [true, false, false, true, false, false, true],
                    [false, true, true, false, true, true, false]];
                break;
            case '4':
                theLed = [
                    [true, true, true, true, false, false, false],
                    [false, false, false, true, false, false, false],
                    [false, false, false, true, false, false, false],
                    [false, false, false, true, false, false, false],
                    [true, true, true, true, true, true, true]];
                break;
            case '5':
                theLed = [
                    [true, true, true, true, false, false, true],
                    [true, false, false, true, false, false, true],
                    [true, false, false, true, false, false, true],
                    [true, false, false, true, false, false, true],
                    [true, false, false, false, true, true, false]];
                break;
            case '6':
                theLed = [
                    [false, true, true, true, true, true, false],
                    [true, false, false, true, false, false, true],
                    [true, false, false, true, false, false, true],
                    [true, false, false, true, false, false, true],
                    [false, true, false, false, true, true, false]];
                break;
            case '7':
                theLed = [
                    [true, false, false, false, false, false, true],
                    [true, false, false, false, false, true, false],
                    [true, false, false, false, true, false, false],
                    [true, false, false, true, false, false, false],
                    [true, true, true, false, false, false, false]];
                break;
            case '8':
                theLed = [
                    [false, true, true, false, true, true, false],
                    [true, false, false, true, false, false, true],
                    [true, false, false, true, false, false, true],
                    [true, false, false, true, false, false, true],
                    [false, true, true, false, true, true, false]];
                break;
            case '9':
                theLed = [
                    [false, true, true, false, false, false, true],
                    [true, false, false, true, false, false, true],
                    [true, false, false, true, false, false, true],
                    [true, false, false, true, false, false, true],
                    [false, true, true, false, true, true, false]];
                break;
            case '0':
                theLed = [
                    [false, true, true, true, true, true, false],
                    [true, false, false, false, true, false, true],
                    [true, false, false, true, false, false, true],
                    [true, false, true, false, false, false, true],
                    [false, true, true, true, true, true, false]];
                break;
            case '!':
                theLed = [[true, true, true, true, true, false, true]];
                break;
            case ',':
                theLed = [
                    [false, false, false, false, false, false, false, true],
                    [false, false, false, false, false, false, true],
                ];
                break;
            case ':':
                theLed = [
                    [false, true, false, false, false, true, false, false],
                ];
                break;
            case '-':
                theLed = [
                    
                    [false, false, false, true, false, false, false],
                    [false, false, false, true, false, false, false],
                    [false, false, false, true, false, false, false]];
                break;
            case '_':
                theLed = [[false, false, false, false, false, false, true],
                    [false, false, false, false, false, false, true],
                    [false, false, false, false, false, false, true],
                    [false, false, false, false, false, false, true],
                    [false, false, false, false, false, false, true]];
                break;
            case '<':
                theLed = [
                    [false, false, true, true, false, false, false ],
                    [false, true, false, false, true, false, false ],
                    [false, true, false, false, false, true, false ],
                    [false, false, true, false, false, false, true ],
                    [false, true, false, false, false, true, false ],
                    [false, true, false, false, true, false, false ],
                    [false, false, true, true, false, false, false ],
                ];
                break;
            case '|':
                theLed = [
                    [false, false, false, false, true, false, false],
                    [false, false, false, false, false, true, false],
                    [true, true, true, false, false, false, true],
                    [false, false, false, false, false, false, true],
                    [false, false, false, false, false, false, true],
                    [false, false, false, false, false, false, true],
                    [true, true, true, false, false, false, true],
                    [false, false, false, false, false, true, false],
                    [false, false, false, false, true, false, false]
                ];
                break;
            default: //Blank space
                theLed = [[false, false, false, false, false, false, false],
                [false, false, false, false, false, false, false],
                [false, false, false, false, false, false, false]];
        }
        return theLed;
    };


    const drawMessage = (messageArray: boolean[][], leftPointer: number) => {
        const updatedLights = Array.from({ length: HEIGHT }, () =>
            Array(SCROLLER_LENGTH).fill(false)
        );
        for (let col = 0; col < messageArray.length; col++) {
            for (let row = 0; row < HEIGHT; row++) {
                const offsetCol = leftPointer + col;
                if (offsetCol >= 0 && offsetCol < SCROLLER_LENGTH) {
                    updatedLights[row][offsetCol] = messageArray[col][row];
                }
            }
        }
        setLights(updatedLights);
    };

    useEffect(() => {
        const message = textToLED('< |');
        const furthestLeftPoint = 0 - message.length;

        const interval = setInterval(() => {
            setLeftPointer((prevLeftPointer) => {
                const newPointer = prevLeftPointer <= furthestLeftPoint ? SCROLLER_LENGTH + 5 : prevLeftPointer - 5;
                drawMessage(message, newPointer); // Uppdatera med det nya värdet
                return newPointer;
            });
        }, 1000 / fps);

        return () => clearInterval(interval); // Rensa intervallet när komponenten demonteras
    }, [fps]);

    // Array för rader (0 till 6)
    const rows = Array.from({ length: HEIGHT }, (_, rowIndex) => rowIndex);

    // Array för lampor i varje rad (0 till 59)
    const lightsPerRow = Array.from({ length: SCROLLER_LENGTH }, (_, lightIndex) => lightIndex);

    return (
        <div className={style.LED_BG_Wrapper}>
            <div className={style.LED_BG_Marquee}>
                {rows.map((row) => (
                    <div key={row} className={style.LED_BG_Row}>
                        {lightsPerRow.map((light) => (
                            <div
                                key={light}
                                className={lights[row][light] ? style.LED_BG_LightOn : style.LED_BG_LightOff}  // Dynamisk klass baserat på ljusstatus
                            ></div>
                        ))}
                    </div>
                ))}
            </div>
        </div>
    );
};

interface LEDSignSmallProps {
    circle?: boolean;
    ray?: boolean;
}

export const LEDSignSmall: FC<LEDSignSmallProps> = ({ circle = false, ray = false }) => {
    const HEIGHT = 7;
    const WIDTH = 7;

    // Skapa en tom 7x7-matris för lamporna
    const [lights, setLights] = useState<boolean[][]>(
        Array.from({ length: HEIGHT }, () => Array(WIDTH).fill(false))
    );

    // Definiera LED-symboler för olika verktyg
    const LED = (circle: boolean, ray: boolean): boolean[][] => {
        if (circle) {
            return [
                [false, false, true, true, true, false, false],
                [false, true, false, false, false, true, false],
                [true, false, false, false, false, false, true],
                [true, false, false, false, false, false, true],
                [true, false, false, false, false, false, true],
                [false, true, false, false, false, true, false],
                [false, false, true, true, true, false, false],
            ];
        }
        if (ray) {
            return [
                [false, false, false, true, false, false, false],
                [false, false, true, true, true, false, false],
                [false, true, true, true, true, true, false],
                [false, false, false, true, false, false, false],
                [false, false, false, true, false, false, false],
                [false, false, false, true, false, false, false],
                [false, false, false, true, false, false, false],
            ];
        }
        // Om varken cirkel eller ray är true, returnera en tom matris
        return Array.from({ length: HEIGHT }, () => Array(WIDTH).fill(false));
    };

    const drawMessage = (messageArray: boolean[][]) => {
        const updatedLights = Array.from({ length: HEIGHT }, () =>
            Array(WIDTH).fill(false)
        );
        for (let row = 0; row < messageArray.length; row++) {
            for (let col = 0; col < messageArray[row].length; col++) {
                if (col < WIDTH && row < HEIGHT) {
                    updatedLights[row][col] = messageArray[row][col];
                }
            }
        }
        setLights(updatedLights);
    };

    useEffect(() => {
        const symbol = LED(circle, ray); // Använd propsen för att bestämma vilken symbol som ska visas
        drawMessage(symbol);
    }, [circle, ray]);

    // Rendera matriserna för LED-skärmen
    const rows = Array.from({ length: HEIGHT }, (_, rowIndex) => rowIndex);
    const lightsPerRow = Array.from({ length: WIDTH }, (_, lightIndex) => lightIndex);

    return (
        <div className={style.LED_SM_Wrapper}>
            <div className={style.LED_SM_Marquee}>
                {rows.map((row) => (
                    <div key={row} className={style.LED_SM_row}>
                        {lightsPerRow.map((light) => (
                            <div
                                key={light}
                                className={lights[row][light] ? style.LED_SM_LightOn : style.LED_SM_LightOff}  // Dynamisk klass baserat på ljusstatus
                            ></div>
                        ))}
                    </div>
                ))}
            </div>
        </div>
    );
};