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

[Serializable]
public struct GridRender : ISharedComponentData {
	public Material Material;
	public float Width;
	public int2 Size;
	public Mesh WorkMesh;
	public Vector3[] Vertices;
	public Vector3[] Normals;
}

[Serializable]
public struct LineRenderer : ISharedComponentData {
	public const int MaxVertices = 510000;
	public Mesh WorkMesh;
	public Material Material;
	public Vector3[] Vertices;
	public Vector3[] Normals;
	public NativeCounter Counter;
	public NativeCounter.Concurrent ConcurrentCounter;
}

[Serializable]
public struct Line : IComponentData {
	public float3 start;
	public float3 end;
	public float width;
}

[Serializable]
public struct EntityPair : IComponentData {
	public Entity E1;
	public Entity E2;
}