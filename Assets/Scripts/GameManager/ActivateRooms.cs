using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActivateRooms : MonoBehaviour
{
   [SerializeField] private Camera miniMapCamera;

   private void Start() 
   {
    // her 3/4 saniyede bir EnableRoom methodunu cagiriyor.
    // Start'dan 0.5f saniye sonra basliyor.
    InvokeRepeating("EnableRoom",0.5f, 0.75f);
   }

    private void EnableRoom()
    {
        // butun odalari gezelim. Elimizde dictionary'miz var.
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;
            HelperUtilities.CameraWorldPositionBounds(out Vector2Int miniMapCameraWorldPositionLowerBounds,
                                                      out Vector2Int miniMapCameraWorldPositionUpperBounds, miniMapCamera);
            
            if (room.lowerBounds.x <= miniMapCameraWorldPositionUpperBounds.x && room.lowerBounds.y <= miniMapCameraWorldPositionUpperBounds.y &&
                room.upperBounds.x >= miniMapCameraWorldPositionLowerBounds.x && room.upperBounds.y >= miniMapCameraWorldPositionLowerBounds.y)
            {
                room.instantiatedRoom.gameObject.SetActive(true);
            }
            else
            {
                room.instantiatedRoom.gameObject.SetActive(false);
            }   
        }

    }
}

