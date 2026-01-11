using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class AllRiddles
{
    public static readonly Dictionary<int, String> HardRiddles
        = new()
        {
            {3, "D32B68"},  //HARD-BOMB1
            {6, "H9M167"},  //HARD-BOMB2
            {9, "48RDX6"},  //HARD-BOMB3
            {12, "68B56S"}, //HARD-BOMB4
            {15, "TX67XT"}, //HARD-BOMB5
            {18, "G48S32"}, //HARD-BOMB6
        };
    
    public static readonly Dictionary<int, String> Riddles
        = new()
        {
            { 1, "67A45R"},
            { 2, "B21BRK"},
            { 4, "NX5RDX"},
            { 5, "P5G47P"},
            { 7, "2A3X59"},
            { 8, "BRPAC6"},
            {10, "RT4DLL"},
            {11, "9X9L5T"},
            {13, "7AX569"},
            {14, "X67RT9"},
            {16, "2TT5EN"},
            {17, "38R59S"},
            {19, "V5GP2Y"},
            {20, "LLM5CL"},
            {21, "N78FKO"},
            {22, "WD3RTY"},
            {23, "MSM547"},
            {24, "SNT3NZ"},
        };
    
    public static KeyValuePair<int, String> GetHardRiddle(int bombNumber)
    {
        return HardRiddles.ElementAt(bombNumber);
    }
}

