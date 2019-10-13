using Unity.Entities;

/// <summary>
/// Component representing the information of a radial force.
/// </summary>
[System.Serializable]
public struct ForceGenerator : IComponentData {
	/// The magnitude of the force
	public float force;
	/// The maximum distance the force can travel
	public float distance;
}

/// <summary>
/// Wrapper for adding ForceGenerator component to GameObjects
/// </summary>
[UnityEngine.DisallowMultipleComponent]
public class ForceGeneratorProxy : ComponentDataProxy<ForceGenerator> {}	