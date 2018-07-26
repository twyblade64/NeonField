using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

public class MouseMovementSystem : ComponentSystem {
  public struct Data {
    public readonly int Length;
    public ComponentDataArray<Position> positions;
    [ReadOnly] public ComponentDataArray<MouseTracker> mouseTrackers;
  }

  [Inject] private Data m_Data;

  protected override void OnUpdate(){
    Ray mouseScreenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Plane plane = new Plane(Vector3.up, Vector3.zero);
		float rayPlaneInter;
		if (plane.Raycast(mouseScreenRay, out rayPlaneInter)) {
			Vector3 pos = mouseScreenRay.origin + mouseScreenRay.direction * rayPlaneInter;
      for (int i = 0; i < m_Data.Length; ++i) {
        m_Data.positions[i] = new Position{Value = (float3)pos};
      }
		}
  }
}