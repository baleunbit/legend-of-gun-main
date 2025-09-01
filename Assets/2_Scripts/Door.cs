using UnityEngine;

public class Door : MonoBehaviour
{
    public int targetRoomIndex;
    private RoomGenerator roomGen;

    void Start()
    {
        roomGen = FindFirstObjectByType<RoomGenerator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject targetRoom = roomGen.GetRoom(targetRoomIndex);
            if (targetRoom != null)
            {
                Room roomScript = targetRoom.GetComponent<Room>();
                if (roomScript != null && roomScript.entryPoint != null)
                {
                    other.transform.position = roomScript.entryPoint.position;
                }
            }
        }
    }
}
