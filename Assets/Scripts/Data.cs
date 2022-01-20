using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "data.asset", menuName = "Custom Assets/Project Data")]
public class Data : ScriptableSingleton<Data>
{
	public float AnimStepStatus = 1f;
}
