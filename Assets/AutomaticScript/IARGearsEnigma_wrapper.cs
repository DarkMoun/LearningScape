using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class IARGearsEnigma_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SolveGearsEnigma(System.Boolean loginSelectable)
	{
		MainLoop.callAppropriateSystemMethod ("IARGearsEnigma", "SolveGearsEnigma", loginSelectable);
	}

}
