using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class MainCameraSystem : SystemBase
{
  protected override void OnUpdate()
  {
    var translationGroup = GetComponentDataFromEntity<Translation>(true);

    Entities.ForEach((ref SteadyMovement steadyMovement, in Perspective perspective, in MainCamera mainCamera) =>
    {
      if (perspective.IsStarfoxStyle)
      {
        steadyMovement.destination = new float3(-40f, 1f, 4f);
        steadyMovement.rotationSpeed = 25;
        steadyMovement.rotationDestination = new float3(0, 90f, 0);
      }
      else
      {
        steadyMovement.destination = new float3(0f, 1f, -30f);
        steadyMovement.rotationSpeed = 150;
        steadyMovement.rotationDestination = new float3(0, 0, 0);
      }
    }).ScheduleParallel();
  }
}