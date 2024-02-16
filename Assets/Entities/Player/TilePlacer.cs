using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TilePlacer : MonoBehaviour
{
    public Tilemap cellTilemap;
    public Tilemap roadTilemap;
    public RuleTile roadTile;


    public void OnPlaceObject(InputAction.CallbackContext context)
    {
        if (!enabled || !context.performed) return;

        Vector2 pointerScreenPosition =  Pointer.current.position.ReadValue();

        if (UIManager.IsPointerOverUI(pointerScreenPosition)) return;

        Vector2 pointerPosition = Camera.main.ScreenToWorldPoint(pointerScreenPosition);

        Vector3Int cellPosition = roadTilemap.WorldToCell(pointerPosition);

        if (cellTilemap.GetTile(cellPosition) == null) return;  // No cell tile is on that cell position

        roadTilemap.SetTile(cellPosition, roadTile);
        
    }


    public void OnRemoveObject(InputAction.CallbackContext context)
    {
        if (!enabled || !context.performed) return;

        Vector2 pointerScreenPosition = Pointer.current.position.ReadValue();

        if (UIManager.IsPointerOverUI(pointerScreenPosition)) return;

        Vector2 pointerPosition = Camera.main.ScreenToWorldPoint(pointerScreenPosition);

        Vector3Int cellPosition = roadTilemap.WorldToCell(pointerPosition);

        roadTilemap.SetTile(cellPosition, null);

    }
}
