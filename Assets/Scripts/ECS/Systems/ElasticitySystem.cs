using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

[UpdateBefore(typeof(ForceVelocitySystem))]
public class ElasticitySystem : JobComponentSystem {
	public struct Data {
								public readonly int Length;
								public ComponentDataArray<Physical> 	physical;
		[ReadOnly] 	public ComponentDataArray<Position> 	position;
		[ReadOnly] 	public ComponentDataArray<Anchor> 		anchor;
		[ReadOnly] 	public ComponentDataArray<Elasticity> 	elasticity;
	}
	
	[Inject] private Data m_Data;

	[BurstCompile]
	struct ApplyElasticityJob : IJobParallelFor {
								public ComponentDataArray<Physical> physical;
		[ReadOnly] 	public ComponentDataArray<Position> position;
		[ReadOnly] 	public ComponentDataArray<Anchor> anchor;
		[ReadOnly] 	public ComponentDataArray<Elasticity> elasticity;

		public void Execute(int i) {
			float3 f = physical[i].Force;
			f += elasticity[i].Value * (anchor[i].Value - position[i].Value);
			physical[i] = new Physical{Force = f, Mass = physical[i].Mass};
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps) {
		if (m_Data.Length == 0)
				return inputDeps;

		ApplyElasticityJob job = new ApplyElasticityJob {
			physical = m_Data.physical,
			position = m_Data.position,
			anchor = m_Data.anchor,
			elasticity = m_Data.elasticity
		};

		JobHandle jobHandle = job.Schedule(m_Data.Length, 64, inputDeps);
		return jobHandle;
	}
}
