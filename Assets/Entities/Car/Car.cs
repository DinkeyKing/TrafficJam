using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Car : MonoBehaviour
{
    public float minSpeed = 5.0f;
    public float maxSpeed = 10.0f;
    public float laneOffsetLength = 0.15f;
    public float maxFollowDistance = 1.0f;
    public float minFollowDistance = 0.5f;

    public float rayOffsetLength = 0.2f;

    public float boxRightOffsetLength = 0.25f;
    public float boxUpOffsetLength = 0.5f;
    public Vector2 boxSize = new(0.5f,  0.5f);
    public float maxYieldTime = 3.0f;
    public float yieldTime = 0.0f;

    public float speed = 0.0f;
    public float defaultSpeed = 0.0f;
    public Vector3 velocity;
    public Tilemap roadTilemap;  // The prefab placer will assign this reference
    public AIState state;

    private bool positionReached;
    private Vector3 moveDirection;
    private Vector3 targetPosition;
    private Vector3 currentOffset;
    private Vector3Int targetTilePosition;
    private Vector3Int previousTilePosition;

    private const float TOLARANCE = 0.01f;
    private const float SQR_TOLORANCE = TOLARANCE * TOLARANCE;

    public enum AIState
    {
        STOP,
        DRIVE,
        YIELD
    }


    // Debug draws
    private void OnDrawGizmos()
    {
        // Right car detection box
        Vector3 offset = transform.up * boxUpOffsetLength + transform.right * boxRightOffsetLength;
        Vector3 overlapPosition = transform.position + offset;

        Gizmos.DrawWireCube(overlapPosition, boxSize);
    }


    private bool IsOnIntersection()
    {
        Vector3Int currentTilePosition = roadTilemap.WorldToCell(transform.position);

        List<Vector3Int> neighborTiles = new();


        // NORTH
        Vector3Int northTilePosition = currentTilePosition + Vector3Int.up;
        var northTile = roadTilemap.GetTile(northTilePosition);
        if (northTile != null) neighborTiles.Add(northTilePosition);

        // EAST
        Vector3Int eastTilePosition = currentTilePosition + Vector3Int.right;
        var eastTile = roadTilemap.GetTile(eastTilePosition);
        if (eastTile != null) neighborTiles.Add(eastTilePosition);

        // SOUTH
        Vector3Int southTilePosition = currentTilePosition + Vector3Int.down;
        var southTile = roadTilemap.GetTile(southTilePosition);
        if (southTile != null) neighborTiles.Add(southTilePosition);

        // WEST
        Vector3Int westTilePosition = currentTilePosition + Vector3Int.left;
        var westTile = roadTilemap.GetTile(westTilePosition);
        if (westTile != null) neighborTiles.Add(westTilePosition);

        if (neighborTiles.Count > 2) return true;

        return false;
    }


    private bool PriorityToTheRight()
    {
        Vector3 offset = transform.up * boxUpOffsetLength + transform.right * boxRightOffsetLength;
        Vector3 overlapPosition = transform.position + offset;

        var collider = Physics2D.OverlapBox(overlapPosition, boxSize, 0.0f);


        if (collider != null)
        {
            Car otherCar = collider.GetComponent<Car>();

            if (otherCar != null)
            {
                // We need to know if the detected car is moving towards the intersection/us.
                Vector3 fromOtherToSelf = (transform.position - collider.transform.position).normalized;

                if (Vector3.Dot(fromOtherToSelf, collider.transform.up) > 0.0f && otherCar.speed > 0.0 && IsOnIntersection())
                {
                    return true;
                }
            }
        }

        return false;
    }


    // Selects a random tile position from the given position's neighbouring tiles. If dead end, returns the given previous tile's position. 
    private Vector3Int DecideNextTilePosition(Vector3Int currentTilePosition, Vector3Int previousTilePosition)
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

        // Pick a random direction from the possible options and return it.
        int randomIndex = Random.Range(0, possibleForwardTiles.Count);

        return possibleForwardTiles[randomIndex];
    }


    private void SetSpeed()
    {
        Vector3 rayOffset = moveDirection * rayOffsetLength;  // Make sure ray doesn't hit own collider.
        Vector3 rayOrigin = transform.position + rayOffset;
        var hit = Physics2D.Raycast(rayOrigin, moveDirection, maxFollowDistance);
        
        Debug.DrawRay(rayOrigin, moveDirection * maxFollowDistance);
        
        if (hit.collider != null)
        {
            Car carInFront = hit.collider.GetComponent<Car>();

            if (carInFront != null)
            {
                if (Vector3.Dot(moveDirection, carInFront.moveDirection) > 0.0f)
                {
                    if (hit.distance < minFollowDistance) speed = carInFront.speed * 0.5f;  // If too close to the car in front, slow down to make some space and try to avoid intersections.
                    else speed = Mathf.Min(carInFront.speed, defaultSpeed);  // Pickup the followed car's speed only if it's smaller than the default speed.
                    
                    return;
                }
            }
        }

        speed = defaultSpeed;
    }


    // The car moves around on the roads, picking random directions.
    private void Drive()
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

            if (laneOffset != currentOffset)
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


        moveDirection = (targetPosition - transform.position).normalized;

        // Set the car rotation, so it looks towards the target point.
        float angle = Vector2.SignedAngle(Vector2.up, moveDirection); // Calculate the angle from the object's 'up' to the direction
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); // Apply rotation

        SetSpeed();
        velocity = speed * moveDirection;
        Vector3 motion = Time.deltaTime * velocity;

        transform.position += motion;


        if ((transform.position - targetPosition).sqrMagnitude < SQR_TOLORANCE)  // Check if close to target position.
        {
            positionReached = true;

            Vector3Int currentTilePosition = roadTilemap.WorldToCell(transform.position);
        }
    }


    private void Awake()
    {
        if (roadTilemap == null)  // The tilemap reference must be assigned before instantiated.
        {
            Destroy(gameObject);
        }

        defaultSpeed = minSpeed + UnityEngine.Random.value * (maxSpeed - minSpeed);

        state = AIState.DRIVE;

        Vector3Int currentTilePosition = roadTilemap.WorldToCell(transform.position);
        previousTilePosition = currentTilePosition;
        positionReached = true;
    }
    

    void Update()
    {
        // State transitions
        switch (state)
        {
            case AIState.STOP:
                // The traffic light will set and change this state
                break;

            case AIState.DRIVE:

                if (PriorityToTheRight())
                {
                    yieldTime = maxYieldTime;
                    state = AIState.YIELD;
                }
                break;

            case AIState.YIELD:

                if (!PriorityToTheRight() || yieldTime <= 0.0f) state = AIState.DRIVE;

                break;
        }

        // State behaviours
        switch (state)
        {
            case AIState.STOP:
                speed = 0.0f;
                velocity = Vector3.zero;
                break;

            case AIState.YIELD:
                speed = 0.0f;
                velocity = Vector3.zero;

                yieldTime -= Time.deltaTime;
                break;

            case AIState.DRIVE:
                Drive();
                break;
        }
    }



}
