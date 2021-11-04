using Unity.Entities;
using Unity.Collections;
using Unity.Physics.Stateful;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTriggerSystem : SystemBase
{
  private List<StatefulTriggerEvent> HealthCollisions = new List<StatefulTriggerEvent>();

  private ComponentDataFromEntity<Health> HealthHolders;
  private ComponentDataFromEntity<Damage> DamageHolders;
  private ComponentDataFromEntity<SwarmEnemy> SwarmEnemies;


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
    HealthHolders = GetComponentDataFromEntity<Health>();
    DamageHolders = GetComponentDataFromEntity<Damage>();
    SwarmEnemies = GetComponentDataFromEntity<SwarmEnemy>();

    Entities
      .ForEach((ref Health health, in DynamicBuffer<StatefulTriggerEvent> triggerBuffer) =>
    {
      HandleCollisions(triggerBuffer, ecb);
    }).WithoutBurst().Run();
  }

  private void HandleCollisions(DynamicBuffer<StatefulTriggerEvent> triggerBuffer, EntityCommandBuffer ecb)
  {
    HealthCollisions = new List<StatefulTriggerEvent>();

    for (int i = 0; i < triggerBuffer.Length; i++)
    {
      var currentTrigger = triggerBuffer[i];
      if (!(SwarmEnemies.HasComponent(currentTrigger.EntityA) && SwarmEnemies.HasComponent(currentTrigger.EntityB)) && currentTrigger.State == EventOverlapState.Enter)
      {
        if (DamageHolders.HasComponent(currentTrigger.EntityA) && HealthHolders.HasComponent(currentTrigger.EntityB))
        {
          ApplyDamage(currentTrigger.EntityB, currentTrigger.EntityA, ecb);
          HealthCollisions.Add(currentTrigger);
        }
        if (DamageHolders.HasComponent(currentTrigger.EntityB) && HealthHolders.HasComponent(currentTrigger.EntityA))
        {
          ApplyDamage(currentTrigger.EntityA, currentTrigger.EntityB, ecb);
          HealthCollisions.Add(currentTrigger);
        }
      }
    }
  }

  private void ApplyDamage(Entity entityDamaged, in Entity entityDamaging, EntityCommandBuffer ecb)
  {
    var health = EntityManager.GetComponentData<Health>(entityDamaged);
    health.health -= EntityManager.GetComponentData<Damage>(entityDamaging).damage;
    ecb.SetComponent(entityDamaged, health);
  }
}
