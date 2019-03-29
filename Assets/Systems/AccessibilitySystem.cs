﻿using UnityEngine;
using FYFY;
using UnityEngine.UI;
using TMPro;

public class AccessibilitySystem : FSystem {

    //creation de famille qui recupere tous les components type Accessibility_settings
    private Family needUpdateFont_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFont), typeof(Accessibility_settings)));

    private Family needUpdateFontSize_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFontSize)));

    private Family needUpdateFontOutlineWidth_f = FamilyManager.getFamily(new AllOfComponents(typeof(UpdateFontOutline)));

    //creation de famille qui recupere tous les components type Text; TextMeshPro et TextMeshProUGUI
    private Family text_f = FamilyManager.getFamily(new AnyOfComponents (typeof(TextMeshPro), typeof(TextMeshProUGUI)));
    
    public AccessibilitySystem ()
    {
        if (Application.isPlaying)
        {
            needUpdateFont_f.addEntryCallback(onNeedUpdateFont); //Ecouteur qui regarde quand un nouvel element rentre dans la famille et dans ce cas appel la méthode onNeedUpdate
            needUpdateFontSize_f.addEntryCallback(onNeedUpdateFontSize); //A chaque fois qu'on touche à la sliderBar taille police, on est rentré dans la famille needUpdateFontSize_f
            needUpdateFontOutlineWidth_f.addEntryCallback(onNeedUpdateFontOutlineWidth);
        }
    }

    //Script pour modifier l'épaisseur contour des text
    private void onNeedUpdateFontOutlineWidth(GameObject go)
    {
        
        UpdateFontOutline ufo = go.GetComponent<UpdateFontOutline>();
        foreach (GameObject textFontOutline in text_f)
        {
            TMP_Text thickness = textFontOutline.GetComponent<TMP_Text>();
            if (thickness != null)
                thickness.outlineWidth = ufo.newWidthContour;
        }
        GameObjectManager.removeComponent<UpdateFontOutline>(go);
    }

    //Script pour modifier la taille de la police 
    private void onNeedUpdateFontSize (GameObject go)
    {
        UpdateFontSize ufs = go.GetComponent<UpdateFontSize>();
        foreach (GameObject textSize in text_f)
        {
            TMP_Text tmFontSize = textSize.GetComponent<TMP_Text>();
            tmFontSize.fontSize = ufs.newFontSize;

        }
        GameObjectManager.removeComponent<UpdateFontSize>(go);
    }
    
    //Script pour switcher entre police par défaut et police accessible 
    private void onNeedUpdateFont(GameObject go)
    {
        Accessibility_settings accessSettings = go.GetComponent<Accessibility_settings>();
        
        Font textFont;
        TMP_FontAsset TM_Font;
        if (accessSettings.enableFont) //Si la case est cochée alors changement de toutes les polices
        {
            textFont = accessSettings.accessibleFont;//attribution de la police defini dans Accessibility_settings à txt
            TM_Font = accessSettings.accessibleFontTMPro;
        } else //sinon mettre les polices par défaut
        {
            textFont = accessSettings.defaultFont;
            TM_Font = accessSettings.defaultFontTMPro;
        }

        foreach (GameObject textGo in text_f) //parcours de tous les GO de la famille text_f
        {
            TMP_Text tm = textGo.GetComponent<TMP_Text>();
            tm.font = TM_Font;
        }
        
        GameObjectManager.removeComponent<UpdateFont>(go); //tuer updatefont sinon pas de possibilité de changer de police car tous les éléments seront déjà dans la famille
        
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        
    }
}