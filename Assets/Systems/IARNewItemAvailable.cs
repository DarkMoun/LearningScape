﻿using UnityEngine;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IARNewItemAvailable : FSystem {

    // Disable feedback when parent item is clicked or under mouse pointer and blink HUD if a new item is available

    private Family f_itemsEnabled = FamilyManager.getFamily(new AllOfComponents(typeof(NewItemManager)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_notificationEnabled = FamilyManager.getFamily(new AnyOfTags("NewItemFeedback"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family f_newItemOver = FamilyManager.getFamily(new AllOfComponents(typeof(NewItemManager), typeof(PointerOver)));
    private Family f_inventoryWarning = FamilyManager.getFamily(new AnyOfTags("InventoryWarning"));
    private Family f_triggerableWarning = FamilyManager.getFamily(new AllOfComponents(typeof(NewItemManager), typeof(UnityEngine.UI.Selectable)), new AnyOfTags("InventoryElements"));
    private Family f_tabs = FamilyManager.getFamily(new AllOfComponents(typeof(NewItemManager), typeof(Button)), new AnyOfTags("IARTab"));

    private GameObject lastKeyboardViewed = null;

    private bool warningNewItem = true;
    private bool HUD_neverDisplayed = true;

    private Dictionary<int, GameObject> id2Go;

    public static IARNewItemAvailable instance;

    public IARNewItemAvailable()
    {
        if (Application.isPlaying)
        {
            f_itemsEnabled.addEntryCallback(OnNewItemEnabled);
            f_itemsEnabled.addExitCallback(OnItemDisabled);
            f_newItemOver.addEntryCallback(OnMouseOver);

            id2Go = new Dictionary<int, GameObject>();
        }
        instance = this;
    }

    private GameObject getFeedbackChild(GameObject go)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).CompareTag("NewItemFeedback"))
            {
                return go.transform.GetChild(i).gameObject;
            }
        }
        return null;
    }

    private void OnNewItemEnabled(GameObject go)
    {
        GameObject child = getFeedbackChild(go);
        if (child)
            GameObjectManager.setGameObjectState(child, true);
        if (!id2Go.ContainsKey(go.GetInstanceID()))
            id2Go.Add(go.GetInstanceID(), go);
    }

    private void OnItemDisabled(int instanceId)
    {
        // if an item is disable => disable also its child tagged NewItemFeedback
        GameObject go;
        if (id2Go.TryGetValue(instanceId, out go))
        {
            GameObject child = getFeedbackChild(go);
            if (child)
                GameObjectManager.setGameObjectState(child, false);
        }
    }

    private void OnMouseOver(GameObject go)
    {
        // find child with tag "NewItemFeedback"
        GameObject child = getFeedbackChild(go);
        if (child && child.activeInHierarchy)
        {
            NewItemManager nim = go.GetComponent<NewItemManager>();
            if (nim.disableOnMouseOver || (nim.disableOnClick && (Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Return))) || (nim.disableOnClick && Input.GetButtonDown("X_button")))
                GameObjectManager.setGameObjectState(child, false);
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
       if (lastKeyboardViewed != EventSystem.current.currentSelectedGameObject)
       {
            lastKeyboardViewed = EventSystem.current.currentSelectedGameObject;
            foreach (GameObject go in f_triggerableWarning)
            {
                if (go == lastKeyboardViewed)
                {
                    OnMouseOver(go);
                    break;
                }
            }
       }

      
       foreach (GameObject go in f_newItemOver)
            OnMouseOver(go); // same process as OnMouseEnter callback

       foreach (GameObject go in f_tabs)
        {
            if (go == EventSystem.current.currentSelectedGameObject)
                OnMouseOver(go);
        }

        // blink HUD "A" if at least one new item is available
        if (f_notificationEnabled.Count > 0)
        {
            if (Time.time - (int)Time.time > 0.5f && warningNewItem)
            {
                if (HUD_neverDisplayed)
                {
                    // enable parent
                    GameObjectManager.setGameObjectState(f_inventoryWarning.First().transform.parent.gameObject, true);
                    HUD_neverDisplayed = false;
                }
                // display warning
                GameObjectManager.setGameObjectState(f_inventoryWarning.First(), true);
                warningNewItem = false;
            }
            else if (Time.time - (int)Time.time < 0.5f && !warningNewItem)
            {
                // disable warning
                GameObjectManager.setGameObjectState(f_inventoryWarning.First(), false);
                warningNewItem = true;
            }
			f_inventoryWarning.First ().transform.parent.gameObject.GetComponent<Animator> ().enabled = true;
        }
        else
        {
			GameObject warn = f_inventoryWarning.First ();
            GameObject HUD_A = warn.transform.parent.gameObject;
            if (!warningNewItem)
            {
				GameObjectManager.setGameObjectState(warn, false);
                warningNewItem = true;
                HUD_A.transform.position = new Vector3(HUD_A.transform.position.x, 50.0f, HUD_A.transform.position.z);
            }
            HUD_A.GetComponent<Animator>().enabled = false;


        }
    }

}