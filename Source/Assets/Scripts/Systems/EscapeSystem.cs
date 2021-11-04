using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;

public class EscapeSystem : SystemBase
{
	protected override void OnUpdate()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
