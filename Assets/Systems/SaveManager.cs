﻿using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using FYFY;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;
using TMPro;

public class SaveManager : FSystem {

	private Family f_wrongWords = FamilyManager.getFamily(new AnyOfTags("PlankText"), new AllOfComponents(typeof(PointerSensitive), typeof(TextMeshPro)), new NoneOfComponents(typeof(IsSolution)));
	private Family f_wrongChair = FamilyManager.getFamily(new AnyOfTags("Chair"), new AllOfComponents(typeof(ToggleableGO)), new NoneOfComponents(typeof(IsSolution)));

	private Family f_prefabs = FamilyManager.getFamily(new AllOfComponents(typeof(PrefabContainer)));
	private Family f_menuButtons = FamilyManager.getFamily(new AllOfComponents(typeof(FadingMenu), typeof(Button)));
	private Family f_gameMenuContent = FamilyManager.getFamily(new AllOfComponents(typeof(VerticalLayoutGroup), typeof(WindowNavigator)));

	private Family f_fpsController = FamilyManager.getFamily(new AllOfComponents(typeof(FirstPersonController)));
	private Family f_timer = FamilyManager.getFamily(new AllOfComponents(typeof(Timer)));
	private Family f_componentMonitoring = FamilyManager.getFamily(new AllOfComponents(typeof(ComponentMonitoring)));
	private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));

	private Family f_queries = FamilyManager.getFamily(new AnyOfTags("Q-R1", "Q-R2", "Q-R3"), new AllOfComponents(typeof(QuerySolution)));
	private Family f_gearsSet = FamilyManager.getFamily(new AnyOfTags("Gears"), new AllOfComponents(typeof(LinkedWith)));
	private Family f_queriesRoom2 = FamilyManager.getFamily(new AnyOfTags("Q-R2"));

	private Family f_linked = FamilyManager.getFamily(new AllOfComponents(typeof(LinkedWith)));
	private Family f_dreamFragments = FamilyManager.getFamily(new AllOfComponents(typeof(DreamFragment)));

	private Family f_pressY = FamilyManager.getFamily(new AnyOfTags("PressY"));
	private Family f_toggleable = FamilyManager.getFamily(new AllOfComponents(typeof(ToggleableGO), typeof(Animator)));
	private Family f_whiteBoard = FamilyManager.getFamily(new AnyOfTags("Board"));
	private Family f_boardTexture = FamilyManager.getFamily(new AllOfComponents(typeof(ChangePixelColor)));

	public static SaveManager instance;

	private SaveContent saveContent;

	private List<Family> tracedFamilies;

	private string saveFolderPath = "./SaveFiles";
	private string autoSaveFileName = "auto_save";
	private string saveFilesExtension = ".txt";

	private GameObject loadPopup;
	private GameObject loadButtonPrefab;
	private GameObject loadListContainer;
	// the load button in main menu
	private Button menuLoadButton;
	// the load button in the popup window
	private Button popupLoadButton;
	private GameObject selectedSave;

	private GameObject savePopup;
	private GameObject saveButtonPrefab;
	private GameObject saveListContainer;
	private TMP_InputField popupSaveInputfield;
	// popup windows displayed when the player clicks on popupSaveButton
	private GameObject popupSaveInvalid;
	private GameObject popupSaveOverride;
	private GameObject popupSaveDone;

	private GameObject playerGO;

	/// <summary>
	/// Contains the id of each collectable items in the SaveContent array:
	/// 0-introScroll, 1-keyBallBox, 2-wire, 3-keySatchel, 4-mirror, 5-glasses1, 6-glasses2,
	/// 7 to 11-scroll 1 to 5, 12-lamp, 13 to 17-virtuelPuzzleSet 1 to 5
	/// </summary>
	private Dictionary<string, int> collectableItemIDs;

	private string tmpPath;
	private DreamFragment tmpDF;
	private GameObject tmpGO;
	private DreamFragmentToggle tmpDFToggle;
	private ComponentMonitoring[] tmpMonitorsArray;
	private SaveContent tmpSave;
	private FileInfo[] tmpFI;

    public SaveContent SaveContent
	{
		get
		{
			if(saveContent == null)
			{
				saveContent = CreateNewSave();
				Debug.LogError("Tried to access to an invald saveContent. A new save was created.");
				File.AppendAllText("./Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Tried to access to an invald saveContent. A new save was created."));
			}

			playerGO = f_fpsController.First();

			return saveContent;
		}
	}

    public SaveManager()
	{
		if(Application.isPlaying)
		{
			// add traced families to this list and use their id the store them in save content
			//tracedFamilies.Add(f_wrongWords); // 0
			//tracedFamilies.Add(f_wrongChair); // 1

			// set collectable ids dictionary
			collectableItemIDs = new Dictionary<string, int>();
			collectableItemIDs.Add("Intro_Scroll", 0);
			collectableItemIDs.Add("KeyBallBox", 1);
			collectableItemIDs.Add("Wire", 2);
			collectableItemIDs.Add("KeySatchel", 3);
			collectableItemIDs.Add("Mirror", 4);
			collectableItemIDs.Add("Glasses1", 5);
			collectableItemIDs.Add("Glasses2", 6);
			collectableItemIDs.Add("Scroll1", 7);
			collectableItemIDs.Add("Scroll2", 8);
			collectableItemIDs.Add("Scroll3", 9);
			collectableItemIDs.Add("Scroll4", 10);
			collectableItemIDs.Add("Scroll5", 11);
			collectableItemIDs.Add("Lamp", 12);
			collectableItemIDs.Add("PuzzleSet_01", 13);
			collectableItemIDs.Add("PuzzleSet_02", 14);
			collectableItemIDs.Add("PuzzleSet_03", 15);
			collectableItemIDs.Add("PuzzleSet_04", 16);
			collectableItemIDs.Add("PuzzleSet_05", 17);

			// look for the load window
			foreach (GameObject go in f_prefabs)
			{
				if (go.name == "LoadPopup")
				{
					loadPopup = go;
					loadButtonPrefab = go.GetComponent<PrefabContainer>().prefab;
					loadListContainer = go.GetComponentInChildren<GridLayoutGroup>().gameObject;
					foreach (Button b in go.GetComponentsInChildren<Button>())
						if (b.gameObject.name == "LoadButton")
						{
							popupLoadButton = b;
							break;
						}
				}
				else if (go.name == "SavePopup")
				{
					savePopup = go;
					saveButtonPrefab = go.GetComponent<PrefabContainer>().prefab;
					saveListContainer = go.GetComponentInChildren<GridLayoutGroup>().gameObject;
					foreach(Transform child in go.transform)
                    {
                        switch (child.gameObject.name)
                        {
							case "SaveInputField":
								popupSaveInputfield = child.GetComponent<TMP_InputField>();
								break;

							case "Invalid":
								popupSaveInvalid = child.gameObject;
								break;

							case "Override":
								popupSaveOverride = child.gameObject;
								break;

							case "Saved":
								popupSaveDone = child.gameObject;
								break;

							default:
								break;
                        }
                    }
				}
			}
			// find main menu load button 
			foreach (GameObject go in f_menuButtons)
				if (go.name == "Load")
				{
					menuLoadButton = go.GetComponent<Button>();
					break;
				}
			GenerateSaveButtons();
		}
		instance = this;
	}

	private void GenerateSaveButtons()
	{
		// Look for all valid saves in save folder and create buttons for it
		if (loadButtonPrefab)
		{
			// browse save folder
			DirectoryInfo di = new DirectoryInfo(saveFolderPath);
			tmpFI = di.GetFiles(string.Concat("*", saveFilesExtension));
			foreach (FileInfo file in tmpFI)
			{
				tmpSave = LoadFromFile(file.FullName);
				if (tmpSave != null)
				{
					// create button in load UI
					GameObject loadListElem = GameObject.Instantiate(loadButtonPrefab);
					loadListElem.name = Path.GetFileNameWithoutExtension(file.Name);
					loadListElem.GetComponentInChildren<TextMeshProUGUI>().text = loadListElem.name;
					loadListElem.GetComponent<SaveComponent>().content = tmpSave;
					loadListElem.GetComponent<Button>().onClick.AddListener(delegate { SetSelectedSaveButton(loadListElem); });
					loadListElem.transform.SetParent(loadListContainer.transform);
					loadListElem.transform.localScale = Vector3.one;
					GameObjectManager.bind(loadListElem);
					// create button in save UI
					GameObject saveListElem = GameObject.Instantiate(saveButtonPrefab);
					saveListElem.name = Path.GetFileNameWithoutExtension(file.Name);
					saveListElem.GetComponentInChildren<TextMeshProUGUI>().text = saveListElem.name;
					saveListElem.GetComponent<Button>().onClick.AddListener(delegate { SetSaveInputfieldText(saveListElem); });
					saveListElem.transform.SetParent(saveListContainer.transform);
					saveListElem.transform.localScale = Vector3.one;
					GameObjectManager.bind(saveListElem);
				}
			}
		}
		// enable loading button if there are valid saves
		if (loadListContainer && loadListContainer.transform.childCount > 0)
		{
			menuLoadButton.interactable = true;
			// change content size depending on the number of buttons
			loadListContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(loadListContainer.GetComponent<RectTransform>().sizeDelta.x,
				25 * loadListContainer.transform.childCount + 5 * (loadListContainer.transform.childCount - 1));
			saveListContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(saveListContainer.GetComponent<RectTransform>().sizeDelta.x,
				25 * saveListContainer.transform.childCount + 5 * (saveListContainer.transform.childCount - 1));
		}
	}

	/// <summary>
	/// Used to initialize saveContent when a new game is started
	/// </summary>
	/// <returns></returns>
	public SaveContent CreateNewSave()
	{
		SaveContent save = new SaveContent();

		save.virtualPuzzle = LoadGameContent.gameContent.virtualPuzzle;

		save.storyTextCount = 0;
		save.playerPositionRoom = 0;
		save.playingDuration = 0;

		// There are 18 collectable items (including virtual puzzles)
		save.collectableItemsStates = new int[18];
		// There are 14 dream fragments of type 0 and 6 of type 1 with id going from 0 to 19
		save.dreamFragmentsStates = new int[20];
		save.lockedDoorsStates = new bool[3];
		save.toggleablesStates = new bool[8];
		save.boardEraseTexture = null;
		save.boardEraserPosition = f_whiteBoard.First().transform.GetChild(2).position;

		save.iarQueriesStates = new bool[13];

		save.performedActions = new List<SaveContent.LaalysAction>();
		save.receivedHints = new List<SaveContent.HintData>();

		return save;
    }

	public void SetNewSave()
    {
		saveContent = CreateNewSave();
    }

	/// <summary>
	/// Encrypt and save the content of "save" in a file
	/// </summary>
	/// <param name="path"></param>
	public void SaveOnFile(string fileName)
    {
		if(saveContent != null)
		{
			saveContent.saveDate = DateTime.Now;
			saveContent.sessionID = LoadGameContent.sessionID;
			// Check player position to know in which room he is
			if (playerGO.transform.position.x < -17)
				saveContent.playerPositionRoom = 0;
			else if(playerGO.transform.position.x < 0)
				saveContent.playerPositionRoom = 1;
			else if(playerGO.transform.position.x < 27)
				saveContent.playerPositionRoom = 2;
			else
				saveContent.playerPositionRoom = 3;
			saveContent.playingDuration = StoryDisplaying.instance.Duration;
			saveContent.hintCooldown = HelpSystem.instance.HintCooldown;
			saveContent.hintCooldown = saveContent.hintCooldown < 0 ? 0 : saveContent.hintCooldown;

			tmpPath = string.Concat(saveFolderPath, "/", fileName, saveFilesExtension);

			// Create all necessary directories if they don't exist
			Directory.CreateDirectory(Path.GetDirectoryName(tmpPath));

			File.WriteAllText(tmpPath, JsonUtility.ToJson(saveContent, true));
		}
        else
		{
			Debug.LogError("Tried to save with an invalid SaveContent. Nothing was done.");
			File.AppendAllText("./Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - Tried to save with an invalid SaveContent. Nothing was done."));
		}
	}

	/// <summary>
	/// Called when clicking on the save button in the popup or when answering yes to override file
	/// </summary>
	/// <param name="checkName">checkName is false when already checked and this function is called after answering yes to override</param>
	public void TrySaving(bool checkName = true)
    {
        if (!checkName || CheckSaveNameValidity())
        {
            if (checkName)
			{
				// check if this name is already used
				bool usedName = false;
				foreach (Transform child in saveListContainer.transform)
					if (popupSaveInputfield.text == child.gameObject.name)
					{
						usedName = true;
						break;
					}
				if (usedName)
					// display popup asking for override
					GameObjectManager.setGameObjectState(popupSaveOverride, true);
                else
                {
					SaveOnFile(popupSaveInputfield.text);
					GameObjectManager.setGameObjectState(popupSaveDone, true);

					// if the save didn't already exist, add it to the list in the popup
					GameObject saveListElem = GameObject.Instantiate(saveButtonPrefab);
					saveListElem.name = Path.GetFileNameWithoutExtension(popupSaveInputfield.text);
					saveListElem.GetComponentInChildren<TextMeshProUGUI>().text = saveListElem.name;
					saveListElem.GetComponent<Button>().onClick.AddListener(delegate { SetSaveInputfieldText(saveListElem); });
					saveListElem.transform.SetParent(saveListContainer.transform);
					saveListElem.transform.localScale = Vector3.one;
					GameObjectManager.bind(saveListElem);
					saveListContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(saveListContainer.GetComponent<RectTransform>().sizeDelta.x,
						25 * saveListContainer.transform.childCount + 5 * (saveListContainer.transform.childCount - 1));
				}
			}
            else
			{
				SaveOnFile(popupSaveInputfield.text);
				GameObjectManager.setGameObjectState(popupSaveDone, true);
			}
        }
		else
			// display popup for invalid name
			GameObjectManager.setGameObjectState(popupSaveInvalid, true);
	}

	/// <summary>
	/// Save the game in an auto save file
	/// </summary>
	public void AutoSave()
    {
		SaveOnFile(autoSaveFileName);
    }

	/// <summary>
	/// Load the content of a file into saveContent
	/// </summary>
	/// <param name="path"></param>
	public SaveContent LoadFromFile(string path)
	{
		saveContent = null;

		if (File.Exists(path))
        {
            try
            {
				saveContent = JsonUtility.FromJson<SaveContent>(File.ReadAllText(path));
            }
            catch (Exception) { }

			if(saveContent == null)
            {
				saveContent = CreateNewSave();

				Debug.LogError("The save couldn't be loaded because of invalid content. A new save was created.");
				File.AppendAllText("./Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - The save couldn't be loaded because of invalid content. A new save was created."));
			}
        }
        else
		{
			saveContent = CreateNewSave();

			Debug.LogError("The save couldn't be loaded because of invalid file path. A new save was created.");
			File.AppendAllText("./Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - The save couldn't be loaded because of invalid file path. A new save was created."));
		}

		return saveContent;
    }

	/// <summary>
	/// Set the game to the state of saveContent
	/// </summary>
	public void LoadSave()
    {
		// Check save validity
		if(saveContent == null)
		{
			saveContent = CreateNewSave();

			Debug.LogError("The save couldn't be loaded because of invalid content. A new save was created.");
			File.AppendAllText("./Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Error - The save couldn't be loaded because of invalid content. A new save was created."));
		}
        else
		{
			// Load save
			// set virtual puzzle
			saveContent.virtualPuzzle = saveContent.virtualPuzzle || LoadGameContent.gameContent.virtualPuzzle;

			// set session ID
			if (saveContent.sessionID != "")
			{
				LoadGameContent.sessionID = saveContent.sessionID;
				if (LoadGameContent.gameContent.traceToLRS)
					SendStatements.instance.initGBLXAPI();
			}

			// set story reading progression
			StoryDisplaying.instance.LoadStoryProgression(saveContent.storyTextCount);

			// set player position
			f_fpsController.First().gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
			if (saveContent.playerPositionRoom == 1)
				f_fpsController.First().gameObject.transform.position = new Vector3(-11, 2, -1);
			else if (saveContent.playerPositionRoom == 2)
				f_fpsController.First().gameObject.transform.position = new Vector3(11, 2, -2);
			else if (saveContent.playerPositionRoom == 3)
				f_fpsController.First().gameObject.transform.position = new Vector3(30, 2, -2);
			f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber = saveContent.playerPositionRoom;
			// disable black wall at the beginning
			GameObject night = GameObject.Find("Night");
			night.GetComponent<Animator>().enabled = true;
			night.GetComponent<Collider>().enabled = false;

			// set starting time
			f_timer.First().GetComponent<Timer>().startingTime = Time.time - saveContent.playingDuration;

			// set collectable objects states
			foreach(GameObject go in f_linked)
            {
                if (collectableItemIDs.ContainsKey(go.name))
                {
					int id = collectableItemIDs[go.name];
					// if collected, disable it in the scene
					if (saveContent.collectableItemsStates[id] != 0)
						GameObjectManager.setGameObjectState(go, false);
					if (saveContent.collectableItemsStates[id] == 1)
						// enable object in inventory
						GameObjectManager.setGameObjectState(go.GetComponent<LinkedWith>().link, true);

                    switch (id)
                    {
						case 0: // display text on the floor if intro scroll is collected
							if (saveContent.collectableItemsStates[id] != 0)
								GameObjectManager.setGameObjectState(f_pressY.First(), true);
							break;

						case 1: // unlock ballbox if key state is 2
							if (saveContent.collectableItemsStates[id] == 2)
								BallBoxManager.instance.UnlockBallBox();
							break;

						case 2: // display the wire on the plank if its state is 2
							if (saveContent.collectableItemsStates[id] == 2)
								PlankAndWireManager.instance.DisplayWireOnSolution();
							break;

						case 3: // unlock satchel if key state is 2
							if (saveContent.collectableItemsStates[id] == 2)
								SatchelManager.instance.UnlockSatchel();
							break;

						case 4: // put mirror on plank if its state is 2
							if (saveContent.collectableItemsStates[id] == 2)
								PlankAndMirrorManager.instance.PutMirrorOnPlank();
							break;

						// enable corresponding scroll in IAR if state is 1
						case 7:
						case 8:
						case 9:
						case 10:
						case 11:
						// enable corresponding puzzle set in IAR if state is 1
						case 13:
						case 14:
						case 15:
						case 16:
						case 17:
							if (saveContent.collectableItemsStates[id] == 1)
								GameObjectManager.setGameObjectState(go.GetComponent<LinkedWith>().link.GetComponent<LinkedWith>().link.transform.Find(go.name).gameObject, true);
							break;

						default:
							break;
                    }
                }
            }

			// set dream fragments states
			foreach(GameObject go in f_dreamFragments)
            {
				tmpDF = go.GetComponent<DreamFragment>();
				if(tmpDF.type < 2)
                {
					if (saveContent.dreamFragmentsStates[tmpDF.id] != 0)
					{
						// if collected, turn off fragment in scene
						DreamFragmentCollecting.instance.TurnOffDreamFragment(go);
						if (tmpDF.type == 0)
						{
							// enable in IAR
							tmpGO = go.GetComponent<LinkedWith>().link;
							GameObjectManager.setGameObjectState(tmpGO, true);
							if(saveContent.dreamFragmentsStates[tmpDF.id] == 2)
							{
								// set as seen in IAR
								tmpDFToggle = tmpGO.GetComponent<DreamFragmentToggle>();
								tmpGO.GetComponentInChildren<Image>().sprite = tmpDFToggle.onState;
								tmpDFToggle.currentState = tmpDFToggle.onState;
							}
						}
					}

                }
            }

			// set doors states
			if(saveContent.lockedDoorsStates[0])
				LockResolver.instance.UnlockIntroWall();
			if (saveContent.lockedDoorsStates[1])
				LoginManager.instance.UnlockLoginDoor();
			if (saveContent.lockedDoorsStates[2])
				LockResolver.instance.UnlockRoom2Fences();

			// set toggleables states
			foreach(GameObject go in f_toggleable)
            {
				if(go.tag == "Chair")
                {
					if(go.GetComponent<IsSolution>() && saveContent.toggleablesStates[0])
						go.GetComponent<Animator>().SetTrigger("turnOn");
                    else
                    {
						int id = -1;
						int.TryParse(go.name.Substring(go.name.Length - 2, 1), out id);
						if (id > 0 && id < 6 && saveContent.toggleablesStates[id])
							go.GetComponent<Animator>().SetTrigger("turnOn");
					}
				}
				else if(go.name == "Table" && saveContent.toggleablesStates[6])
					go.GetComponent<Animator>().SetTrigger("turnOn");
				else if (saveContent.toggleablesStates[7])
					go.GetComponent<Animator>().SetTrigger("turnOn");
			}

			// load room 3 board texture
			Texture2D tex = new Texture2D(1, 1);
			tex.LoadImage(saveContent.boardEraseTexture);
			f_boardTexture.First().GetComponent<Renderer>().material.mainTexture = tex;
			// set eraser position
			f_whiteBoard.First().transform.GetChild(2).position = saveContent.boardEraserPosition;

			// set queries states
			foreach (GameObject query in f_queries)
			{
				if(query.tag == "Q-R3")
                {
					switch (query.name)
					{
						case "Q1":
							if (saveContent.iarQueriesStates[9])
								IARQueryEvaluator.instance.IarCheckAnswer(query, true);
							break;

						case "Q2":
							if (saveContent.iarQueriesStates[10])
								IARQueryEvaluator.instance.IarCheckAnswer(query, true);
							break;

						case "Q3":
							if (saveContent.iarQueriesStates[11])
								IARQueryEvaluator.instance.IarCheckAnswer(query, true);
							break;

						case "Q4":
							if (saveContent.iarQueriesStates[12])
								IARQueryEvaluator.instance.IarCheckAnswer(query, true);
							break;

						default:
							break;
					}
				}
                else if (saveContent.iarQueriesStates[GetQueryID(query)])
					IARQueryEvaluator.instance.IarCheckAnswer(query, true);
			}
			// display gears if all Q-R1 are solved
			bool roomQueriesAnswered = true;
			for (int i = 0; i < 3; i++)
				roomQueriesAnswered = roomQueriesAnswered && saveContent.iarQueriesStates[i];
            if (roomQueriesAnswered)
            {
				// display gears
				GameObjectManager.setGameObjectState(f_gearsSet.First(), true);
				// Hide queries
				GameObjectManager.setGameObjectState(f_gearsSet.First().GetComponent<LinkedWith>().link, false);
			}
			// display password to room 3 if Q-R2 are solvedroomQueriesAnswered = true;
			for (int i = 3; i < 9; i++)
				roomQueriesAnswered = roomQueriesAnswered && saveContent.iarQueriesStates[i];
            if (roomQueriesAnswered)
            {
				// disable queries
				GameObjectManager.setGameObjectState(f_queriesRoom2.First().transform.parent.gameObject, false);
				// enable final code
				GameObjectManager.setGameObjectState(f_queriesRoom2.First().transform.parent.parent.GetChild(1).gameObject, true);
			}

            // set gears enigma state
            if (saveContent.gearEnigmaState)
				IARGearsEnigma.instance.SolveGearsEnigma(!saveContent.lockedDoorsStates[1]);

			// set hint cooldown
			HelpSystem.instance.SetPlayerHintTimer(saveContent.hintCooldown);
			// generate received hints
			foreach(SaveContent.HintData hint in saveContent.receivedHints)
			{
				// find the monitoring component corresponding to this hint data
				bool monitorFound = false;
				foreach (GameObject monitor in f_componentMonitoring)
				{
					tmpMonitorsArray = monitor.GetComponents<ComponentMonitoring>();
					foreach (ComponentMonitoring monitoringComponent in tmpMonitorsArray)
						if (monitoringComponent.id == hint.monitorID)
						{
							// generate hint
							if (hint.wrongAnswer == "")
								HelpSystem.instance.DisplayHint(monitoringComponent, hint.name, hint.level, hint.seen, true);
							// if hint was given after a wrong answer
							else
								HelpSystem.instance.CreateWrongAnswerHint(monitoringComponent, hint.name, hint.wrongAnswer, hint.seen);
							monitorFound = true;
							break;
						}
					if (monitorFound)
						break;
				}
            }
		}
	}

	public void LoadSelectedSave()
    {
        if (selectedSave)
        {
			saveContent = selectedSave.GetComponent<SaveComponent>().content;
			LoadSave();
			GameObjectManager.setGameObjectState(loadPopup, false);
			if(selectedSave.name != autoSaveFileName)
				popupSaveInputfield.text = selectedSave.name;
			MenuSystem.instance.StartGame();
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
	protected override void onProcess(int familiesUpdateCount) {
	}

	public int GetCollectableItemID(GameObject go)
    {
		if (go && collectableItemIDs.ContainsKey(go.name))
			return collectableItemIDs[go.name];

		return -1;
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="query"></param>
	/// <param name="contextQ3">The same context used to identify the solved enigma in statements</param>
	/// <returns></returns>
	public int GetQueryID(GameObject query, string contextQ3 = "")
    {
		int room = -1, queryID = -1, id = -1;

        if (query)
		{
			int.TryParse(query.tag.Substring(query.tag.Length - 1, 1), out room);
			if(room == 3)
            {
                switch (contextQ3)
                {
					case "Enigma16":
						id = 10;
						break;

					case "Lamp":
						id = 11;
						break;

					case "WhiteBoard":
						id = 12;
						break;

					default:
						if (contextQ3.Contains("Puzzle"))
							id = 9;
						break;
                }
            }
            else
			{
				int.TryParse(query.name.Substring(query.name.Length - 1, 1), out queryID);
				if (room > 0 && queryID > 0)
					id = 3 * (room - 1) + queryID - 1;
			}
		}

		return id;
    }

	public void SetHintAsSeen(HintContent hint)
	{
		int l = saveContent.receivedHints.Count;
		SaveContent.HintData data;
		for (int i = 0; i < l; i++)
		{
			data = saveContent.receivedHints[i];
			// Find in the list the HintData corresponding to this HintContent
			if (hint.monitor.id == data.monitorID && hint.actionName == data.name)
			{
				// Set the HintData as seen
				saveContent.receivedHints[i] = new SaveContent.HintData(data.monitorID, data.name, data.level, data.wrongAnswer, true);
				break;
			}
		}
	}

	public void RemoveHintFromSave(HintContent hint)
	{
		int l = saveContent.receivedHints.Count;
		SaveContent.HintData data;
		int index = -1;
		for (int i = 0; i < l; i++)
		{
			data = saveContent.receivedHints[i];
			// Find in the list the HintData corresponding to this HintContent
			if (hint.monitor.id == data.monitorID && hint.actionName == data.name)
			{
				index = i;
				break;
			}
		}

		if (index > -1)
			saveContent.receivedHints.RemoveAt(index);
	}

	/// <summary>
	/// Used for the loading popup
	/// </summary>
	/// <param name="loadButton"></param>
	public void SetSelectedSaveButton(GameObject loadButton)
    {
		if (loadButton.GetComponent<SaveComponent>())
		{
			selectedSave = loadButton;
			popupLoadButton.interactable = true;
		}
    }

	/// <summary>
	/// Used when a button of the saving popup is clicked
	/// </summary>
	/// <param name="saveButton"></param>
	public void SetSaveInputfieldText(GameObject saveButton)
    {
		if(saveButton)
			popupSaveInputfield.text = saveButton.name;
	}

	/// <summary>
	/// Called when trying to save
	/// </summary>
	public bool CheckSaveNameValidity()
    {
		bool isValid = true;

		// remove file extension
		if (popupSaveInputfield.text.EndsWith(saveFilesExtension))
			popupSaveInputfield.text = popupSaveInputfield.text.Substring(popupSaveInputfield.text.Length - saveFilesExtension.Length, saveFilesExtension.Length);

		isValid = popupSaveInputfield.text != "";

		char[] chars = Path.GetInvalidFileNameChars();

		foreach(char c in chars)
            if (popupSaveInputfield.text.IndexOf(c) != -1)
            {
				isValid = false;
				break;
            }

		return isValid;
    }

	/// <summary>
	/// Called when changing tab or closing IAR
	/// </summary>
	public void CloseSavePopup()
    {
        if (savePopup.activeSelf)
		{
			GameObjectManager.setGameObjectState(savePopup, false);
			GameObjectManager.setGameObjectState(popupSaveInvalid, false);
			GameObjectManager.setGameObjectState(popupSaveOverride, false);
			GameObjectManager.setGameObjectState(popupSaveDone, false);
		}
}
}