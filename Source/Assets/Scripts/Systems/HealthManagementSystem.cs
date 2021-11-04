using Unity.Entities;
using Unity.Collections;
using Unity.Physics.Stateful;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManagementSystem : SystemBase
{
	private List<StatefulTriggerEvent> HealthCollisions = new List<StatefulTriggerEvent>();

	private ComponentDataFromEntity<Health> HealthHolders;
	private ComponentDataFromEntity<Damage> DamageHolders;

	private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

	protected override void OnCreate()
	{
		base.OnCreate();
		m_EndSimulationEcbSystem = World
			.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override void OnStartRunning()
	{
		base.OnStartRunning();

		Entities.ForEach((ref Health health) =>
		{
			health.health = health.maxHealth;
		}).WithoutBurst().Run();
	}

	protected override void OnUpdate()
	{
		var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer();

		HealthHolders = GetComponentDataFromEntity<Health>(); 
		DamageHolders = GetComponentDataFromEntity<Damage>();

		Entities
			.WithAny<PlayerComponent, SwarmEnemy, Bullet>()
			.ForEach((Entity entity, ref Health health) =>
		{
			if (health.health <= 0.0f)
			{
				if (EntityManager.HasComponent<PlayerComponent>(entity))
				{
					// ecb.DestroyEntity(entity);
				}
				if (EntityManager.HasComponent<SwarmEnemy>(entity) && !health.hasDied)
				{
					var enemy = EntityManager.GetComponentData<SwarmEnemy>(entity);
					enemy.isChasing = false;
					ecb.SetComponent(entity, enemy);
					health.hasDied = true;
        }
			}
		}).WithoutBurst().Run();
	}
}
