using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MouseTracker : IComponentData {}
public class MouseTrackerComponent : ComponentDataWrapper<MouseTracker> {}	