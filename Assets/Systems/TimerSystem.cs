﻿using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class TimerSystem : FSystem {

    private Family texts = FamilyManager.getFamily(new AllOfComponents(typeof(Text)));
    private Family timings = FamilyManager.getFamily(new AnyOfTags("Timings"));

    private Text timer; //text ui displaying the timer
    private float initialTime;  //time at the beginning of the game

    public TimerSystem()
    {
        initialTime = Time.time;
        timer = null;
        foreach(GameObject t in texts)
        {
            if(t.name == "CurrentTime")
            {
                timer = t.GetComponent<Text>();
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
	protected override void onProcess(int familiesUpdateCount) {
        float time = Time.time - initialTime;
        int hours = (int)time / 3600;
        int minutes = (int)(time % 3600)/60;
        int seconds = (int)(time % 3600) % 60;
        string t = string.Concat(hours.ToString("D2"),":",minutes.ToString("D2"),":",seconds.ToString("D2")); //time text in the format "00:00:00"
        timer.text = t;
        if (Timer.addTimer) //when true, the current time is saved and displayed on screen
        {
            foreach(GameObject timing in timings)
            {
                if (!timing.activeSelf) //find an unused timing ui text
                {
                    timing.SetActive(true);
                    timing.GetComponent<Text>().text = t;
                    break;
                }
            }
            Timer.addTimer = false;
        }
    }
}