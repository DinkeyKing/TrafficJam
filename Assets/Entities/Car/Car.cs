using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Car : MonoBehaviour
{
    public float minSpeed = 5.0f;
    public float maxSpeed = 10.0f;
    public float laneOffsetLength = 0.15f;
    public float maxFollowDistance = 1.0f;
    public float minFollowDistance = 0.5f;

    public float speed = 0.0f;
    public float defaultSpeed = 0.0f;
    public Vector3 velocity;
    public Tilemap roadTilemap;  // The prefab placer will assign this reference
    public AIState state;

    private bool positionReached;
    private Vector3 direction;
    private Vector3 targetPosition;
    private Vector3 currentOffset;
    private Vector3Int targetTilePosition;
    private Vector3Int previousTilePosition;

    private const float TOLARANCE = 0.01f;
    private const float SQR_TOLORANCE = TOLARANCE * TOLARANCE;

    public enum AIState
    {
        STOP,
        DRIVE
    }


    // Selects a random tile position from the given position's neighbouring tiles. If dead end, returns the given previous tile's position. 
    public Vector3Int DecideNextTilePosition(Vector3Int currentTilePosition, Vector3Int previousTilePosition)
    {
        TileBase previousTile = roadTilemap.GetTile(previousTilePosition);

        List<Vector3Int> possibleForwardTiles = new();


        // NORTH
        Vector3Int northTilePosition = currentTilePosition + Vector3Int.up;
        var northTile = roadTilemap.GetTile(northTilePosition);
        if (northTile != null && northTilePosition != previousTilePosition) possibleForwardTiles.Add(northTilePosition);

        // EAST
        Vector3Int eastTilePosition = currentTilePosition + Vector3Int.right;
        var eastTile = roadTilemap.GetTile(eastTilePosition);
        if (eastTile != null && eastTilePosition != previousTilePosition) possibleForwardTiles.Add(eastTilePosition);

        // SOUTH
        Vector3Int southTilePosition = currentTilePosition + Vector3Int.down;
        var southTile = roadTilemap.GetTile(southTilePosition);
        if (southTile != null && southTilePosition != previousTilePosition) possibleForwardTiles.Add(southTilePosition);

        // WEST
        Vector3Int westTilePosition = currentTilePosition + Vector3Int.left;
        var westTile = roadTilemap.GetTile(westTilePosition);
        if (westTile != null && westTilePosition != previousTilePosition) possibleForwardTiles.Add(westTilePosition);


        if (possibleForwardTiles.Count == 0) return previousTilePosition;  // Dead end

        // Pick a random direction from the possible options and return it
        int randomIndex = Random.Range(0, possibleForwardTiles.Count);

        return possibleForwardTiles[randomIndex];
    }


    private void SetSpeed()
    {
        var hit = Physics2D.Raycast(transform.position, direction, maxFollowDistance);
        
        Debug.DrawRay(transform.position, direction * maxFollowDistance);
        
        if (hit.collider != null)
        {
            Car carInFront = hit.collider.GetComponent<Car>();

            if (carInFront != null)
            {
                if (carInFront.speed <= speed && Vector3.Dot(velocity, carInFront.velocity) > 0.0f)
                {
                    speed = carInFront.speed;

                    if (hit.distance < minFollowDistance) speed = 0.0f;  // If too close to the car in front, stop to make som space and avoid intersection.

                    return;
                }
            }
        }

        speed = defaultSpeed;
    }

    private void Awake()
    {
        defaultSpeed = minSpeed + UnityEngine.Random.value * (maxSpeed - minSpeed);

        if (roadTilemap == null)  // The tilemap reference must be assigned before instantiated.
        {
            Destroy(gameObject);
        }

        state = AIState.DRIVE;

        Vector3Int currentTilePosition = roadTilemap.WorldToCell(transform.position);
        previousTilePosition = currentTilePosition;
        positionReached = true;
    }
    
    void Update()
    {
       if (positionReached)
        {
            Vector3Int currentTilePosition = roadTilemap.WorldToCell(transform.position);

            targetTilePosition = DecideNextTilePosition(currentTilePosition, previousTilePosition);


            // Determine offset based on target tile direction
            Vector3 laneOffset = Vector3.zero;
            if (targetTilePosition - currentTilePosition == Vector3Int.up) laneOffset = Vector3.right * laneOffsetLength;
            if (targetTilePosition - currentTilePosition == Vector3Int.right) laneOffset = Vector3.down * laneOffsetLength;
            if (targetTilePosition - currentTilePosition == Vector3Int.down) laneOffset = Vector3.left * laneOffsetLength;
            if (targetTilePosition - currentTilePosition == Vector3Int.left) laneOffset = Vector3.up * laneOffsetLength;

            if(laneOffset != currentOffset)
            {
                transform.position += laneOffset;
                currentOffset = laneOffset;
            }
            

            targetPosition = roadTilemap.GetCellCenterWorld(targetTilePosition);
            targetPosition = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);  // The z position will be constant during all movement.
            targetPosition += laneOffset;


            positionReached = false;
            previousTilePosition = currentTilePosition;
        }


        direction = (targetPosition - transform.position).normalized;
        transform.up = direction;  // Set the car rotation, so it looks towards the target point.

        SetSpeed();
        velocity = speed * direction;
        Vector3 motion = Time.deltaTime * velocity;

        transform.position += motion;


        if ((transform.position - targetPosition).sqrMagnitude < SQR_TOLORANCE)  // Check if close to target position.
        {
            positionReached = true;

            Vector3Int currentTilePosition = roadTilemap.WorldToCell(transform.position);
        }
    }
}
