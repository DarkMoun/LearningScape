﻿using UnityEngine;
using FYFY;
using FYFY_plugins.Monitoring;

public class ToggleObject : FSystem {

    // This system enables to manage in game toggleable objects

	private Family f_toggleable = FamilyManager.getFamily(new AllOfComponents(typeof(ToggleableGO), typeof(Highlighted))); // Highlighted is dynamically added by Highlither system
	private Family f_wrongChair = FamilyManager.getFamily(new AnyOfTags("Chair"), new AllOfComponents(typeof(ToggleableGO)), new NoneOfComponents(typeof(IsSolution)));
    private Family f_lid = FamilyManager.getFamily(new AnyOfTags("ChestLid"));

    private float speed;

    //variables used to toggle chairs in the first room
	private GameObject[] togglingChairsDown;
	private int togglingChairsDownCount = 0;
	private GameObject[] togglingChairsUp;
	private int togglingChairsUpCount = 0;

    //variables used to toggle the table in the first room
	private bool toggleTableDown = false;
	private bool toggleTableUp = false;
	private GameObject table;
	private float heightBeforeTableToggle;
	private bool tableGoinUp;
	private Vector3 tableTarget;

    //variables used to open the chest room 3
    private GameObject chestLid;
    private bool openingChest = false;
    private bool closingChest = false;

    //temporary variables
    private GameObject tmpGO;
	private ToggleableGO tmpToggleableGO;
    private float tmpRotationCount = 0;

    public static ToggleObject instance;

	public ToggleObject(){
        if (Application.isPlaying)
        {
            togglingChairsDown = new GameObject[6];
            togglingChairsUp = new GameObject[6];
            table = GameObject.Find("Table");

            chestLid = f_lid.First();
        }
        instance = this;
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        speed = 50 * Time.deltaTime;

        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("X_button"))
        {
            int nbToggleable = f_toggleable.Count;
            for (int i = 0; i < nbToggleable; i++)
            {
                tmpGO = f_toggleable.getAt(i);
                tmpToggleableGO = tmpGO.GetComponent<ToggleableGO>();
                //if an untoggled object is clicked
                if (!tmpToggleableGO.toggled)
                {
                    tmpToggleableGO.toggled = true;
                    if (tmpGO.tag == "Chair")
                    {
                        if (tmpGO.transform.rotation.eulerAngles.x < 0.0001f) //if chair up
                        {
                            //add the chair to toggling down list
                            //all objects in this list will be toggled down
                            togglingChairsDown[togglingChairsDownCount] = tmpGO;
                            togglingChairsDownCount++;
                            GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { name = "turnOn", performedBy = "player", family = tmpGO.GetComponent<IsSolution>() ? null : f_wrongChair });
                        }
                        else if (tmpGO.transform.rotation.eulerAngles.x > 89.9999f) //if chair down
                        {
                            //add the chair to toggling up list
                            //all objects in this list will be toggled up
                            togglingChairsUp[togglingChairsUpCount] = tmpGO;
                            togglingChairsUpCount++;
                        }
                        else
                        {
                            //if the chair is stuck in toggling position (angle between 0 and 90), set its position to up
                            tmpGO.transform.rotation = Quaternion.Euler(0, tmpGO.transform.rotation.eulerAngles.y, tmpGO.transform.rotation.eulerAngles.z);
                        }
                    }
                    else if (tmpGO.name == "Table")
                    {
                        heightBeforeTableToggle = table.transform.position.y;
                        tableGoinUp = true; //animation moving the table up while rotating it
                        tableTarget = new Vector3(table.transform.position.x, heightBeforeTableToggle + 1, table.transform.position.z); //a point above the table
                        //start toggling table up/down depending on its current state (0 or 180)
                        if (tmpGO.transform.rotation.eulerAngles.z > 90)
                            toggleTableUp = true;
                        else
                        {
                            toggleTableDown = true;
                            GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { name = "turnOn", performedBy = "player" });
                        }
                    }
                    else if (tmpGO.name == "boite")
                    {
                        //start toggling the chest opened/cloesed depending on its current state
                        if (chestLid.transform.localRotation.eulerAngles.x < 1)
                        {
                            openingChest = true;
                            // puzzle enigma require chest opened
                            GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { overrideName = LoadGameContent.gameContent.virtualPuzzle ? "turnOn_VirtualPuzzleEnigma" : "turnOn_PhysicalPuzzleEnigma", performedBy = "player" });

                            // lamp enigma require chest opened also
                            GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { overrideName = "turnOn_LampEnigma", performedBy = "player" });
                        }
                        else
                            closingChest = true;
                    }
                    GameObjectManager.addComponent<ActionPerformedForLRS>(tmpGO, new { verb = "interacted", objectType = "toggable", objectName = tmpGO.name });
                    break;
                }
            }
        }

        //toggle chairs
        //down
        for (int i = 0; i < togglingChairsDownCount; i++)
        {
            //rotate the chair
            togglingChairsDown[i].transform.rotation = Quaternion.RotateTowards(togglingChairsDown[i].transform.rotation, Quaternion.Euler(90, togglingChairsDown[i].transform.rotation.eulerAngles.y, togglingChairsDown[i].transform.rotation.eulerAngles.z), 10 * speed);
            if (togglingChairsDown[i].transform.rotation.eulerAngles.x > 89.9999f)
            {
                //stop animation when the final position is reached
                togglingChairsDown[i].GetComponent<ToggleableGO>().toggled = false;
                togglingChairsDown[i] = togglingChairsDown[togglingChairsDownCount - 1];
                togglingChairsDownCount--;
            }
        }
        //up
        for (int i = 0; i < togglingChairsUpCount; i++)
        {
            //rotate the chair
            togglingChairsUp[i].transform.rotation = Quaternion.RotateTowards(togglingChairsUp[i].transform.rotation, Quaternion.Euler(0, togglingChairsUp[i].transform.rotation.eulerAngles.y, togglingChairsUp[i].transform.rotation.eulerAngles.z), 10 * speed);
            if (togglingChairsUp[i].transform.rotation.eulerAngles.x < 0.0001f)
            {
                GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { name = "turnOff", performedBy = "player", family = togglingChairsUp[i].GetComponent<IsSolution>() ? null : f_wrongChair });
                //stop animation when the final position is reached
                togglingChairsUp[i].GetComponent<ToggleableGO>().toggled = false;
                togglingChairsUp[i] = togglingChairsUp[togglingChairsUpCount - 1];
                togglingChairsUpCount--;
            }
        }

        //toggle table
        //down
        if (toggleTableDown)
        {
            //move table up
            if (tableGoinUp)
            {
                //move slower when close to top
                float dist = (tableTarget - table.transform.position).magnitude;
                table.transform.position = Vector3.MoveTowards(table.transform.position, tableTarget, (0.01f + dist / 10) * speed);
                if (table.transform.position == tableTarget)
                {
                    //start going down when table reaches top
                    tableTarget = new Vector3(table.transform.position.x, heightBeforeTableToggle, table.transform.position.z);
                    tableGoinUp = false;
                }
            }
            //move table down
            else
            {
                //move slower when close to top
                float dist = (tableTarget - table.transform.position).magnitude;
                table.transform.position = Vector3.MoveTowards(table.transform.position, tableTarget, (0.01f + (1 - dist) / 10) * speed);
                if (table.transform.position == tableTarget)
                {
                    //stop animation when table comes back to initial position
                    toggleTableDown = false;
                    table.GetComponent<ToggleableGO>().toggled = false;
                }
            }
            //rotate table
            table.transform.rotation = Quaternion.RotateTowards(table.transform.rotation, Quaternion.Euler(table.transform.rotation.eulerAngles.x, table.transform.rotation.eulerAngles.y, 180), 5 * speed);
        }
        //up
        else if (toggleTableUp)
        {
            //move table up
            if (tableGoinUp)
            {
                //move slower when close to top
                float dist = (tableTarget - table.transform.position).magnitude;
                table.transform.position = Vector3.MoveTowards(table.transform.position, tableTarget, (0.01f + dist / 10) * speed);
                if (table.transform.position == tableTarget)
                {
                    //start going down when table reaches top
                    tableTarget = new Vector3(table.transform.position.x, heightBeforeTableToggle, table.transform.position.z);
                    tableGoinUp = false;
                }
            }
            //move table down
            else
            {
                //move slower when close to top
                float dist = (tableTarget - table.transform.position).magnitude;
                table.transform.position = Vector3.MoveTowards(table.transform.position, tableTarget, (0.01f + (1 - dist) / 10) * speed);
                if (table.transform.position == tableTarget)
                {
                    GameObjectManager.addComponent<ActionPerformed>(table, new { name = "turnOff", performedBy = "player" });
                    //stop animation when table comes back to initial position
                    toggleTableUp = false;
                    table.GetComponent<ToggleableGO>().toggled = false;
                }
            }
            //rotate table
            table.transform.rotation = Quaternion.RotateTowards(table.transform.rotation, Quaternion.Euler(table.transform.rotation.eulerAngles.x, table.transform.rotation.eulerAngles.y, 0), 5 * speed);
        }

        //toggle chest room3
        //open
        if (openingChest)
        {
            chestLid.transform.Rotate((4 - (float)tmpRotationCount / 120 * 2) * 100 * Time.deltaTime, 0, 0);
            tmpRotationCount += (4 - (float)tmpRotationCount / 120 * 2) * 100 * Time.deltaTime;
            if (tmpRotationCount > 120)
            {
                chestLid.transform.Rotate(120 - tmpRotationCount, 0, 0);
                tmpRotationCount = 0;
                openingChest = false;
                chestLid.transform.parent.gameObject.GetComponent<ToggleableGO>().toggled = false;
            }
        }
        //close
        else if (closingChest)
        {
            chestLid.transform.Rotate(-(4 - (float)tmpRotationCount / 120 * 2) * 100 * Time.deltaTime, 0, 0);
            tmpRotationCount += (4 - (float)tmpRotationCount / 120 * 2) * 100 * Time.deltaTime;
            if (tmpRotationCount > 120)
            {
                chestLid.transform.Rotate(-(120 - tmpRotationCount), 0, 0);
                tmpRotationCount = 0;
                closingChest = false;
                chestLid.transform.parent.gameObject.GetComponent<ToggleableGO>().toggled = false;

                // When the chest is closed, we have to propagate informayion both in puzzle and lamp enigmas
                GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { overrideName = LoadGameContent.gameContent.virtualPuzzle ? "turnOff_VirtualPuzzleEnigma" : "turnOff_PhysicalPuzzleEnigma", performedBy = "player" });
                GameObjectManager.addComponent<ActionPerformed>(tmpGO, new { overrideName = "turnOff_LampEnigma", performedBy = "player" });
            }
        }
    }
}