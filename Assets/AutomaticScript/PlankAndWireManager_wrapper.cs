using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class PlankAndWireManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void DisplayWireOnSolution()
	{
		MainLoop.callAppropriateSystemMethod ("PlankAndWireManager", "DisplayWireOnSolution", null);
	}

}
