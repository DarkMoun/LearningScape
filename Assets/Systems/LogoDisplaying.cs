﻿using UnityEngine;
using UnityEngine.PostProcessing;
using FYFY;
using FYFY_plugins.Monitoring;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;

public class LogoDisplaying : FSystem {

    // This system manage displaying of the story

    private Family f_logo = FamilyManager.getFamily(new AllOfComponents(typeof(ImgBank)));
    private Family f_fadingMenuElems = FamilyManager.getFamily(new AllOfComponents(typeof(FadingMenu)));
    private Family f_loadingFragment = FamilyManager.getFamily(new AnyOfTags("LoadingFragment"));

    private GameObject logoGo;
    private Image fadingImage;
    private Image background;

    // Contains all texts of the story
    private List<List<string>> storyTexts;
    // Contains current texts of the story
    private string[] readTexts;

    ImgBank logo;

    private int fadeSpeed = 3;

    private float readingTimer = -Mathf.Infinity;
    private int currentLogo = -1;
    private int nextLogo = 0;
    private bool showLogo = false;
    private bool closeLogo = false;

    private GameObject loadingFragment;
    private Material lfMat;
    private Vector3 targetEmissionColor;
    private Vector3 currentColor;
    private bool menuFaded = false;
    private bool motionBlurWasOn;

    public static LogoDisplaying instance;

    public LogoDisplaying()
    {
        if (Application.isPlaying)
        {
            logoGo = f_logo.First();
            foreach (Transform child in logoGo.transform)
            {
                if (child.gameObject.name == "Background")
                    background = child.gameObject.GetComponent<Image>();
                else if (child.gameObject.name == "FadingImage")
                    fadingImage = child.gameObject.GetComponent<Image>();
            }

            logo = logoGo.GetComponent<ImgBank>();

            loadingFragment = f_loadingFragment.First();
            lfMat = loadingFragment.transform.GetChild(0).GetComponent<Renderer>().material;
            targetEmissionColor = Random.insideUnitSphere * 155 + Vector3.one * 100;
            currentColor = new Vector3(0, 86, 255);
        }
        instance = this;
    }
    
    protected override void onPause(int currentFrame)
    {
        GameObjectManager.setGameObjectState(logoGo, false);
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        if (loadingFragment.activeSelf)
        {
            GameObjectManager.setGameObjectState(loadingFragment, false);
            GameObjectManager.setGameObjectState(logoGo.transform.GetChild(2).gameObject, false);
            if(motionBlurWasOn)
                loadingFragment.transform.parent.GetComponent<PostProcessingBehaviour>().profile.motionBlur.enabled = true;
        }
        // switch to next logo if mouse clicked or Escape pressed
        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Cancel") || Input.GetButtonDown("Submit"))
            // change to next logo
            nextLogo++;

        if (nextLogo != currentLogo)
        {
            // make logo transparent (usefull in case of clicking)
            fadingImage.color = new Color(1, 1, 1, 0);
            // check if logo remaining
            if (nextLogo < logo.bank.Length)
            {
                currentLogo = nextLogo;
                fadingImage.sprite = logo.bank[currentLogo];
                fadingImage.preserveAspect = true;
                showLogo = true;
                readingTimer = Time.time;
            }
            else
            {
                if (!closeLogo)
                {
                    int nbFadinElems = f_fadingMenuElems.Count;
                    for (int i = 0; i < nbFadinElems; i++)
                        GameObjectManager.setGameObjectState(f_fadingMenuElems.getAt(i), true);
                    closeLogo = true;
                    readingTimer = Time.time;
                    // Start main menu
                    MenuSystem.instance.Pause = false;
                }
            }

        }

        if (!closeLogo)
        {
            if (showLogo) // alpha to plain
            {
                if (Time.time - readingTimer < fadeSpeed)
                    // fade progress
                    fadingImage.color = new Color(1, 1, 1, (Time.time - readingTimer) / fadeSpeed);
                else
                {
                    // fade ends
                    fadingImage.color = new Color(1, 1, 1, 1);
                    showLogo = false;
                    readingTimer = Time.time;
                }
            }
            else // plain to alpha
            {
                if (Time.time - readingTimer < fadeSpeed)
                    fadingImage.color = new Color(1, 1, 1, 1 - (Time.time - readingTimer) / fadeSpeed);
                else
                {
                    fadingImage.color = new Color(1, 1, 1, 0);
                    showLogo = true;
                    readingTimer = Time.time;
                    nextLogo++;
                }
            }
        }
        else
        {
            float alpha = 0f;
            if (Time.time - readingTimer < fadeSpeed)
            {
                alpha = 1f;
                if (!menuFaded)
                {
                    alpha = (Time.time - readingTimer) / fadeSpeed;
                    background.color = new Color(0, 0, 0, 1-alpha);
                }
            }
            else // fade end
            {
                alpha = 1f;
                if(menuFaded)
                    // Pause this
                    this.Pause = true;
                else
                {
                    menuFaded = true;
                    // Disable Logo
                    GameObjectManager.setGameObjectState(logoGo, false);
                    readingTimer = Time.time;
                }
            }
            GameObject tmpGO = null;
            int nbFadinElems = f_fadingMenuElems.Count;
            for (int i = 0; i < nbFadinElems; i++)
            {
                tmpGO = f_fadingMenuElems.getAt(i);
                if (tmpGO.GetComponent<Image>())
                    tmpGO.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
                else
                {
                    ColorBlock cb = tmpGO.GetComponent<Button>().colors;
                    cb.normalColor = new Color(cb.normalColor.r, cb.normalColor.g, cb.normalColor.b, alpha);
                    tmpGO.GetComponent<Button>().colors = cb;
                }
            }
        }
    }
}