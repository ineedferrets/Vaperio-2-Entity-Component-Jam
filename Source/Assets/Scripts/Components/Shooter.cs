using Unity.Entities;

[GenerateAuthoringComponent]
public struct Shooter : IComponentData
{
  public Entity Bullet;
  public float reloadTime;
  public float timeSinceLastShot;
}

