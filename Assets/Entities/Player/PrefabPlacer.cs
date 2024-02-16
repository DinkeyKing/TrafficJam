using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PrefabPlacer : MonoBehaviour
{
    public Car carPrefab;
    public Tilemap roadTilemap;

    public float prefabZOffset = -5.0f;


    public void Awake()
    {
        carPrefab.roadTilemap = roadTilemap;
    }

    public void OnPlaceObject(InputAction.CallbackContext context)
    {
        if (!enabled || !context.performed) return;


        Vector2 pointerScreenPosition = Pointer.current.position.ReadValue();

        if (UIManager.IsPointerOverUI(pointerScreenPosition)) return;

        Vector2 pointerPosition = Camera.main.ScreenToWorldPoint(pointerScreenPosition);

        Vector3Int cellPosition = roadTilemap.WorldToCell(pointerPosition);

        if (roadTilemap.GetTile(cellPosition) == null) return;  // No road tile is on that cell position


        Vector3 placePosition = roadTilemap.GetCellCenterWorld(cellPosition);
        placePosition.z = prefabZOffset;

       Instantiate(carPrefab, placePosition, Quaternion.identity);

    }
}
