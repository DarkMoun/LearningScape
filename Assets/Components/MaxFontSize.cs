﻿using TMPro;
using UnityEngine;

public class MaxFontSize : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public int maxSize; //Valeur renseignée dans le max size jusqu'où le texte peut grossir au max
    [HideInInspector]
    public float defaultMaxSize; //Valeur renseignée dans le max size qui a permis de définir la taille de la police de départ

    public TMP_Text message;

    public void Start()
    {
        message.outlineWidth = 0;
        message.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
    }
    
}