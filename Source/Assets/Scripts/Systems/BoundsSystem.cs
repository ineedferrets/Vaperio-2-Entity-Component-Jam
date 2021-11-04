using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using System;

public class BoundsSystem : SystemBase
{
	private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
	protected override void OnCreate()
	{
		base.OnCreate();
		// Find the ECB system once and store it for later usage
		m_EndSimulationEcbSystem = World
			.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override void OnUpdate()
	{
		var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer();

		Entities
			.WithNone<PlayerComponent>()
			.WithAny<Bullet, SwarmEnemy>()
			.ForEach((Entity entity, ref Translation translation) =>
		{
			if (!IsInBounds(translation))
			{
				ecb.DestroyEntity(entity);
			}
		}).WithoutBurst().Run();
	}

	private static bool IsInBounds(Translation translation)
	{
		float x = translation.Value.x;
		float y = translation.Value.y;
		float z = translation.Value.z;
		return x < Globals.xMaxBoundBullet && x > Globals.xMinBoundBullet
			&& y < Globals.yMaxBoundBullet && y > Globals.yMinBoundBullet
			&& z < Globals.zMaxBoundBullet && z > Globals.zMinBoundBullet;
	}
}
