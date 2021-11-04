using Unity.Mathematics;
using UnityEngine;

public class RotationUtils
{
  public static float3 ToEulerAngles(quaternion q)
  {
    float3 angles;

    // roll (x-axis rotation)
    double sinr_cosp = 2 * (q.value.w * q.value.x + q.value.y * q.value.z);
    double cosr_cosp = 1 - 2 * (q.value.x * q.value.x + q.value.y * q.value.y);
    angles.x = (float)math.atan2(sinr_cosp, cosr_cosp);

    // pitch (y-axis rotation)
    double sinp = 2 * (q.value.w * q.value.y - q.value.z * q.value.x);
    if (math.abs(sinp) >= 1)
      angles.y = (float)CopySign(math.PI / 2, sinp); // use 90 degrees if out of range
    else
      angles.y = (float)math.asin(sinp);

    // yaw (z-axis rotation)
    double siny_cosp = 2 * (q.value.w * q.value.z + q.value.x * q.value.y);
    double cosy_cosp = 1 - 2 * (q.value.y * q.value.y + q.value.z * q.value.z);
    angles.z = (float)math.atan2(siny_cosp, cosy_cosp);

    return RadiansToDegrees(angles);
  }

  private static double CopySign(double a, double b)
  {
    return math.abs(a) * math.sign(b);
  }

  public static quaternion GetQuaternionFromEulerAngles(float3 eulerAngles)
  {
    Quaternion quaternionToSet = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
    return new quaternion(quaternionToSet.x, quaternionToSet.y, quaternionToSet.z, quaternionToSet.w);
  }

  public static float3 DegreesToRadians(float3 eulerAnglesInDegrees)
  {
    return eulerAnglesInDegrees * (math.PI / 180);
  }
  public static float3 RadiansToDegrees(float3 eulerAnglesInRadians)
  {
    return eulerAnglesInRadians * (180 / math.PI);
  }

  public static quaternion GetDotsQuaternion(Quaternion quaternionObject)
  {
    return new quaternion(quaternionObject.x, quaternionObject.y, quaternionObject.z, quaternionObject.w);
  }

  public static quaternion GetNonDotsQuaternion(quaternion dotsQuaternion)
  {
    return new quaternion(dotsQuaternion.value.x, dotsQuaternion.value.y, dotsQuaternion.value.z, dotsQuaternion.value.w);
  }

  public static quaternion GetNonDotsQuaternion(float4 dotsQuaternion)
  {
    return new quaternion(dotsQuaternion.x, dotsQuaternion.y, dotsQuaternion.z, dotsQuaternion.w);
  }

 
  public static quaternion GetNextRotationTowardsTarget(float rotationSpeed, quaternion rotation, quaternion targetRotation, float deltaTime)
  {
    Quaternion nextRotation = Quaternion.RotateTowards(RotationUtils.GetNonDotsQuaternion(rotation), RotationUtils.GetNonDotsQuaternion(targetRotation), rotationSpeed * deltaTime);
    return new quaternion(nextRotation.x, nextRotation.y, nextRotation.z, nextRotation.w);
  }
}