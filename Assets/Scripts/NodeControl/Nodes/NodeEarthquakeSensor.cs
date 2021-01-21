using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class NodeEarthquakeSensor : NodeManager
{
	protected override string prefabName { get => "Sensor"; }

	public override string DisplayName { get => "지진센서:" + PhysicalID; }

	private bool isDisaster = false;
	private UnityEngine.AI.NavMeshObstacle navMeshObstacle;
	private Material cylinderMaterial;
	private Color color = new Color(0.4235294f, 0.172549f, 0.172549f, 1.0f);

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
	public float ValueEarthquake;

	protected override void Init()
	{
		navMeshObstacle = gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>();
		if (navMeshObstacle == null) throw new System.Exception("NavObstacle is null");

		cylinderMaterial = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
		if (cylinderMaterial == null) throw new System.Exception("Material is null");

		gameObject.GetComponent<MeshRenderer>().material.color = color;
	}
}
