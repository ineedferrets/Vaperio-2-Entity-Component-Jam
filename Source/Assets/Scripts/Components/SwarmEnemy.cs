using Unity.Entities;

[GenerateAuthoringComponent]
public struct SwarmEnemy : IComponentData
{
	public float avoidanceDistance;
	public float neighbourDistance;
	public bool isChasing;
}
