using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class SatchelManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void UnlockSatchel()
	{
		MainLoop.callAppropriateSystemMethod ("SatchelManager", "UnlockSatchel", null);
	}

}
