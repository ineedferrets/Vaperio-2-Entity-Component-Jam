using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using System;

struct SwarmEnemyData
{
	public float3 translation;
	public float3 velocity;
	public float avoidanceDistance;
	public float neighbourDistance;
	public float acceleration;
	public float maxSpeed;
	public bool isChasing;
}

public class SwarmAISystem : SystemBase
{
	private Entity player;

	protected override void OnStartRunning()
	{
		player = GetEntityQuery(typeof(PlayerComponent)).GetSingletonEntity();
	}
	protected override void OnUpdate()
	{
		float deltaTime = Time.DeltaTime;
		float3 playerPosition = EntityManager.GetComponentData<Translation>(player).Value;
		NativeArray<Entity> swarm = GetEntityQuery(typeof(SwarmEnemy), typeof(Translation), typeof(PhysicsVelocity), typeof(Movement))
			.ToEntityArray(Allocator.TempJob);

		NativeArray<SwarmEnemyData> swarmEnemyData = new NativeArray<SwarmEnemyData>(swarm.Length, Allocator.TempJob);
		NativeArray<bool> isToBeDeleted = new NativeArray<bool>(swarm.Length, Allocator.TempJob);

		for (int i = 0; i < swarm.Length; i++)
		{
			swarmEnemyData[i] = new SwarmEnemyData
			{
				translation = EntityManager.GetComponentData<Translation>(swarm[i]).Value,
				velocity = EntityManager.GetComponentData<PhysicsVelocity>(swarm[i]).Linear,
				avoidanceDistance = EntityManager.GetComponentData<SwarmEnemy>(swarm[i]).avoidanceDistance,
				neighbourDistance = EntityManager.GetComponentData<SwarmEnemy>(swarm[i]).neighbourDistance,
				acceleration = EntityManager.GetComponentData<Movement>(swarm[i]).acceleration,
				maxSpeed = EntityManager.GetComponentData<Movement>(swarm[i]).maxSpeed,
				isChasing = EntityManager.GetComponentData<SwarmEnemy>(swarm[i]).isChasing
			};

			isToBeDeleted[i] = false;
		}

		NativeArray<SwarmEnemyData> copySwarmEnemyData = new NativeArray<SwarmEnemyData>(swarm.Length, Allocator.TempJob);
		copySwarmEnemyData.CopyFrom(swarmEnemyData);

		SwarmMoveJob job = new SwarmMoveJob
		{
			roSwarmEnemyData = copySwarmEnemyData,
			swarmEnemyData = swarmEnemyData,
			deleteEnemy = isToBeDeleted,
			playerPosition = playerPosition,
			deltaTime = deltaTime
		};

		JobHandle jobHandle = job.Schedule(swarm.Length, 2);

		jobHandle.Complete();

		for (int i = 0; i < swarm.Length; i++)
		{
			if (isToBeDeleted[i])
			{
				EntityManager.DestroyEntity(swarm[i]);
				continue;
			}
		
			PhysicsVelocity oldVelocity = EntityManager.GetComponentData<PhysicsVelocity>(swarm[i]);
			PhysicsVelocity newVelocity = new PhysicsVelocity { Angular = oldVelocity.Angular, Linear = job.swarmEnemyData[i].velocity };
			EntityManager.SetComponentData<PhysicsVelocity>(swarm[i], newVelocity);
		}

		isToBeDeleted.Dispose();
		copySwarmEnemyData.Dispose();
		swarmEnemyData.Dispose();
		swarm.Dispose();
	}


}

struct SwarmMoveJob : IJobParallelFor
{
	[ReadOnly]
	public NativeArray<SwarmEnemyData> roSwarmEnemyData;

	public NativeArray<SwarmEnemyData> swarmEnemyData;
	
	public NativeArray<bool> deleteEnemy;

	[ReadOnly]
	public float3 playerPosition;

	[ReadOnly]
	public float deltaTime;

	private static float3 CalculateDirection(float3 currentPosition, float3 playerPosition, float avoidanceDistance, float neighbourDistance,
	in NativeArray<SwarmEnemyData> swarmEnemyData)
	{
		NativeList<float3> neighbourVelocities = new NativeList<float3>(Allocator.Temp);
		NativeList<float3> neighbourTranslations = new NativeList<float3>(Allocator.Temp);
		float3 direction;

		if (currentPosition.x < playerPosition.x)
			direction = AdjustDirectionForCentre(currentPosition, new float3(-40.0f, 0.0f, 0.0f));
		else
			direction = AdjustDirectionForCentre(currentPosition, playerPosition);

		IterateThroughSwarm(currentPosition, avoidanceDistance, ref neighbourTranslations, neighbourDistance, ref neighbourVelocities,
			in swarmEnemyData);

		int divisor = 1;

		if (!neighbourTranslations.IsEmpty)
		{
			direction += AdjustDirectionForAvoidance(currentPosition, ref neighbourTranslations);
			divisor++;
		}
		if (!neighbourVelocities.IsEmpty)
		{
			direction += AdjustDirectionForAlignment(ref neighbourVelocities);
			divisor++;
		}

		direction /= divisor;

		neighbourTranslations.Dispose();
		neighbourVelocities.Dispose();

		return direction;
	}

	private static void IterateThroughSwarm(float3 currentEnemyTranslation, float avoidanceDistance,
		ref NativeList<float3> neighbourTranslations, float neighbourDistance, ref NativeList<float3> neighbourVelocities,
		in NativeArray<SwarmEnemyData> swarmEnemyData)
	{
		for (int i = 0; i < swarmEnemyData.Length; i++)
		{
			if (currentEnemyTranslation.Equals(swarmEnemyData[i].translation))
				continue;

			float distance = math.abs(math.length(currentEnemyTranslation - swarmEnemyData[i].translation));
			if (distance < avoidanceDistance)
			{
				neighbourTranslations.Add(swarmEnemyData[i].translation);
			}

			if (distance < neighbourDistance)
			{
				neighbourVelocities.Add(swarmEnemyData[i].velocity);
			}
		}
	}

	private static float3 AdjustDirectionForAvoidance(float3 currentTranslation, ref NativeList<float3> neighbourTranslations)
	{
		float3 newDirection = float3.zero;

		for (int i = 0; i < neighbourTranslations.Length; i++)
		{
			newDirection += currentTranslation - neighbourTranslations[i];
		}

		newDirection = newDirection / neighbourTranslations.Length;

		return math.normalize(newDirection);
	}

	private static float3 AdjustDirectionForAlignment(ref NativeList<float3> neighbourVelocities)
	{
		float3 meanVelocity = neighbourVelocities[0];
		for (int j = 0; j < neighbourVelocities.Length; j++)
		{
			meanVelocity = (meanVelocity + neighbourVelocities[j]) * 0.5f;
		}
		return math.normalizesafe(meanVelocity);
	}

	private static float3 AdjustDirectionForCentre(float3 currentTranslation, float3 aimTranslation)
	{
		return math.normalize(aimTranslation - currentTranslation);
	}

	public void Execute(int i)
	{
		if (swarmEnemyData[i].translation.x < -40.0f)
		{
			deleteEnemy[i] = true;
			return;
		}

		if (!swarmEnemyData[i].isChasing)
			return;

		float3 previousVelocity = swarmEnemyData[i].velocity;

		float3 direction = CalculateDirection(swarmEnemyData[i].translation, playerPosition, swarmEnemyData[i].avoidanceDistance,
			swarmEnemyData[i].neighbourDistance, in roSwarmEnemyData);
	

		float3 newVelocity = previousVelocity + (deltaTime * direction * swarmEnemyData[i].acceleration);

		if (math.length(newVelocity) > swarmEnemyData[i].maxSpeed)
		{
			newVelocity = math.normalize(newVelocity) * swarmEnemyData[i].maxSpeed;
		}

		swarmEnemyData[i] = new SwarmEnemyData
		{
				translation = swarmEnemyData[i].translation,
				velocity = newVelocity,
				avoidanceDistance = swarmEnemyData[i].avoidanceDistance,
				neighbourDistance = swarmEnemyData[i].neighbourDistance,
				acceleration = swarmEnemyData[i].acceleration,
				maxSpeed = swarmEnemyData[i].maxSpeed
		};
	}
}
