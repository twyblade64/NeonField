using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Component representing the elasticity of an entity.
/// </summary>
[Serializable]
public struct Elasticity : IComponentData {
	/// How elastic it is.
	public float YoungModulus;
	/// Original length
	public float ReferenceLength;
}

/// <summary>
/// Component representing a physical entity.
/// </summary>
[Serializable]
public struct Physical : IComponentData {
	/// The current sum of the forces affecting the entity
	public float3 Force;
	/// The inverse mass of the objet
	public float InverseMass;
}

/// <summary>
/// Component representing the velocity of an entity.
/// </summary>
[Serializable]
public struct Velocity : IComponentData {
	/// The current velocity of the entity.
	public float3 Value;
}

/// <summary>
/// Component representing an entity with an speed limit.
/// </summary>
[Serializable]
public struct MaxSpeed : IComponentData {
	/// Maximum speed
	public float Value;
}

/// <summary>
/// Component representing an entity which energy decreases.
/// </summary>
[Serializable]
public struct Damper : IComponentData {
	/// The decrease rate of the entity's energy.
	public float Value;
}

/// <summary>
/// Component representing an entity that is rendered as a line.
/// </summary>
[Serializable]
public struct LineRenderer : ISharedComponentData {
	/// The mesh used to write the vertices and normals of the line
	public Mesh WorkMesh;
	/// The material used to render the line
	public Material Material;
	/// An array containing the information of the line vertices.
	public Vector3[] Vertices;
	/// An array containing the information of the line vertex normals.
	public Vector3[] Normals;
}

/// <summary>
/// Component representing a line
/// </summary>
[Serializable]
public struct Line : IComponentData {
	public float3 P1;
	public float3 P2;
}

/// <summary>
/// Component representing an entity that has thickness.
/// </summary>
[Serializable]
public struct Thickness : IComponentData {
	public float Value;
}

/// <summary>
/// Component representing an entity that references two other entities.
/// </summary>
[Serializable]
public struct EntityPair : IComponentData {
	public Entity E1;
	public Entity E2;
}

/// <summary>
/// Component representing an entity that has some axis frozen.
/// </summary>
[Serializable]
public struct FreezeAxis : IComponentData {
	/// Bitmask to define wich axis to frose.
	public enum AxisMask  {
		NONE 	= 0,
		X			= 1,
		Y			= 2,
		Z			= 4,
		ALL = 7
	}

	/// The mask used to define which axis are frozen.
	public AxisMask FreezeMask;
	/// The position at which froze each axis.
	public float3 FreezePos;
}