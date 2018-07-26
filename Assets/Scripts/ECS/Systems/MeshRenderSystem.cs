using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Rendering;
using UnityEngine.Experimental.PlayerLoop;

[UpdateAfter(typeof(PreLateUpdate))]
public class MeshRenderSystem : ComponentSystem {
	public struct Data {
									public readonly int Length;
			[ReadOnly]	public SharedComponentDataArray<MeshInstanceRenderer> meshRenderer;
			[ReadOnly]	public ComponentDataArray<TransformMatrix> transformMatrix;
	}

	[Inject] private Data m_Data;

  protected override void OnUpdate() {
		Matrix4x4[] matrixList = new Matrix4x4[1023];
    for (int i = 0; i < m_Data.Length; i+=1023) {
				int lim = Mathf.Min(1023, m_Data.Length - i);
				for (int j = 0; j < lim; ++j) {
					float4x4 f4x4 = m_Data.transformMatrix[i+j].Value;
					matrixList[j] = new Matrix4x4(f4x4.c0, f4x4.c1, f4x4.c2, f4x4.c3);
				}
				Graphics.DrawMeshInstanced(m_Data.meshRenderer[i].mesh, m_Data.meshRenderer[i].subMesh, m_Data.meshRenderer[i].material, matrixList, lim);
			}
  }
}
