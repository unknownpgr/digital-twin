using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class NodeFloodSensor : NodeManager
{
	protected override string prefabName { get => "Sensor"; }

	public override string DisplayName { get => "수재해센서:" + PhysicalID; }

	private bool isDisaster = false;
	private UnityEngine.AI.NavMeshObstacle navMeshObstacle;
	private Material cylinderMaterial;
	private Color color = new Color(0.1411765f, 0.5254902f, 1.0f, 1.0f);
    
	[JsonIgnore]
	public bool IsDisaster
	{
		get => isDisaster;
		set
		{
			isDisaster = value;
			navMeshObstacle.carving = value;
			cylinderMaterial.color = value ? color - new Color(0, 0, 0, 0.7f) : new Color(0, 0, 0, 0);
		}
	}

	[JsonIgnore]
	public float ValueFlood;

	protected override void Init()
	{
		navMeshObstacle = gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>();
		if (navMeshObstacle == null) throw new System.Exception("NavObstacle is null");

		cylinderMaterial = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
		if (cylinderMaterial == null) throw new System.Exception("Material is null");

		gameObject.GetComponent<MeshRenderer>().material.color = color;
	}
}
