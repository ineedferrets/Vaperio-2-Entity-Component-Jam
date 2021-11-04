using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Movement : IComponentData
{
	public float speed;
	public float rotationSpeed;
	public float maxAngle;
	public float maxSpeed;
	public float acceleration;
}

