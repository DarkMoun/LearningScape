﻿using UnityEngine;

//Singleton that stores materials and sprites unused at the beginning
public class FocusedGOMaterial : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Material material;
    public Sprite selectedTabSprite;
    public Sprite plankSolvedSprite;
    public Material displayedBGMaterial;
}