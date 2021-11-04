using Unity.Entities;

[GenerateAuthoringComponent]
public struct HealthUIValue : IComponentData
{
	public float health;
}
