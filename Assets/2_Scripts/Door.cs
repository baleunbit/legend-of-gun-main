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
                Transform entry = targetRoom.transform.Find("EntryPoint");
                if (entry != null)
                {
                    other.transform.position = entry.position;
                }
            }
        }
    }
}
