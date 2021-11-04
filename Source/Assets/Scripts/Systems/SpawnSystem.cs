using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;

public class SpawnSystem : SystemBase
{
	public NativeArray<Entity> spawnPoints;

	public int swarmSize = 200;

	protected override void OnStartRunning()
	{
		spawnPoints = GetEntityQuery(typeof(SpawnComponent)).ToEntityArray(Allocator.Persistent);
	}

	protected override void OnDestroy()
	{
		spawnPoints.Dispose();
		base.OnDestroy();
	}

	protected override void OnUpdate()
	{
		NativeArray<Entity> swarm = GetEntityQuery(typeof(SwarmEnemy)).ToEntityArray(Allocator.Temp);

		if (swarm.Length <= swarmSize)
		{
			for (int i = 0; i < swarmSize - swarm.Length; i++)
			{
				Entity spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
				Entity prefab = EntityManager.GetComponentData<SpawnComponent>(spawnPoint).Value;

				float3 translation = EntityManager.GetComponentData<Translation>(spawnPoint).Value;
				Spawn(prefab, translation);
			}
		}

		swarm.Dispose();
	}

	void Spawn(Entity prefab, float3 translation)
	{
		translation.x += UnityEngine.Random.Range(-2f, 2f);
		translation.y += UnityEngine.Random.Range(-2f, 2f);

		Entity newEnemy = EntityManager.Instantiate(prefab);
		EntityManager.SetComponentData<Translation>(newEnemy, new Translation { Value = translation });

		Movement oldMovement = EntityManager.GetComponentData<Movement>(newEnemy);
		Movement newMovement = new Movement
		{
			acceleration = oldMovement.acceleration + UnityEngine.Random.Range(-0.1f, 0.1f),
			maxSpeed = oldMovement.maxSpeed + UnityEngine.Random.Range(-0.02f, 0.02f)
		};

		EntityManager.SetComponentData<Movement>(newEnemy, newMovement);

		SwarmEnemy oldProperties = EntityManager.GetComponentData<SwarmEnemy>(newEnemy);
		SwarmEnemy newProperties = new SwarmEnemy
		{
			avoidanceDistance = oldProperties.avoidanceDistance + UnityEngine.Random.Range(-1.0f, 1.0f),
			neighbourDistance = oldProperties.neighbourDistance + UnityEngine.Random.Range(-1.0f, 1.0f)
		};
	}
}
