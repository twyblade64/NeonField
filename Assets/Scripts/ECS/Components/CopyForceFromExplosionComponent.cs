using Unity.Entities;

/// <summary>
/// Tag for the CopyForceFromExplosionSystem
/// </summary>
public struct CopyForceFromExplosion : IComponentData { }

/// <summary>
/// Wrapper for adding CopyForceFromExplosion component to GameObjects
/// </summary>
public class CopyForceFromExplosionComponent : ComponentDataWrapper<CopyForceFromExplosion> { } 
