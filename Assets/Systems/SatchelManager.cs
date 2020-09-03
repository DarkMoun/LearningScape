﻿using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;

public class SatchelManager : FSystem {

    // this system manage the satchel and its paper

    //all selectable objects
    private Family f_bag = FamilyManager.getFamily(new AnyOfTags("Bag"));
    private Family f_selectedBag = FamilyManager.getFamily(new AnyOfTags("Bag"), new AllOfComponents(typeof(ReadyToWork), typeof(Animator)));
    private Family f_closeBag = FamilyManager.getFamily(new AnyOfTags("Bag", "InventoryElements", "HUD_TransparentOnMove"), new AllOfComponents(typeof(PointerOver)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_itemSelected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new AllOfComponents(typeof(SelectedInInventory)));
    private Family f_itemUnselected = FamilyManager.getFamily(new AnyOfTags("InventoryElements"), new NoneOfComponents(typeof(SelectedInInventory)));

    private float dist;

    //bag
    private Vector3 bagPaperInitialPos;
    private GameObject selectedBag;
    private Animator bagAnimator;

    // paper
    private GameObject paper;
    private Image paperImg;
    private BagImage paperImgRef;

    private bool satchelOpenning = false;
    private bool paperOpenning = false;
    private bool paperOut = false;
    private bool closingSatchel = false;

    private int currentPaperViews = 0; // 0 means no glasses, 1 means glass 1 is on, 2 means glass 2 is on, 3 means both glasses are on

    public static SatchelManager instance;

    public SatchelManager()
    {
        if (Application.isPlaying)
        {
            bagPaperInitialPos = f_bag.First().GetComponentInChildren<Canvas>().gameObject.transform.parent.localPosition;
            paper = f_bag.First().transform.GetChild(1).gameObject;
            paperImg = f_bag.First().GetComponentInChildren<Image>();
            paperImgRef = f_bag.First().GetComponentInChildren<BagImage>(); 

            f_selectedBag.addEntryCallback(onReadyToWorkOnSatchel);
            f_itemSelected.addEntryCallback(onItemSelectedInInventory);
            f_itemUnselected.addEntryCallback(onItemUnselectedInInventory);
        }
        instance = this;
    }

    private void onReadyToWorkOnSatchel(GameObject go)
    {
        selectedBag = go;

        bagAnimator = selectedBag.GetComponent<Animator>();
        bagAnimator.SetTrigger("openSatchel"); // start animation
        satchelOpenning = true;
        paperOpenning = false;
        paperOut = false;
        closingSatchel = false;

        // Launch this system
        instance.Pause = false;
    }

    private void updatePictures()
    {
        // switch appropriate image depending on glasses worn
        switch (currentPaperViews)
        {
            case 0:
                paperImg.sprite = paperImgRef.image0;
                break;
            case 1:
                paperImg.sprite = paperImgRef.image1;
                break;
            case 2:
                paperImg.sprite = paperImgRef.image2;
                break;
            default:
                paperImg.sprite = paperImgRef.image3;
                break;
        }
    }

    private void onItemSelectedInInventory(GameObject go)
    {
        if (go.name == "Glasses1" || go.name == "Glasses2")
        {
            if (go.name == "Glasses1")
                currentPaperViews = currentPaperViews + 1;
            else
                currentPaperViews = currentPaperViews + 2;
            if (paperOut)
                updatePictures();
        }
    }

    private void onItemUnselectedInInventory(GameObject go)
    {
        if (go.name == "Glasses1" || go.name == "Glasses2")
        {
            if (go.name == "Glasses1")
                currentPaperViews = currentPaperViews - 1;
            else
                currentPaperViews = currentPaperViews - 2;
            if (paperOut)
                updatePictures();
        }
    }

    // return true if UI with name "name" is selected into inventory
    private GameObject isSelected(string name)
    {
        foreach (GameObject go in f_itemSelected)
            if (go.name == name)
                return go;
        return null;
    }

    // Use this to update member variables when system pause. 
    // Advice: avoid to update your families inside this function.
    protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        if (selectedBag)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and paper is out of the bag and IAR is closed (because Escape close IAR)
            if (((f_closeBag.Count == 0 && Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Cancel") && f_iarBackground.Count == 0)) && paperOut && !closingSatchel)
            {
                if (paperOut)
                {
                    // Reset rendermode
                    selectedBag.GetComponentInChildren<Canvas>().scaleFactor = 1;
                    selectedBag.GetComponentInChildren<Canvas>().renderMode = RenderMode.WorldSpace;
                    selectedBag.GetComponentInChildren<Canvas>().gameObject.GetComponent<RectTransform>().localPosition = Vector3.forward * (-0.51f - paper.transform.localPosition.z);
                }
                bagAnimator.SetTrigger("closeSatchel");
                closingSatchel = true;
            }

            // Check if moving satchel in front of the player is over
            if (satchelOpenning && bagAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                paperOpenning = true;
                updatePictures();
                satchelOpenning = false;
            }
            else if (paperOpenning && bagAnimator.GetCurrentAnimatorStateInfo(0).IsName("OpenPaper") && bagAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                //show the paper on the camera screen when it reaches the final position
                Canvas canvas = selectedBag.GetComponentInChildren<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.scaleFactor = 0.85f + (Screen.height - 720) / 900f; // 900 is magic value to add 0.4f when passing from default resolution (1280/720) to high resolution (1980/1080)
                paper.transform.position = selectedBag.transform.TransformPoint(bagPaperInitialPos) + Vector3.up * 0.8f;
                paperOpenning = false;
                paperOut = true;
            }
            else if (closingSatchel && (bagAnimator.GetCurrentAnimatorStateInfo(0).IsName("ClosePaper") || bagAnimator.GetCurrentAnimatorStateInfo(0).IsName("CloseLockedSatchel")) && bagAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                // remove ReadyToWork component to release selected GameObject
                GameObjectManager.removeComponent<ReadyToWork>(selectedBag);

                selectedBag = null;
                bagAnimator = null;

                // Pause this system
                instance.Pause = true;
            }
        }
	}
}