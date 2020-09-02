﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;
using FYFY_plugins.Monitoring;

public class LoginManager : FSystem {

    // this system manage the login panel (mastermind)

    private Family f_focusedLogin = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_closeLogin = FamilyManager.getFamily(new AnyOfTags("Login", "InventoryElements", "HUD_TransparentOnMove"), new AllOfComponents(typeof(PointerOver)));
    private Family f_mainWindow = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(PointerSensitive)));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_door = FamilyManager.getFamily(new AnyOfTags("Door"), new AllOfComponents(typeof(Animator)));

    private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family f_gameRooms = FamilyManager.getFamily(new AnyOfTags("GameRooms"));
    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));

    // Selectable Component is dynamically added by IARGearsEnigma when this enigma is solved => this is a sure condition to know that login is unlocked
    private Family f_loginUnlocked = FamilyManager.getFamily(new AnyOfTags("Login"), new AllOfComponents(typeof(Selectable)));

    private Family f_storyDisplayer = FamilyManager.getFamily(new AllOfComponents(typeof(StoryText)));

    private GameObject selectedLoginPanel;
    private Vector3 playerGoBackPosition;

    private float speed;
    private InputField ifConnectionR2;
    public static int passwordSolution;

    private TextMeshProUGUI connectionAnswerCheck1;
    private TextMeshProUGUI connectionAnswerCheck2;
    private TextMeshProUGUI connectionAnswerCheck3;
    private Color cacGreen;
    private Color cacOrange;
    private Color cacRed;

    private bool goBack = false;

    private GameObject door;

    public static LoginManager instance;

    public LoginManager()
    {
        if (Application.isPlaying)
        {
            InputField inputField = f_mainWindow.First().transform.GetChild(1).GetComponent<InputField>();

            ifConnectionR2 = inputField;

            // get fourth child of the password and backup answer UI notifications
            GameObject answerCheck = inputField.gameObject.transform.GetChild(3).gameObject;
            connectionAnswerCheck1 = answerCheck.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            cacGreen = connectionAnswerCheck1.color;
            connectionAnswerCheck2 = answerCheck.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            cacOrange = connectionAnswerCheck2.color;
            connectionAnswerCheck3 = answerCheck.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            cacRed = connectionAnswerCheck3.color;

            f_loginUnlocked.addEntryCallback(onLoginUnlocked);
            f_focusedLogin.addEntryCallback(onReadyToWorkOnLogin);

            door = f_door.First();
        }
        instance = this;
    }

    private void onLoginUnlocked(GameObject go)
    {
        // launch animation of login protection
        GameObject loginCover = go.transform.GetChild(0).gameObject; // the first child is the cover
        playerGoBackPosition = go.transform.position + (Vector3.left*3f) - (Vector3.up);

        GameObjectManager.addComponent<PlaySound>(loginCover, new { id = 9 }); // id refer to FPSController AudioBank
        loginCover.GetComponent<Animator>().enabled = true; // enable animation
    }

    private void onReadyToWorkOnLogin(GameObject go)
    {
        selectedLoginPanel = go;

        // Launch this system
        instance.Pause = false;
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
        if (selectedLoginPanel)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed (because Escape close IAR)
            if (((f_closeLogin.Count == 0 && Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Cancel") && f_iarBackground.Count == 0)) && !goBack)
            {
                ExitLogin();
            }
        }

        speed = Time.deltaTime;

        if (goBack)
        {
            f_player.First().transform.position = Vector3.MoveTowards(f_player.First().transform.position, playerGoBackPosition, speed);
            if (f_player.First().transform.position == playerGoBackPosition)
            {
                // show story
                f_storyDisplayer.First().GetComponent<StoryText>().storyProgression++;
                StoryDisplaying.instance.Pause = false;
                // Enable IAR second screen
                GameObject IARsecondScreen = f_mainWindow.First().GetComponentInChildren<LinkedWith>().link;
                GameObjectManager.setGameObjectState(IARsecondScreen, true); // enable questions tab
                f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber = 2;
                // exit login
                ExitLogin();
                goBack = false;
            }
        }
	}

    private void ExitLogin()
    {
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedLoginPanel);

        selectedLoginPanel = null;

        // Pause this system
        instance.Pause = true;
    }

    public void CheckMastermindAnswer() //mastermind
    {
        int answer;
        int.TryParse(ifConnectionR2.text, out answer);
        if (ifConnectionR2.text == "")
            answer = -1;

        if (answer == passwordSolution) //if the answer is correct
        {
            //show correct answer feedback for the 3 numbers
            connectionAnswerCheck1.text = "O";
            connectionAnswerCheck1.color = cacGreen;
            connectionAnswerCheck2.text = "O";
            connectionAnswerCheck2.color = cacGreen;
            connectionAnswerCheck3.text = "O";
            connectionAnswerCheck3.color = cacGreen;
            // enable rooms two and three
            GameObjectManager.setGameObjectState(f_gameRooms.First().transform.GetChild(2).gameObject, true);
            GameObjectManager.setGameObjectState(f_gameRooms.First().transform.GetChild(3).gameObject, true);
            // solution found play animation
            goBack = true;
            GameObjectManager.addComponent<PlaySound>(door, new { id = 9 }); // id refer to FPSController AudioBank
            door.GetComponent<Animator>().enabled = true; // enable animation
            // lock login
            GameObjectManager.removeComponent<Selectable>(f_loginUnlocked.First());
        }
        else
        {
            //else, feedback following the rules of mastermind ('O' correct, '?' right number but wrong place, 'X' wrong number)

            ifConnectionR2.ActivateInputField();
            int answerHundreds = answer / 100;
            int answerTens = answer / 10 % 10;
            int answerUnits = answer % 10;
            int solutionHundreds = passwordSolution / 100;
            int solutionTens = passwordSolution / 10 % 10;
            int solutionUnits = passwordSolution % 10;

            if (answerHundreds == solutionHundreds)
            {
                connectionAnswerCheck1.text = "O";
                connectionAnswerCheck1.color = cacGreen;
            }
            else if ((answerTens != solutionTens && answerHundreds == solutionTens) || (answerUnits != solutionUnits && answerHundreds == solutionUnits))
            {
                connectionAnswerCheck1.text = "?";
                connectionAnswerCheck1.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck1.text = "X";
                connectionAnswerCheck1.color = cacRed;
            }

            if (answerTens == solutionTens)
            {
                connectionAnswerCheck2.text = "O";
                connectionAnswerCheck2.color = cacGreen;
            }
            else if ((answerHundreds != solutionHundreds && answerTens == solutionHundreds) || (answerUnits != solutionUnits && answerTens == solutionUnits))
            {
                connectionAnswerCheck2.text = "?";
                connectionAnswerCheck2.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck2.text = "X";
                connectionAnswerCheck2.color = cacRed;
            }

            if (answerUnits == solutionUnits)
            {
                connectionAnswerCheck3.text = "O";
                connectionAnswerCheck3.color = cacGreen;
            }
            else if ((answerHundreds != solutionHundreds && answerUnits == solutionHundreds) || (answerTens != solutionTens && answerUnits == solutionTens))
            {
                connectionAnswerCheck3.text = "?";
                connectionAnswerCheck3.color = cacOrange;
            }
            else
            {
                connectionAnswerCheck3.text = "X";
                connectionAnswerCheck3.color = cacRed;
            }
        }
    }

    public void OnEndEditMastermindAnswer()
    {
        if (Input.GetButtonDown("Submit"))
            CheckMastermindAnswer();
    }
}