using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using System;

public class BulletSystem : SystemBase
{
  protected override void OnUpdate()
  {
    var deltaTime = Time.DeltaTime;

    Entities.ForEach((Entity entity, ref Translation translation, in Rotation rotation, in Bullet bullet) =>
    {
      Quaternion rotationQuaternion = new Quaternion(rotation.Value.value.x, rotation.Value.value.y, rotation.Value.value.z, rotation.Value.value.w);
      Vector3 movement = (rotationQuaternion * Vector3.forward) * bullet.speed * deltaTime;
      translation.Value = translation.Value + new float3(movement.x, movement.y, movement.z);
    }).WithoutBurst().Run();
  }
}