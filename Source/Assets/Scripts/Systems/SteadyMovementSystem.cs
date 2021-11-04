using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using System;

public class SteadyMovementSystem : SystemBase
{

  protected override void OnUpdate()
  {
    float deltaTime = Time.DeltaTime;
    Entities.ForEach((ref Translation translation, ref Rotation rotation, in SteadyMovement steadyMovement) =>
    {
      if (!steadyMovement.destination.Equals(translation.Value))
      {
        translation.Value = MovementUtils.GetNextPosition(translation.Value, steadyMovement.destination, steadyMovement.speed, deltaTime);
      }

      if (!steadyMovement.rotationDestination.Equals(RotationUtils.ToEulerAngles(rotation.Value.value)))
      {
        quaternion targetRotation = RotationUtils.GetQuaternionFromEulerAngles(steadyMovement.rotationDestination);
        rotation.Value = RotationUtils.GetNextRotationTowardsTarget(steadyMovement.rotationSpeed, rotation.Value, targetRotation, deltaTime);
      }
    }).ScheduleParallel();

  }
}