using Unity.Entities;

[GenerateAuthoringComponent]
public struct MainCamera : IComponentData
{
  public Entity EntityToTrack;
}
