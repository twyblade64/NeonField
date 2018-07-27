using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct Anchor : IComponentData {
	public float3 Value;
}

[Serializable]
public struct Elasticity : IComponentData {
	public float Value;
}

[Serializable]
public struct Physical : IComponentData {
	public float3 Force;
	public float InverseMass;
}

[Serializable]
public struct Velocity : IComponentData {
	public float3 Value;
}

[Serializable]
public struct Damper : IComponentData {
	public float Value;
}

[Serializable]
public struct GridPosition : IComponentData {
	public int2 Value;
}

/*[Serializable]
public struct GridIdentifier : ISharedComponentData {
	public int id;
	public int2 size;
}*/

[Serializable]
public struct GridRender : ISharedComponentData {
	public Material Material;
	public float Width;
	public int2 Size;
	public Mesh WorkMesh;
	public Vector3[] Vertices;
	public Vector3[] Normals;
	//public NativeCounter counter;
	//public NativeCounter.Concurrent concurrentCounter;
}