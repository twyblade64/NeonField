using Unity.Entities;

/// Spy-Shifty solution for syncing systems with fixedUpdate. (https://forum.unity.com/threads/update-interval.523628/)
/// 

[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.FixedUpdate))]
[UpdateBefore(typeof(SyncToPhysicBarrier))]
[UpdateAfter(typeof(SyncFromPhysicBarrier))]
public class PhysicUpdate {}
 
[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.FixedUpdate))]
public class SyncToPhysicBarrier : BarrierSystem {}
 
[UpdateBefore(typeof(SyncToPhysicBarrier))]
[UpdateBefore(typeof(UnityEngine.Experimental.PlayerLoop.FixedUpdate))]
public class SyncFromPhysicBarrier : BarrierSystem {}
 