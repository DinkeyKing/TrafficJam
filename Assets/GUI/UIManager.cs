using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button placeRoadButton;
    public Button placeCarButton;

    public Player player;

 
    private void Awake()
    {
        placeRoadButton.onClick.AddListener(delegate { player.SetMode(Player.placementMode.ROAD_PLACEMENT); });
        placeCarButton.onClick.AddListener(delegate { player.SetMode(Player.placementMode.CAR_PLACEMENT); });
    }


    public static bool IsPointerOverUI(Vector2 screenPosition)
    {
        PointerEventData eventData = new(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
}
