﻿using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using FYFY_plugins.Monitoring;
using FYFY_plugins.TriggerManager;
using System.Collections.Generic;

public class WhiteBoardManager : FSystem {
    
    // this system manage the whiteboard and effacer

    private Family f_whiteBoard = FamilyManager.getFamily(new AnyOfTags("Board"));
    private Family f_focusedWhiteBoard = FamilyManager.getFamily(new AnyOfTags("Board"), new AllOfComponents(typeof(ReadyToWork)));
    private Family f_closeWhiteBoard = FamilyManager.getFamily (new AnyOfTags ("Board", "Eraser", "BoardTexture", "InventoryElements"), new AllOfComponents(typeof(PointerOver)));
    private Family f_eraserFocused = FamilyManager.getFamily(new AnyOfTags("Eraser"), new AllOfComponents(typeof(PointerOver)));
    private Family f_boardRemovableWords = FamilyManager.getFamily(new AnyOfTags("BoardRemovableWords"));
    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_wordSpheres = FamilyManager.getFamily(new AllOfComponents(typeof(EraserTrigger), typeof(Triggered3D)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));

    //board
    private GameObject selectedBoard;
    private GameObject eraser;
    public static bool eraserDragged = false;
    private float distToBoard;
    private Color prevColor;

    //List of triggers still active for each word (the words are differenciated by there position)
    private Dictionary<Vector3, List<GameObject>> triggeredSpheres;

    private List<Vector2> tmpListVector2;

    public static WhiteBoardManager instance;

    public WhiteBoardManager()
    {
        if (Application.isPlaying)
        {
            //initialise variables
            eraser = f_whiteBoard.First().transform.GetChild(2).gameObject;

            // set all board's removable words to "occludable"
            // the occlusion is then made by an invisible material that hides all objects behind it having this setting
            foreach (GameObject word in f_boardRemovableWords)
            {
                foreach (Renderer r in word.GetComponentsInChildren<Renderer>())
                    r.material.renderQueue = 2001;
            }

            f_focusedWhiteBoard.addEntryCallback(onReadyToWorkOnWhiteBoard);
            f_eraserFocused.addEntryCallback(onEnterEraser);
            f_eraserFocused.addExitCallback(onExitEraser);
            f_wordSpheres.addEntryCallback(onSphereTriggered);

            triggeredSpheres = new Dictionary<Vector3, List<GameObject>>();
            //Generate spheres on each word that will be triggered by the eraser to detect if the word has been erased
            GameObject tmpGo;
            GameObject tmpSphere;
            int nb = f_boardRemovableWords.Count;
            for(int i = 0; i < nb; i++)
            {
                tmpGo = f_boardRemovableWords.getAt(i);
                tmpListVector2 = PointsFromCenter(tmpGo.GetComponent<RectTransform>().sizeDelta.x, tmpGo.GetComponent<RectTransform>().sizeDelta.y);
                //Generates triggers on the points generated by "PointsFromCenter"
                foreach (Vector2 v in tmpListVector2)
                {
                    tmpSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    tmpSphere.transform.position = tmpGo.transform.position + new Vector3(v.x, v.y, 0);
                    tmpSphere.transform.localScale = Vector3.one * 0.05f;
                    tmpSphere.layer = 13;
                    tmpSphere.GetComponent<SphereCollider>().isTrigger = true;
                    tmpSphere.GetComponent<MeshRenderer>().enabled = false;
                    tmpSphere.transform.SetParent(tmpGo.transform);
                    tmpSphere.AddComponent<TriggerSensitive3D>();
                    tmpSphere.AddComponent<EraserTrigger>();
                    //Initialise the "triggeredSpheres" dictionary
                    if (!triggeredSpheres.ContainsKey(tmpSphere.transform.parent.position))
                        triggeredSpheres.Add(tmpSphere.transform.parent.position, new List<GameObject>());
                    triggeredSpheres[tmpSphere.transform.parent.position].Add(tmpSphere);
                    GameObjectManager.bind(tmpSphere);
                }
            }
        }
        instance = this;
    }

    private void onReadyToWorkOnWhiteBoard(GameObject go)
    {
        selectedBoard = go;
        distToBoard = (f_player.First().transform.position - selectedBoard.transform.position).magnitude;

        // Launch this system
        instance.Pause = false;

        GameObjectManager.addComponent<ActionPerformed>(go, new { name = "turnOn", performedBy = "player" });
    }

    private void onEnterEraser (GameObject go)
    {
        Renderer rend = eraser.GetComponent<Renderer>();
        prevColor = rend.material.GetColor("_EmissionColor");
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", Color.yellow * Mathf.LinearToGammaSpace(0.8f));
        if(!eraserDragged)
            GameObjectManager.addComponent<ActionPerformedForLRS>(go, new { verb = "highlighted", objectType = "draggable", objectName = go.name });
    }

    private void onExitEraser(int instanceId)
    {
        eraser.GetComponent<Renderer>().material.SetColor("_EmissionColor", prevColor);
    }

    //Calculate points equally distributed on an area depending on width, height and eraserWidth
    private List<Vector2> PointsFromCenter(float width, float height)
    {
        if (width == 0 || height == 0)
            return null;
        
        float eraserWidth = 2.1f;
        List<Vector2> points = new List<Vector2>();

        int nbOfHorizontal, nbOfVertical;
        nbOfHorizontal = (int)(width / eraserWidth) + 1;
        nbOfVertical = (int)(height / eraserWidth) + 1;

        float horizontalStep, verticalStep;
        horizontalStep = width / (nbOfHorizontal + 1);
        verticalStep = height / (nbOfVertical + 1);

        for (int i = 0; i < nbOfHorizontal; i++)
            for (int j = 0; j < nbOfVertical; j++)
                points.Add(new Vector2(-width / 2 + horizontalStep * (i + 1), -height / 2 + verticalStep * (j + 1)) / 7);
        
        return points;
    }

    private void onSphereTriggered(GameObject go)
    {
        triggeredSpheres[go.transform.parent.position].Remove(go);
        if(triggeredSpheres[go.transform.parent.position].Count == 0)
        {
            GameObjectManager.addComponent<ActionPerformed>(go.transform.parent.gameObject, new { name = "perform", performedBy = "player" });
        }
        GameObjectManager.setGameObjectState(go, false);
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
        if (selectedBoard)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed (because Escape close IAR)
            if (((f_closeWhiteBoard.Count == 0 && Input.GetMouseButtonDown(0)) || (Input.GetKeyDown(KeyCode.Escape) && f_iarBackground.Count == 0)))
                ExitWhiteBoard();
            else
            {
                if (eraser.GetComponent<PointerOver>() && Input.GetMouseButtonDown(0))
                {
                    //start dragging eraser when it s clicked
                    eraserDragged = true;

                    GameObjectManager.addComponent<ActionPerformed>(eraser, new { name = "turnOn", performedBy = "player" });
                    GameObjectManager.addComponent<ActionPerformedForLRS>(eraser, new { verb = "dragged", objectType = "draggable", objectName = eraser.name });
                }
                if (eraserDragged)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        //stop dragging eraser when the click is released
                        eraserDragged = false;
                        GameObjectManager.addComponent<ActionPerformed>(eraser, new { name = "turnOff", performedBy = "player" });
                        GameObjectManager.addComponent<ActionPerformedForLRS>(eraser, new { verb = "released", objectType = "draggable", objectName = eraser.name });
                    }
                    else
                    {
                        //move eraser to mouse position
                        Vector3 mousePos = selectedBoard.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distToBoard)));
                        eraser.transform.localPosition = new Vector3(mousePos.x, eraser.transform.localPosition.y, mousePos.z);
                        //prevent eraser from going out of the board
                        if (eraser.transform.localPosition.x > 0.021f)
                        {
                            eraser.transform.localPosition += Vector3.right * (0.021f - eraser.transform.localPosition.x);
                        }
                        else if (eraser.transform.localPosition.x < -0.021f)
                        {
                            eraser.transform.localPosition += Vector3.right * (-0.021f - eraser.transform.localPosition.x);
                        }
                        if (eraser.transform.localPosition.z > 0.016f)
                        {
                            eraser.transform.localPosition += Vector3.forward * (0.016f - eraser.transform.localPosition.z);
                        }
                        else if (eraser.transform.localPosition.z < -0.016f)
                        {
                            eraser.transform.localPosition += Vector3.forward * (-0.016f - eraser.transform.localPosition.z);
                        }
                    }
                }
            }
        }
	}

    private void ExitWhiteBoard()
    {
        if(eraserDragged)
            GameObjectManager.addComponent<ActionPerformed>(eraser, new { name = "turnOff", performedBy = "system" });
        eraserDragged = false;
        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedBoard);

        GameObjectManager.addComponent<ActionPerformed>(selectedBoard, new { name = "turnOff", performedBy = "player" });
        GameObjectManager.addComponent<ActionPerformedForLRS>(selectedBoard, new { verb = "exited", objectType = "interactable", objectName = selectedBoard.name });

        selectedBoard = null;

        // Pause this system
        instance.Pause = true;
    }
}