using Unity.Entities;

/// <summary>
/// Tag for the CopyForceFromExplosionSystem
/// </summary>
[System.Serializable]
public struct CopyForceFromExplosion : IComponentData { }

/// <summary>
/// Wrapper for adding CopyForceFromExplosion component to GameObjects
/// </summary>
[UnityEngine.DisallowMultipleComponent]
public class CopyForceFromExplosionProxy : ComponentDataProxy<CopyForceFromExplosion> { } 
