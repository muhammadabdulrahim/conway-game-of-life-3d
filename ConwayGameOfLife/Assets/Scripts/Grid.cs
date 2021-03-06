﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
	//	Definition for the maximum number of neighbors possible
	public const uint MAX_NEIGHBORS = 26;

	[Header("Rendered object prefab")]
	public GameObject conwayObject;

	[Header("Grid settings")]
	public uint gridWidth;
	public uint gridHeight;
	public uint gridDepth;

	[Header("Simulation settings")]
	public bool autoProgress;
	[Range(0.01f,5f)]
	public float autoProgressionTime = 1;
	public uint toLiveMin, toLiveMax, toReviveMin, toReviveMax;
	private float deltaTime;

	private bool[,,] grid;
	private List<GridItem> gridItemsToKill, gridItemsToLive;
	private GameObject[,,] objects;

	//	Check if a given point is within the bounds of the grid
	private bool IsPointInBounds(uint x, uint y, uint z){
		return x < gridWidth && y < gridHeight && z < gridDepth;
	}

	private bool IsPointInBounds(GridItem gridItem){
		return IsPointInBounds (gridItem.x, gridItem.y, gridItem.z);
	}

	//	Get number of neighbors that are alive
	private uint GetLivingNeighborsCount(uint x, uint y, uint z)
	{
		List<GridItem> neighbors = GetNeighborsAt(x, y, z);
		uint livingCount = 0;
		foreach (GridItem neighbor in neighbors)
		{
			if (grid [neighbor.z, neighbor.y, neighbor.x])
				livingCount++;
		}
		return livingCount;
	}

	//	Get number of neighbors at a specific (x,y,z) point
	private uint GetNeighborsCount(uint x, uint y, uint z)
	{
		return (uint)GetNeighborsAt(x, y, z).Count;
	}

	//	Get neighbors at a specific (x,y,z) point
	private List<GridItem> GetNeighborsAt(uint x, uint y, uint z)
	{
		List<GridItem> neighbors = new List<GridItem> ();

		//	Sanity bounds check
		if (!IsPointInBounds(x,y,z)) {
			return neighbors;
		}

		for( int xCheck=(int)x-1; xCheck<=x+1; xCheck++ )
		{
			for( int yCheck=(int)y-1; yCheck<=y+1; yCheck++ )
			{
				for( int zCheck=(int)z-1; zCheck<=z+1; zCheck++ )
				{
					if (xCheck < 0 || yCheck < 0 || zCheck < 0)
						continue;

					if (IsPointInBounds((uint)xCheck, (uint)yCheck, (uint)zCheck) && (xCheck != x || yCheck != y || zCheck != z))
						neighbors.Add(new GridItem((uint)xCheck, (uint)yCheck, (uint)zCheck));
				}
			}
		}

		return neighbors;
	}

	//	Instantiate the given prefab at the point, settings it as a child of the Grid
	private GameObject CreateObjectAt(GameObject prefab, uint x, uint y, uint z)
	{
		GameObject obj = Instantiate (prefab, transform);
		obj.transform.position = new Vector3 (x, y, z);
		conwayObject.GetComponent<ConwayObject>().UseDeadMaterial();
		return obj;
	}

	//	Setup the visual grid
	private void SetupGrid()
	{
		objects = new GameObject[gridDepth, gridHeight, gridWidth];
		for( uint z = 0; z < gridDepth; z++ )
		{
			for (uint y = 0; y < gridHeight; y++)
			{
				for (uint x = 0; x < gridWidth; x++)
				{
					objects[z, y, x] = CreateObjectAt(conwayObject, x, y, z);
				}
			}
		}
	}

	//	Check if the given prefab object is dead
	private bool IsDeadObject(GameObject obj)
	{
		return obj.GetComponent<ConwayObject>().IsDead;
	}

	//	Check if the given prefab object if alive
	private bool IsLiveObject(GameObject obj)
	{
		return obj.GetComponent<ConwayObject>().IsLiving;
	}

	//	Render updates to the visual grid
	private void RenderGrid()
	{
		for( uint z = 0; z < gridDepth; z++ )
		{
			for (uint y = 0; y < gridHeight; y++)
			{
				for (uint x = 0; x < gridWidth; x++)
				{
					GameObject obj = objects[z, y, x];
					bool gridStatus = grid[z, y, x];
					if (gridStatus && !IsLiveObject(obj))
					{
						obj.GetComponent<ConwayObject>().UseLivingMaterial();
					}
					else if (!gridStatus && !IsDeadObject(obj))
					{
						obj.GetComponent<ConwayObject>().UseDeadMaterial();
					}
				}
			}
		}
	}

	//	Perform neighbor calculations and render the grid
	private void StepForward()
	{
		//	Start with a blank grid
		gridItemsToKill = new List<GridItem>();
		gridItemsToLive = new List<GridItem>();

		for( uint z = 0; z < gridDepth; z++)
		{
			for (uint y = 0; y < gridHeight; y++)
			{
				for (uint x = 0; x < gridWidth; x++)
				{
					bool isAlive = grid[z, y, x];
					uint neighborsTotal = GetNeighborsCount(x, y, z);
					uint neighborsAlive = GetLivingNeighborsCount(x, y, z);

					uint neighborLiveMin = toLiveMin;
					uint neighborLiveMax = toLiveMax;
					uint neighborReviveMin = toReviveMin;
					uint neighborReviveMax = toReviveMax;
					if (neighborsTotal < MAX_NEIGHBORS)
					{
						//float neighborFactor = neighborsTotal / MAX_NEIGHBORS;
						neighborLiveMin /= 2;
						neighborLiveMax /= 2;
						neighborReviveMin /= 2;
						neighborReviveMax /= 2;
					}

					if (isAlive)
					{
						if (neighborsAlive >= neighborLiveMin && neighborsAlive <= neighborLiveMax)
							gridItemsToLive.Add(new GridItem(x, y, z));
						else
							gridItemsToKill.Add(new GridItem(x, y, z));
					}
					else
					{
						if (neighborsAlive >= neighborReviveMin && neighborsAlive <= neighborReviveMax)
							gridItemsToLive.Add(new GridItem(x, y, z));
						else
							gridItemsToKill.Add(new GridItem(x, y, z));
					}
				}
			}
		}
		
		foreach( GridItem killItem in gridItemsToKill )
		{
			grid [killItem.z, killItem.y, killItem.x] = false;
		}
		foreach (GridItem liveItem in gridItemsToLive) {
			grid [liveItem.z, liveItem.y, liveItem.x] = true;
		}

		//	Render the changes from this step
		RenderGrid ();
	}

	void Awake()
	{
		//	Initialize index for living grid items
		grid = new bool[gridDepth, gridHeight, gridWidth];

		//	Randomly initialize the grid
		for( uint z = 0; z < gridDepth; z++)
		{
			for (uint y = 0; y < gridHeight; y++)
			{
				for (uint x = 0; x < gridWidth; x++)
				{
					grid[z, y, x] = Random.value > 0.5f;
				}
			}
		}
		
		//	Render
		SetupGrid();
		RenderGrid();
	}

	void Update ()
	{
		//	If not auto-progressing, advance when user hits Return
		if ( !autoProgress && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) )
		{
			StepForward ();
		}

		//	If auto-progressing, advance when time has elapsed
		if (autoProgress)
		{
			deltaTime += Time.deltaTime;
			if (deltaTime >= autoProgressionTime)
			{
				StepForward ();
				deltaTime -= autoProgressionTime;
			}
		}

	}
}
