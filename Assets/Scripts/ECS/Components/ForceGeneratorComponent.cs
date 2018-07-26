using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ForceGenerator : IComponentData {
	public float force;
	public float distance;
}

public class ForceGeneratorComponent : ComponentDataWrapper<ForceGenerator> {}	