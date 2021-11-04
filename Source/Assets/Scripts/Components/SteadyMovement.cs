using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SteadyMovement : IComponentData
{
  public float speed;
	public float3 destination;
  public float rotationSpeed;
	public float3 rotationDestination;
}

