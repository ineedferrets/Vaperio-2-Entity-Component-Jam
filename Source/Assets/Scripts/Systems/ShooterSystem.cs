using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

public class ShooterSystem : SystemBase
{

  private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
  private EntityCommandBuffer ECB;

  protected override void OnCreate()
  {
    base.OnCreate();
    // Find the ECB system once and store it for later usage
    m_EndSimulationEcbSystem = World
        .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
  }
  protected override void OnUpdate()
  {
    float deltaTime = Time.DeltaTime;
    bool shootPressed = Input.GetKey(KeyCode.F);
    ECB = m_EndSimulationEcbSystem.CreateCommandBuffer();
    Entities.ForEach((Entity shooterEntity, int entityInQueryIndex, ref Translation translation, ref Shooter shooter, in Rotation rotation) =>
    {
      if (shootPressed && shooter.timeSinceLastShot > shooter.reloadTime)
      {
        Spawn(entityInQueryIndex, shooter.Bullet, translation, rotation);
        shooter.timeSinceLastShot = 0f;
      }
      else
      {
        shooter.timeSinceLastShot += deltaTime;
      }
    }).WithoutBurst().Run();
  }

  private void Spawn(int index, Entity prefab, Translation translation, Rotation rotation)
  {
    Entity newBullet = ECB.Instantiate(prefab);
    ECB.SetComponent(newBullet, translation);
    ECB.SetComponent(newBullet, rotation);
  }
}
