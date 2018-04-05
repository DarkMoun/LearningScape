﻿using UnityEngine;

public class Selectable : MonoBehaviour {//objects that can be selected by the player (whit the left click)
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public static bool selected = false; //true when a gameobject is selected

    public bool isSelected = false; //true when this component's gameobject is selected
    public string[] words;          //list of possible answers (used in the first prototype, not used anymore)
    public string answer;           //the correct answer in "words" (used in the first prototype, not used anymore)
    public bool solved = false;     //true if the enigma associated to this gameobject is solved

    //audios played when the player gives an answer
    public AudioClip right;
    public AudioClip wrong;
}