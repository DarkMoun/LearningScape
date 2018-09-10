﻿using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;

public class IARTabNavigation : FSystem {

    // Manage base IAR integration (Open/Close + tab switching)

    private Family f_tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"), new AllOfComponents(typeof(LinkedWith), typeof(Button)));
    private Family f_fgm = FamilyManager.getFamily(new AllOfComponents(typeof(FocusedGOMaterial)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AllOfComponents(typeof(PointerSensitive)));
    private Family f_HUD_A = FamilyManager.getFamily(new AnyOfTags("HUD_A"));
    private Family f_atWork = FamilyManager.getFamily(new AllOfComponents(typeof(ReadyToWork)));

    private Sprite selectedTabSprite;
    private Sprite defaultTabSprite;

    private GameObject iar;
    private GameObject iarBackground;

    private bool openedAtLeastOnce = false;

    private Dictionary<FSystem, bool> systemsStates;

    public static IARTabNavigation instance;

    public IARTabNavigation()
    {
        if (Application.isPlaying)
        {
            foreach (GameObject tab in f_tabs)
            {
                tab.GetComponent<Button>().onClick.AddListener(delegate {
                    SwitchTab(tab);
                });
            }

            selectedTabSprite = f_fgm.First().GetComponent<FocusedGOMaterial>().selectedTabSprite;
            defaultTabSprite = f_fgm.First().GetComponent<FocusedGOMaterial>().defaultTabSprite;

            iarBackground = f_iarBackground.First();
            iar = iarBackground.transform.parent.gameObject;

            systemsStates = new Dictionary<FSystem, bool>();
        }
        instance = this;
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame)
    {
        GameObjectManager.setGameObjectState(f_HUD_A.First(), false); // hide HUD "A"
    }

    // Use this to update member variables when system resume.
    // Advice: avoid to update your families inside this function.
    protected override void onResume(int currentFrame)
    {
        if (openedAtLeastOnce)
            GameObjectManager.setGameObjectState(f_HUD_A.First(), true); // display HUD "A"
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        // Open/Close IAR with Escape and A keys
        if (iar.activeInHierarchy && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Escape) || (Input.GetMouseButtonDown(0) && iarBackground.GetComponent<PointerOver>())))
            closeIar();
        else if (!iar.activeInHierarchy && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Escape)))
        {
            if (Input.GetKeyDown(KeyCode.A))
                openIar(0); // Open IAR on the first tab
            else
                // Open IAR on the last tab only if player doesn't work on selectable enigm (Escape enables to exit the enigm)
                if (f_atWork.Count == 0)
                    openIar(f_tabs.Count - 1); // Open IAR on the last tab
        }
    }

    private void openIar(int tabId)
    {
        openedAtLeastOnce = true;
        GameObjectManager.setGameObjectState(f_HUD_A.First(), false); // hide HUD "A"
        GameObjectManager.setGameObjectState(iar, true); // open IAR
        SwitchTab(f_tabs.getAt(tabId)); // switch to the first tab
        systemsStates.Clear();
        // save systems states
        foreach (FSystem sys in FSystemManager.fixedUpdateSystems())
            systemsStates[sys] = sys.Pause;
        foreach (FSystem sys in FSystemManager.updateSystems())
            systemsStates[sys] = sys.Pause;
        foreach (FSystem sys in FSystemManager.lateUpdateSystems())
            systemsStates[sys] = sys.Pause;
        // set required systems states
        MovingSystem.instance.Pause = true;
        DreamFragmentCollecting.instance.Pause = true;
        Highlighter.instance.Pause = true;
        MirrorSystem.instance.Pause = true;
        ToggleObject.instance.Pause = true;
        CollectObject.instance.Pause = true;
        IARViewItem.instance.Pause = false;
        IARGearsEnigma.instance.Pause = false;
        MoveInFrontOf.instance.Pause = true;
        LockResolver.instance.Pause = true;
        PlankAndWireManager.instance.Pause = true;
        BallBoxManager.instance.Pause = true;
        LoginManager.instance.Pause = true;
        SatchelManager.instance.Pause = true;
        PlankAndMirrorManager.instance.Pause = true;
        WhiteBoardManager.instance.Pause = true;
    }

    public void closeIar()
    {
        GameObjectManager.setGameObjectState(iar, false); // close IAR
        // Restaure systems state (exception for LampManager)
        bool backLampManagerState = LampManager.instance.Pause;
        foreach (FSystem sys in systemsStates.Keys)
            sys.Pause = systemsStates[sys];
        LampManager.instance.Pause = backLampManagerState;
        // display HUD "A"
        GameObjectManager.setGameObjectState(f_HUD_A.First(), true);
    }

    private void SwitchTab(GameObject newSelectedTab)
    {
        // reset all tabs (text and image) and disable all contents
        foreach (GameObject oldTab in f_tabs)
        {
            oldTab.GetComponent<Image>().sprite = defaultTabSprite;
            oldTab.GetComponentInChildren<Text>().fontStyle = FontStyle.Normal;
            GameObjectManager.setGameObjectState(oldTab.GetComponent<LinkedWith>().link, false);
        }
        // set new tab text and image
        newSelectedTab.GetComponent<Image>().sprite = selectedTabSprite;
        newSelectedTab.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        // enable new content
        GameObjectManager.setGameObjectState(newSelectedTab.GetComponent<LinkedWith>().link, true);
    }
}