using Unity.Entities;

[GenerateAuthoringComponent]
public struct Health : IComponentData
{
	public float maxHealth;
	public float health;
	public bool hasDied;
}
