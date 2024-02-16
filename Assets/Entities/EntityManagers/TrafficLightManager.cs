using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrafficLightManager : MonoBehaviour
{
    public static TrafficLightManager Instance { get; private set; }

    public Tilemap roadTilemap;
    public TrafficLight trafficLightPrefab;
    public TrafficLight[,] trafficLights;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        RefreshTrafficLights();
    }


    public void RefreshTrafficLights()
    {
        BoundsInt bounds = roadTilemap.cellBounds;
        TileBase[] allTiles = roadTilemap.GetTilesBlock(bounds);

        trafficLights = new TrafficLight[bounds.size.x, bounds.size.y];

        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];

                if (tile != null)
                {
                    // Tile at x, y
                    Vector3Int localPosition = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                    Vector3 worldPosition = roadTilemap.CellToWorld(localPosition);

                    if (trafficLights[x,y] != null)
                    {
                        int neighborCount = 0;

                        var northTile = roadTilemap.GetTile(localPosition + Vector3Int.up);
                        if (northTile != null) neighborCount++;

                        var eastTile = roadTilemap.GetTile(localPosition + Vector3Int.right);
                        if (eastTile != null) neighborCount++;

                        var southTile = roadTilemap.GetTile(localPosition + Vector3Int.down);
                        if (southTile != null) neighborCount++;

                        var westTile = roadTilemap.GetTile(localPosition + Vector3Int.down);
                        if (westTile != null) neighborCount++;

                        if (neighborCount > 2)
                        {
                            var instance = Instantiate(trafficLightPrefab, worldPosition, Quaternion.identity);
                            trafficLights[x, y] = instance;
                        }
                    }
                    
                }
                else
                {
                    trafficLights[x,y] = null;
                }
            }
        }
    }
}
