using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using System;

public class MovementSystem : SystemBase
{

  protected override void OnUpdate()
  {
    float deltaTime = Time.DeltaTime;
    float verticalInput = Input.GetAxis("Vertical");
    float horizontalInput = Input.GetAxis("Horizontal");
    float ForcedMovementSpeed = 25f;
    float ClassicStyleZPosition = 4f;
    float StarfoxStyleXPosition = -30f;
    Entities.ForEach((ref Translation translation, ref Rotation rotation, in Movement movement, in Perspective perspective) =>
    {
      if (!perspective.IsStarfoxStyle)
      {
        float2 displacement = GetMovement(movement.speed, horizontalInput, verticalInput);
        translation.Value.x = translation.Value.x + displacement.x * deltaTime;
        translation.Value.y = translation.Value.y + displacement.y * deltaTime;

        float forcedMovement = ForcedMovementSpeed * deltaTime;
        translation.Value.z = HandleForcedMovement(forcedMovement, ClassicStyleZPosition, translation.Value.z);
      }
      else
      {
        if (!(verticalInput == 0 && horizontalInput == 0))
        {
          quaternion targetRotation = GetTargetRotation(verticalInput, horizontalInput, movement);
          rotation.Value = RotationUtils.GetNextRotationTowardsTarget(movement.rotationSpeed, rotation.Value, targetRotation, deltaTime);

          float2 displacement = GetMovement(movement.speed, horizontalInput, verticalInput);
          translation.Value.z = translation.Value.z - displacement.x * deltaTime;
          translation.Value.y = translation.Value.y - displacement.y * deltaTime;
        }

        float forcedMovement = ForcedMovementSpeed * deltaTime;
        translation.Value.x = HandleForcedMovement(forcedMovement, StarfoxStyleXPosition, translation.Value.x);
      }

      if (!perspective.IsStarfoxStyle || (verticalInput == 0 && horizontalInput == 0))
      {
        quaternion targetRotation = RotationUtils.GetQuaternionFromEulerAngles(new float3(0, 90, 0));
        rotation.Value = RotationUtils.GetNextRotationTowardsTarget(movement.rotationSpeed, rotation.Value, targetRotation, deltaTime);
      }

      translation.Value = ConstrainPosition(translation.Value, perspective.IsStarfoxStyle);
    }).ScheduleParallel();
  }

  private static float2 GetMovement(float movementSpeed, float horizontalInput, float verticalInput)
  {
    float xMovement = horizontalInput * movementSpeed;
    float yMovement = verticalInput * movementSpeed;
    float totalMovementDistance = Mathf.Sqrt(xMovement * xMovement + yMovement * yMovement);
    float multiplier = totalMovementDistance > movementSpeed ? movementSpeed / totalMovementDistance : 1;
    return new float2(xMovement * multiplier, yMovement * multiplier);
  }

  private static quaternion GetTargetRotation(float verticalInput, float horizontalInput, Movement movement)
  {
    return RotationUtils.GetQuaternionFromEulerAngles(new float3(verticalInput * movement.maxAngle, 90f + horizontalInput * movement.maxAngle, 0f));
  }

  private static float HandleForcedMovement(float forcedMovement, float target, float current)
  {
    if (Mathf.Abs(target - current) < forcedMovement)
    {
      return target;
    }
    else
    {
      return target > current ? current + forcedMovement : current - forcedMovement;
    }
  }

  private static float3 ConstrainPosition(float3 position, bool isStarfoxStyle)
  {
    float x = ConstrainSingleDimension(position.x, Globals.xMaxBoundPlayer, Globals.xMinBoundPlayer);
    float y = isStarfoxStyle ?
            ConstrainSingleDimension(position.y, Globals.yMaxBoundStarfoxPlayer, Globals.yMinBoundStarfoxPlayer)
            : ConstrainSingleDimension(position.y, Globals.yMaxBoundClassicPlayer, Globals.yMinBoundClassicPlayer);
    float z = ConstrainSingleDimension(position.z, Globals.zMaxBoundPlayer, Globals.zMinBoundPlayer);
    return new float3(x, y, z);
  }

  private static float ConstrainSingleDimension(float current, float max, float min)
  {
    float result = current > min ? current : min;
    return result < max ? result : max;
  }
}