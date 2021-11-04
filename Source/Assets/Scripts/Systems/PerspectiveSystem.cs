using Unity.Entities;
using UnityEngine;

public class PerspectiveSystem : SystemBase
{
  protected override void OnUpdate()
  {    
    bool changePressed = Input.GetKeyDown(KeyCode.Space);

    Entities.ForEach((ref Perspective perspective) =>
    {
        if(changePressed) perspective.IsStarfoxStyle = !perspective.IsStarfoxStyle;
    }).ScheduleParallel();
  }
}