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
public struct GridRenderer : ISharedComponentData {
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
}

[Serializable]
public struct Spring : IComponentData {
	public float3 p1;
	public float3 p2;
	public float length;
	public float width;
}

[Serializable]
public struct EntityPair : IComponentData {
	public Entity E1;
	public Entity E2;
}

[Serializable]
public struct FreezeAxis : IComponentData {
	public enum AxisMask  {
		NONE 	= 0,
		X			= 1,
		Y			= 2,
		Z			= 4
	}

	public AxisMask FreezeMask;
	public float3 FreezePos;
}