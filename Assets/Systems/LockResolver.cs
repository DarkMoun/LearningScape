﻿using UnityEngine;
using System.Collections.Generic;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using FYFY_plugins.Monitoring;

public class LockResolver : FSystem {

    // THis system manages lockers with three wheels

    // all selectable locker
    private Family f_focusedLocker = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable), typeof(ReadyToWork), typeof(Locker)));

    private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family f_closeLock = FamilyManager.getFamily(new AnyOfTags("LockIntroWheel", "LockR2Wheel", "ArrowUI", "HUD_TransparentOnMove"), new AllOfComponents(typeof(PointerOver)));

    private Family f_fences = FamilyManager.getFamily(new AnyOfTags("Fence"), new AllOfComponents(typeof(Animator)));
    private Family f_wallIntro = FamilyManager.getFamily(new AnyOfTags("WallIntro"), new AllOfComponents(typeof(Animator)));

    private Family f_iarBackground = FamilyManager.getFamily(new AnyOfTags("UIBackground"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_unlockedRoom = FamilyManager.getFamily(new AllOfComponents(typeof(UnlockedRoom)));

    private Family f_LockArrows = FamilyManager.getFamily(new AllOfComponents(typeof(AnimatedSprites), typeof(PointerOver)), new AnyOfTags("LockArrow"));

    //information for animations
    private Vector3 tmpTargetPosition;

    //locker
    private Locker selectedLocker = null;
    private GameObject selectedWheel;
    private bool lockRotationUp = false;
    private bool lockRotationDown = false;
    private Color lockWheelColor;
    private float wheelRotationCount = 0;
    private float wheelSpeedRotation = 200; // Default value

    private bool room1Unlocked = false;
    private bool room3Unlocked = false;
    private bool IARScreenRoom1Unlocked = false;
    private bool IARScreenRoom3Unlocked = false;

    private GameObject wallIntro;
    private GameObject fences;

    public static LockResolver instance;

    public LockResolver()
    {
        if (Application.isPlaying)
        {
            f_focusedLocker.addEntryCallback(onReadyToWorkOnLocker);

            wallIntro = f_wallIntro.First();
            fences = f_fences.First();
        }
        instance = this;
    }

    private void onReadyToWorkOnLocker(GameObject go)
    {
        selectedLocker = go.GetComponent<Locker>();
        // Enable UI arrows
        GameObjectManager.setGameObjectState(selectedLocker.LeftRightControl, true);
        GameObjectManager.setGameObjectState(selectedLocker.UpDownControl, true);
        // Change selected wheel color and move Up/Down UI over the selected wheel
        selectedWheel = selectedLocker.Wheel2;
        lockWheelColor = selectedWheel.GetComponent<Renderer>().material.color;
        selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
        selectedLocker.UpDownControl.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - selectedLocker.UpDownControl.transform.localPosition.x);
        // activate this system
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
        // Are we in front of the locker
        if (selectedLocker)
        {
            // "close" ui (give back control to the player) when clicking on nothing or Escape is pressed and IAR is closed
            if (((f_closeLock.Count == 0 && Input.GetButtonDown("Fire1")) || (Input.GetButtonDown("Cancel") && f_iarBackground.Count == 0)) && (!room1Unlocked || IARScreenRoom1Unlocked) && (!room3Unlocked || IARScreenRoom3Unlocked))
            {
                ExitLocker();
            }
            else
            {
                // avoid to rotate wheel during unlock animation
                if ((!room1Unlocked || IARScreenRoom1Unlocked) && (!room3Unlocked || IARScreenRoom3Unlocked))
                {
                    if (Input.GetButtonDown("Fire1"))
                    {
                        // Select the clicked wheel 
                        selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                        if (selectedLocker.Wheel1.GetComponent<PointerOver>())
                            selectedWheel = selectedLocker.Wheel1;
                        else if (selectedLocker.Wheel2.GetComponent<PointerOver>())
                            selectedWheel = selectedLocker.Wheel2;
                        else if (selectedLocker.Wheel3.GetComponent<PointerOver>())
                            selectedWheel = selectedLocker.Wheel3;
                        // Change selected wheel color and move Up/Down UI over the selected wheel
                        selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                        selectedLocker.UpDownControl.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - selectedLocker.UpDownControl.transform.localPosition.x);
                    }

                    // process hotkeys to move the wheels
                    if (Input.GetAxis("Vertical") > 0.2 && !lockRotationUp && !lockRotationDown)
                        moveWheelUp();
                    else if (Input.GetAxis("Vertical") < -0.2 && !lockRotationUp && !lockRotationDown)
                        moveWheelDown();
                    else if (Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") < 0)
                        SelectLeftWheel();
                    else if (Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") > 0)
                        SelectRightWheel();
                    //Process mouse arrow
                    if (Input.GetButton("Fire1") && f_LockArrows.Count != 0 && !lockRotationUp && !lockRotationDown)
                    {
                        if (f_LockArrows.First().name == "Down")
                            moveWheelDown();
                        else
                            moveWheelUp();
                    }
                }
            }
        }

        // Do we have to rotate wheel up or down?
        if (lockRotationUp || lockRotationDown)
        {
            if (lockRotationUp)
                selectedWheel.transform.Rotate(Time.deltaTime * wheelSpeedRotation, 0, 0);
            else
                selectedWheel.transform.Rotate(-Time.deltaTime * wheelSpeedRotation, 0, 0);
            wheelRotationCount += Time.deltaTime * wheelSpeedRotation;

            // is rotation finished ?
            if (wheelRotationCount > 36)
            {
                if (lockRotationUp)
                {
                    selectedWheel.transform.Rotate(36 - wheelRotationCount, 0, 0);
                    lockRotationUp = false;
                }
                else
                {
                    selectedWheel.transform.Rotate(-(36 - wheelRotationCount), 0, 0);
                    lockRotationDown = false;
                }
                wheelRotationCount = 0;
                // Check if the solution is found
                if (selectedLocker.Wheel1.GetComponent<WheelFrontFace>().faceNumber == selectedLocker.wheel1Solution && selectedLocker.Wheel2.GetComponent<WheelFrontFace>().faceNumber == selectedLocker.wheel2Solution && selectedLocker.Wheel3.GetComponent<WheelFrontFace>().faceNumber == selectedLocker.wheel3Solution)
                {
                    tmpTargetPosition = f_player.First().transform.position + Vector3.back * 3;
                    // depending of locker => unlock the right room
                    if (selectedLocker.tag == "LockIntro")
                    {
                        room1Unlocked = true;
                    }
                    else
                        room3Unlocked = true;
                }
            }
        }

        // Do we have to unlock first room and associated IAR screen
        if (room1Unlocked && !IARScreenRoom1Unlocked)
        {
            // straf right
            f_player.First().transform.position = Vector3.MoveTowards(f_player.First().transform.position, tmpTargetPosition, 4 * Time.deltaTime);
            // check if animation is over
            if (f_player.First().transform.position == tmpTargetPosition)
            {
                GameObjectManager.addComponent<PlaySound>(wallIntro, new { id = 9 }); // id refer to FPSController AudioBank
                wallIntro.GetComponent<Animator>().enabled = true; // enable animation

                // update IAR
                GameObjectManager.setGameObjectState(selectedLocker.IARScreenUnlock, true);// enable questions tab
                f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber = 1;
                // update flags
                IARScreenRoom1Unlocked = true;
                // disable UI items usable for this enigm
                if (selectedLocker.GetComponent<LinkedWith>())
                    GameObjectManager.setGameObjectState(selectedLocker.GetComponent<LinkedWith>().link, false);
                // Exit the locker
                ExitLocker();
            }
        }

        if (room3Unlocked && !IARScreenRoom3Unlocked)
        {
            f_player.First().transform.position = Vector3.MoveTowards(f_player.First().transform.position, tmpTargetPosition, 4 * Time.deltaTime);
            if (f_player.First().transform.position == tmpTargetPosition)
            {
                GameObjectManager.addComponent<PlaySound>(fences, new { id = 9 }); // id refer to FPSController AudioBank
                fences.GetComponent<Animator>().enabled = true; // enable animation
                GameObjectManager.setGameObjectState(selectedLocker.IARScreenUnlock, true); // enable questions tab
                f_unlockedRoom.First().GetComponent<UnlockedRoom>().roomNumber = 3;
                IARScreenRoom3Unlocked = true;
                ExitLocker();
            }
        }
    }

    private void ExitLocker()
    {
        // Hide UI
        GameObjectManager.setGameObjectState(selectedLocker.LeftRightControl, false);
        GameObjectManager.setGameObjectState(selectedLocker.UpDownControl, false);
        // Reset default wheel color
        selectedLocker.Wheel1.GetComponent<Renderer>().material.color = lockWheelColor;
        selectedLocker.Wheel2.GetComponent<Renderer>().material.color = lockWheelColor;
        selectedLocker.Wheel3.GetComponent<Renderer>().material.color = lockWheelColor;

        // remove ReadyToWork component to release selected GameObject
        GameObjectManager.removeComponent<ReadyToWork>(selectedLocker.gameObject);

        selectedLocker = null;

        // disable this system
        instance.Pause = true;
    }

    public void moveWheelUp()
    {
        if (!lockRotationUp && !lockRotationDown)
        {
            GameObjectManager.addComponent<PlaySound>(selectedWheel, new { id = 7 }); // id refer to FPSController AudioBank
            lockRotationUp = true;
            WheelFrontFace wff = selectedWheel.GetComponent<WheelFrontFace>();
            if(wff.faceNumber == 9)
                wff.faceNumber = 0;
            else
                wff.faceNumber++;
        }
    }

    public void moveWheelDown()
    {
        if (!lockRotationUp && !lockRotationDown)
        {
            GameObjectManager.addComponent<PlaySound>(selectedWheel, new { id = 7 }); // id refer to FPSController AudioBank
            lockRotationDown = true;
            WheelFrontFace wff = selectedWheel.GetComponent<WheelFrontFace>();
            if (wff.faceNumber == 0)
                wff.faceNumber = 9;
            else
                wff.faceNumber--;
        }
    }

    public void SelectLeftWheel()
    {
        if (!lockRotationUp && !lockRotationDown)
        {
            if (selectedWheel.GetInstanceID() == selectedLocker.Wheel2.GetInstanceID())
            {
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                selectedWheel = selectedLocker.Wheel1;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                selectedLocker.UpDownControl.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - selectedLocker.UpDownControl.transform.localPosition.x);
            }
            else if (selectedWheel.GetInstanceID() == selectedLocker.Wheel3.GetInstanceID())
            {
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                selectedWheel = selectedLocker.Wheel2;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                selectedLocker.UpDownControl.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - selectedLocker.UpDownControl.transform.localPosition.x);
            }
        }
    }

    public void SelectRightWheel()
    {
        if (!lockRotationUp && !lockRotationDown)
        {
            if (selectedWheel.GetInstanceID() == selectedLocker.Wheel1.GetInstanceID())
            {
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                selectedWheel = selectedLocker.Wheel2;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                selectedLocker.UpDownControl.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - selectedLocker.UpDownControl.transform.localPosition.x);
            }
            else if (selectedWheel.GetInstanceID() == selectedLocker.Wheel2.GetInstanceID())
            {
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color - Color.white * 0.2f;
                selectedWheel = selectedLocker.Wheel3;
                selectedWheel.GetComponent<Renderer>().material.color = selectedWheel.GetComponent<Renderer>().material.color + Color.white * 0.2f;
                selectedLocker.UpDownControl.transform.localPosition += Vector3.right * (selectedWheel.transform.localPosition.x - selectedLocker.UpDownControl.transform.localPosition.x);
            }
        }
    }

    public void SetWheelSpeed(float newValue)
    {
        wheelSpeedRotation = newValue;
    }
}