﻿using UnityEngine;
using FYFY;

public class TakeObject : FSystem {
    // Both of the Vive Controllers
    private Family controllers = FamilyManager.getFamily(new AllOfComponents(typeof(Grabber)));
    //all takable objects
    private Family tObjects = FamilyManager.getFamily(new AllOfComponents(typeof(Selectable), typeof(Takable)));
    //enigma03's balls
    private Family balls = FamilyManager.getFamily(new AnyOfTags("Ball"));
    private Family player = FamilyManager.getFamily(new AnyOfTags("Player"));

    private float onTableHeight;

    public TakeObject()
    {
        // For each controller
        foreach (GameObject c in controllers)
        {
            Grabber g = c.GetComponent<Grabber>();
            // Get the tracked object (device)
            g.trackedObj = g.GetComponent<SteamVR_TrackedObject>();
        }

        //at the beginning of the game, all taken object are not kinematic
        foreach (GameObject go in tObjects)
        {
            go.GetComponent<Rigidbody>().isKinematic = false;
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
        //respawn objects that fall under the room
        foreach(GameObject go in tObjects)
        {
            if(go.transform.position.y < go.transform.parent.transform.position.y-1)
            {
                go.transform.position = go.transform.parent.transform.position + Vector3.up*3;
            }
        }

        /*if (!Selectable.selected && !CollectableGO.onInventory)   //if there is not selected object and inventory isn't opened
        {
            if (Takable.objectTaken)    //if an object is taken
            {
                foreach (GameObject go in tObjects)
                {
                    if (go.GetComponent<Takable>().taken)   //find the taken object
                    {
                        if(go.tag == "TableE05")
                        {
                            Camera.main.transform.localRotation = Quaternion.Euler(90,0,0);
                            player.First().transform.position += Vector3.up * (onTableHeight - player.First().transform.position.y);
                            go.transform.position = player.First().transform.position + Vector3.down*2;    //move the object under the player
                            go.transform.rotation = Quaternion.Euler(0, player.First().transform.rotation.eulerAngles.y, 0);      //rotate the object to the camera
                        }
                        else
                        {
                            Vector3 v = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                            v.Normalize();
                            go.transform.position = Camera.main.transform.position + v * (go.transform.localScale.y + 1.5f);    //move the object in front of the camera
                            if (go.GetComponent<MirrorScript>())
                            {
                                go.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);      //rotate the object to the camera
                            }
                            else
                            {
                                go.transform.rotation = Quaternion.Euler(10, Camera.main.transform.rotation.eulerAngles.y, 0);      //rotate the object to the camera
                            }
                        }
                        if (Input.GetMouseButtonDown(1)) //if right click, release the object
                        {
                            go.GetComponent<Takable>().taken = false;
                            go.GetComponent<Rigidbody>().isKinematic = false;
                            Takable.objectTaken = false;
                            if (go.tag == "Box")    //when box is released, balls are no more kinematic 
                            {
                                foreach (GameObject ball in balls)
                                {
                                    ball.GetComponent<Rigidbody>().isKinematic = false;
                                }
                            }
                            else if(go.tag == "TableE05")
                            {
                                Camera.main.transform.localRotation = Quaternion.Euler(Vector3.zero);
                                player.First().transform.position = go.transform.position - go.transform.forward * 1.5f;
                            }
                        }
                        break;
                    }
                }
            }
            else    //is there is not taken object
            {
                foreach (GameObject go in tObjects)
                {
                    //if right click on a focused (but not selected) object, take it
                    if (go.GetComponent<Selectable>().focused && Input.GetMouseButtonDown(1))
                    {
                        go.GetComponent<Takable>().taken = true;
                        go.GetComponent<Rigidbody>().isKinematic = true;
                        Takable.objectTaken = true;
                        if (go.tag == "Box")
                        {
                            foreach (GameObject ball in balls)
                            {
                                ball.GetComponent<Rigidbody>().isKinematic = true;
                            }
                        }
                        else if(go.tag == "TableE05")
                        {
                            player.First().transform.forward = go.transform.forward;
                            player.First().transform.position = go.transform.position + Vector3.up * 2;
                            onTableHeight = player.First().transform.position.y;
                        }
                        break;
                    }
                }
            }
        }*/
        foreach (GameObject c in controllers)
        {
            Grabber g = c.GetComponent<Grabber>();
            SteamVR_Controller.Device controller = SteamVR_Controller.Input((int)g.trackedObj.index);

            // If the user is pressing the trigger
            if (controller.GetHairTriggerDown())
            {
                if(g.collidingObject)
                {
                    GrabObject(g);
                }
            }

            // If the user has released the trigger
            if (controller.GetHairTriggerUp())
            {
                ReleaseObject(g);
            }
        }
	}

    private void GrabObject(Grabber g)
    {
        // Move the collidingObject inside the hand
        g.objectInHand = g.collidingObject;
        g.collidingObject = null;
        // Joint
        var joint = AddFixedJoint(g);
        joint.connectedBody = g.objectInHand.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint(Grabber g)
    {
        FixedJoint fx = g.gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    private void ReleaseObject(Grabber g)
    {
        // Check the joint
        if(g.GetComponent<FixedJoint>())
        {
            // Remove the connected body
            g.GetComponent<FixedJoint>().connectedBody = null;
            GameObject.Destroy(g.GetComponent<FixedJoint>());
            // Throw !
            SteamVR_Controller.Device controller = SteamVR_Controller.Input((int)g.trackedObj.index);
            g.objectInHand.GetComponent<Rigidbody>().velocity = controller.velocity;
            g.objectInHand.GetComponent<Rigidbody>().angularVelocity = controller.angularVelocity;
        }
        g.objectInHand = null;
    }
}