﻿using UnityEngine;
using FYFY;
using FYFY_plugins.Monitoring;

public class CollectObject : FSystem {

    // This system shows objects in inventory when they are clicked in game

    //all collectable objects
    // 5 <=> UI layer. We want only in game linked game object so we exlude ones in UI
    // We process only Highlighted game objects (this component is dynamically added by Highlight system)
    private Family f_collectableObjects = FamilyManager.getFamily(new AllOfComponents(typeof(LinkedWith), typeof(Highlighted)), new NoneOfLayers(5), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_pressA = FamilyManager.getFamily(new AnyOfTags("PressA"));
    private Family f_HUD_A = FamilyManager.getFamily(new AnyOfTags("HUD_A"));

    public static CollectObject instance;

    private GameObject seenScroll;

    private GameObject itemCollectedNotif;
    private bool animateItem;
    private float timeStart;
    private Vector3 targetPosition;

    public CollectObject()
    {
        if (Application.isPlaying)
        {
            itemCollectedNotif = f_HUD_A.First().transform.parent.GetChild(6).gameObject;
            targetPosition = f_HUD_A.First().transform.position;
            animateItem = false;
        }
        instance = this;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        if (Input.GetButtonDown("Fire1"))
        {
            foreach (GameObject collect in f_collectableObjects)
            {
                GameObjectManager.addComponent<ActionPerformed>(collect, new { name = "perform", performedBy = "player" });
                GameObjectManager.addComponent<ActionPerformedForLRS>(collect, new { verb = "collected", objectType = "item", objectName = collect.name });
                // enable UI target
                GameObjectManager.setGameObjectState(collect.GetComponent<LinkedWith>().link, true);
                // particular case of collecting room2 scrolls
                if (collect.name.Contains("Scroll") && collect.name.Length == 7)
                {
                    // find link into IAR left screen
                    GameObject UI_metaScroll = collect.GetComponent<LinkedWith>().link;
                    GameObjectManager.setGameObjectState(UI_metaScroll.transform.GetChild(0).gameObject, true); // force to enable new item notification
                    // find link into IAR right screen
                    GameObject UIScroll = UI_metaScroll.GetComponent<LinkedWith>().link.transform.Find(collect.name).gameObject;
                    // enable it
                    GameObjectManager.setGameObjectState(UIScroll, true);
                }
                // particular case of puzzle pieces
                if (collect.name.Contains("PuzzleSet_") && collect.name.Length == 12)
                {
                    // find link into IAR left screen
                    GameObject UI_metaScroll = collect.GetComponent<LinkedWith>().link;
                    GameObjectManager.setGameObjectState(UI_metaScroll.transform.GetChild(0).gameObject, true); // force to enable new item notification
                    // find link into IAR right screen
                    GameObject UIScroll = UI_metaScroll.GetComponent<LinkedWith>().link.transform.Find(collect.name).gameObject;
                    // enable it
                    GameObjectManager.setGameObjectState(UIScroll, true);
                }
                // disable in-game source
                GameObjectManager.setGameObjectState(collect, false);
                // Play notification
                itemCollectedNotif.GetComponent<Animator>().SetTrigger("Start");
                
                // Play sound
                GameObjectManager.addComponent<PlaySound>(collect, new { id = 10 }); // id refer to FPSController AudioBank
                // particular case of collecting Intro_scroll game object => show ingame "Press A" notification
                if (collect.name == "Intro_Scroll")
                {
                    GameObjectManager.setGameObjectState(f_pressA.First(), true);
                }
            }
        }
    }
}