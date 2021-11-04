using Unity.Mathematics;
using UnityEngine;

public class MovementUtils
{

  public static float3 GetNextPosition(float3 currentPosition, float3 destination, float speed, float deltaTime)
  {
    if (!currentPosition.Equals(destination))
    {
      var vectorToMoveAlong = new Vector3(destination.x - currentPosition.x, destination.y - currentPosition.y, destination.z - currentPosition.z);
      var vectorToMove = (speed / vectorToMoveAlong.magnitude) * deltaTime * vectorToMoveAlong;
      if (vectorToMove.magnitude < vectorToMoveAlong.magnitude)
      {
        return new float3(currentPosition.x + vectorToMove.x, currentPosition.y + vectorToMove.y, currentPosition.z + vectorToMove.z);
      }
      else
      {
        return destination;
      }
    }
    return currentPosition;
  }

}