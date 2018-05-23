﻿using UnityEngine;
using FYFY;
using UnityEngine.UI;
using PauseSystem = Pause;
using UnityStandardAssets.Characters.FirstPerson;

public class IARTab : FSystem {

    private Family inventoryFamily = FamilyManager.getFamily(new AnyOfTags("Inventory"));
    private Family screens = FamilyManager.getFamily(new AnyOfTags("ScreenIAR"));
    private Family canvas = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas)));
    private Family tabs = FamilyManager.getFamily(new AnyOfTags("IARTab"));
    private Family audioSourceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(AudioSource)));
    private Family door = FamilyManager.getFamily(new AllOfComponents(typeof(Door)));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family fences = FamilyManager.getFamily(new AnyOfTags("Fence"));
    private Family lockR2 = FamilyManager.getFamily(new AnyOfTags("LockRoom2"));
    private Family fgm = FamilyManager.getFamily(new AllOfComponents(typeof(FocusedGOMaterial)));

    private GameObject tabsGO;
    public static bool onIAR = false;
    public static bool room2Unlocked = false;
    public static bool room3Unlocked = false;
    private bool listenerAddedRoom2 = false;
    private bool listenerAddedRoom3 = false;

    private GameObject inventory;
    private GameObject screenR1;
    private GameObject screenR2;
    private GameObject screenR3;
    private GameObject menu;

    private Image inventoryButtonImage;
    private Image screen1ButtonImage;
    private Image screen2ButtonImage;
    private Image screen3ButtonImage;
    private Image menuButtonImage;
    private Sprite selectedTabSprite;
    private Sprite initialTabSprite;

    private bool onInventory = false;
    private bool onMenu = false;
    private GameObject activeUI = null;
    private bool playerEnabled = true;

    private bool wasOnInventory = false;
    private bool wasOnMenu = false;
    private bool windowClosed = false;

    private GameObject fence1;
    private GameObject fence2;

    private bool playerLookingToDoor = false;
    private Vector3 tmpTarget;
    private int angleCount = 0;

    private AudioSource gameAudioSource;

    public IARTab()
    {
        door.First().transform.position += Vector3.up * (5.73f - door.First().transform.position.y); //opened
        //door.First().transform.position += Vector3.up * (2.13f - door.First().transform.position.y); //closed

        tabsGO = tabs.First().transform.parent.gameObject;
        inventory = inventoryFamily.First();
        int nb = screens.Count;
        for(int i = 0; i < nb; i++)
        {
            if (screens.getAt(i).name.Contains(1.ToString()))
            {
                screenR1 = screens.getAt(i);
            }
            else if (screens.getAt(i).name.Contains(2.ToString()))
            {
                screenR2 = screens.getAt(i);
            }
            else if (screens.getAt(i).name.Contains(3.ToString()))
            {
                screenR3 = screens.getAt(i);
            }
        }
        nb = canvas.Count;
        for(int i = 0; i < nb; i++)
        {
            if(canvas.getAt(i).name == "PauseMenu")
            {
                menu = canvas.getAt(i);
                break;
            }
        }

        nb = tabs.Count;
        for(int i = 0; i < nb; i++)
        {
            switch (tabs.getAt(i).name)
            {
                case "InventoryTab":
                    inventoryButtonImage = tabs.getAt(i).GetComponent<Image>();
                    tabs.getAt(i).GetComponent<Button>().onClick.AddListener(delegate {
                        SwitchTab(inventory, inventoryButtonImage);
                        onInventory = true;
                    });
                    break;

                case "ScreenR1Tab":
                    screen1ButtonImage = tabs.getAt(i).GetComponent<Image>();
                    tabs.getAt(i).GetComponent<Button>().onClick.AddListener(delegate {
                        SwitchTab(screenR1, screen1ButtonImage);
                    });
                    break;

                case "ScreenR2Tab":
                    screen2ButtonImage = tabs.getAt(i).GetComponent<Image>();
                    break;

                case "ScreenR3Tab":
                    screen3ButtonImage = tabs.getAt(i).GetComponent<Image>();
                    break;

                case "MenuTab":
                    menuButtonImage = tabs.getAt(i).GetComponent<Image>();
                    tabs.getAt(i).GetComponent<Button>().onClick.AddListener(delegate {
                        SwitchTab(menu, menuButtonImage);
                        onMenu = true;
                    });
                    break;

                default:
                    break;
            }
        }

        selectedTabSprite = fgm.First().GetComponent<FocusedGOMaterial>().selectedTabSprite;
        initialTabSprite = inventoryButtonImage.sprite;

        nb = audioSourceFamily.Count;
        for(int i = 0; i < nb; i++)
        {
            if (audioSourceFamily.getAt(i).name == "Game")
            {
                gameAudioSource = audioSourceFamily.getAt(i).GetComponent<AudioSource>();
            }
        }

        nb = fences.Count;
        for (int i = 0; i < nb; i++)
        {
            if (fences.getAt(i).name.Contains(1.ToString()))
            {
                fence1 = fences.getAt(i);
            }
            else if (fences.getAt(i).name.Contains(2.ToString()))
            {
                fence2 = fences.getAt(i);
            }
        }
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
        if (inventory.activeSelf && !onInventory)
        {
            onIAR = true;
            playerEnabled = Inventory.playerEnabled;
            SwitchTab(inventory, inventoryButtonImage);
            tabsGO.SetActive(true);
        }
        else if (menu.activeSelf && !onMenu)
        {
            onIAR = true;
            playerEnabled = PauseSystem.playerEnabled;
            SwitchTab(menu, menuButtonImage);
            tabsGO.SetActive(true);
        }
        onMenu = menu.activeSelf;
        onInventory = inventory.activeSelf;

        wasOnInventory = (onInventory && !wasOnInventory) || wasOnInventory;
        wasOnMenu = (onMenu && !wasOnMenu) || wasOnMenu;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!onInventory && !onMenu && onIAR)
            {
                activeUI.SetActive(false);
            }
        }

        if (activeUI)
        {
            if (!activeUI.activeSelf)
            {
                if(room2Unlocked && !listenerAddedRoom2)
                {
                    if (windowClosed)
                    {
                        onIAR = false;
                        activeUI = null;
                        tabsGO.SetActive(false);
                        if (wasOnInventory)
                        {
                            CollectableGO.askCloseInventory = true;
                        }
                        if (wasOnMenu)
                        {
                            PauseSystem.askResume = true;
                        }
                        wasOnInventory = false;
                        wasOnMenu = false;
                        Inventory.playerEnabled = true;
                        PauseSystem.playerEnabled = true;
                        windowClosed = false;
                    }
                    else
                    {
                        ShowUI.askCloseWindow = true;
                        windowClosed = true;
                    }
                }
                else
                {
                    onIAR = false;
                    activeUI = null;
                    tabsGO.SetActive(false);
                    if (wasOnInventory)
                    {
                        CollectableGO.askCloseInventory = true;
                    }
                    if (wasOnMenu)
                    {
                        PauseSystem.askResume = true;
                    }
                    wasOnInventory = false;
                    wasOnMenu = false;
                    Inventory.playerEnabled = playerEnabled;
                    PauseSystem.playerEnabled = playerEnabled;
                }
            }
        }
        if(room2Unlocked && !listenerAddedRoom2)
        {
            if (!activeUI)
            {
                player.First().GetComponent<FirstPersonController>().enabled = false;
                Cursor.visible = false;
                if (!playerLookingToDoor)
                {
                    tmpTarget = door.First().transform.position - Camera.main.transform.position;
                    Vector3 newDir = Vector3.RotateTowards(Camera.main.transform.forward, tmpTarget, Mathf.PI / 180, 0);
                    Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                    if (Vector3.Angle(tmpTarget, Camera.main.transform.forward) < 1)
                    {
                        Camera.main.transform.forward = tmpTarget;
                        gameAudioSource.clip = door.First().GetComponent<Door>().openAudio;
                        gameAudioSource.PlayDelayed(0);
                        gameAudioSource.loop = true;
                        playerLookingToDoor = true;
                        tmpTarget = door.First().transform.position + Vector3.up * (5.73f - door.First().transform.position.y);
                    }
                }
                else
                {
                    door.First().transform.position = Vector3.MoveTowards(door.First().transform.position, tmpTarget, 0.1f);
                    if (door.First().transform.position == tmpTarget)
                    {
                        playerLookingToDoor = false;
                        gameAudioSource.loop = false;
                        screen2ButtonImage.GetComponent<Button>().onClick.AddListener(delegate {
                            SwitchTab(screenR2, screen2ButtonImage);
                        });
                        screen2ButtonImage.GetComponentInChildren<Text>().text = "Rêve 2";
                        listenerAddedRoom2 = true;
                        StoryDisplaying.readingTransition = true;
                    }
                }
            }
            else
            {
                activeUI.SetActive(false);
            }
        }
        if (room3Unlocked && !listenerAddedRoom3)
        {
            player.First().GetComponent<FirstPersonController>().enabled = false;
            Cursor.visible = false;
            if (!playerLookingToDoor)
            {
                tmpTarget = (fence1.transform.position + fence2.transform.position)/2 - Camera.main.transform.position;
                Vector3 newDir = Vector3.RotateTowards(Camera.main.transform.forward, tmpTarget, Mathf.PI / 180, 0);
                Camera.main.transform.rotation = Quaternion.LookRotation(newDir);
                if (Vector3.Angle(tmpTarget, Camera.main.transform.forward) < 1)
                {
                    Camera.main.transform.forward = tmpTarget;
                    gameAudioSource.PlayOneShot(lockR2.First().GetComponent<Selectable>().right);
                    playerLookingToDoor = true;
                }
            }
            else
            {
                fence1.transform.Rotate(0, 0, -1);
                fence2.transform.Rotate(0, 0, 1);
                angleCount++;
                if (angleCount == 103)
                {
                    angleCount = 0;
                    playerLookingToDoor = false;
                    screen3ButtonImage.GetComponent<Button>().onClick.AddListener(delegate {
                        SwitchTab(screenR3, screen3ButtonImage);
                    });
                    screen3ButtonImage.GetComponentInChildren<Text>().text = "Rêve 3";
                    listenerAddedRoom3 = true;
                    player.First().GetComponent<FirstPersonController>().enabled = true;
                }
            }
        }
    }

    private void SwitchTab(GameObject tabContent, Image button)
    {
        if (!CollectableGO.onInventory && tabContent.tag == "Inventory")
        {
            CollectableGO.askOpenInventory = true;
        }
        inventory.SetActive(false);
        screenR1.SetActive(false);
        screenR2.SetActive(false);
        screenR3.SetActive(false);
        menu.SetActive(false);
        int nb = tabs.Count;
        for(int i = 0; i < nb; i++)
        {
            tabs.getAt(i).GetComponent<Image>().sprite = initialTabSprite;
            tabs.getAt(i).GetComponentInChildren<Text>().fontStyle = FontStyle.Normal;
        }

        tabContent.SetActive(true);
        button.sprite = selectedTabSprite;
        button.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        activeUI = tabContent;
        Inventory.playerEnabled = playerEnabled;
        PauseSystem.playerEnabled = playerEnabled;
    }
}