using Unity.Entities;
using UnityEngine.UI;
using System;

public class LevelCounterSystem : SystemBase
{
  public ComponentDataFromEntity<HealthUIValue> HealthUIValueGroup;
  protected override void OnUpdate()
  {
    HealthUIValueGroup = GetComponentDataFromEntity<HealthUIValue>();
    Entities.ForEach((ref Health health, ref  HealthUI healthUI) =>
    {
      var healthUIValue = HealthUIValueGroup[healthUI.TextUI];
      healthUIValue.health = health.health;
      HealthUIValueGroup[healthUI.TextUI] = healthUIValue;
    }).WithoutBurst().Run();

    Entities.ForEach((Entity entity, HealthUIValue healthUIValue) =>
    {
      var uiText = EntityManager.GetComponentObject<Text>(entity);
      uiText.text = String.Format("Health: {0}", healthUIValue.health);
    }).WithoutBurst().Run();
  }
}

