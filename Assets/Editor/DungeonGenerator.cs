using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class Room
{
    public string roomName;
    public int roomId;
    public Vector2Int position;
    public GameObject roomPrefab; // Prefab for this room

    public Room(string name, int id, Vector2Int pos, GameObject prefab)
    {
        roomName = name;
        roomId = id;
        position = pos;
        roomPrefab = prefab;
    }
}

public class DungeonGeneratorWindow : EditorWindow
{
    public List<GameObject> roomPrefabs = new List<GameObject>(); // List of room prefabs
    public int numberOfRoomsToGenerate = 10;

    private List<Room> rooms = new List<Room>();
    private Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    private List<GameObject> spawnedRooms = new List<GameObject>(); // Track spawned rooms

    [MenuItem("Tools/Dungeon Generator")]
    public static void ShowWindow()
    {
        GetWindow<DungeonGeneratorWindow>("Dungeon Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Procedural Dungeon Generator", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Number of Rooms to Generate");
        numberOfRoomsToGenerate = EditorGUILayout.IntField(numberOfRoomsToGenerate);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Room Prefabs");
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DisplayRoomPrefabs();
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Generate Dungeon"))
        {
            ClearDungeon();
            GenerateDungeon();
        }

        if (GUILayout.Button("Clear Dungeon"))
        {
            ClearDungeon();
        }

        if (GUILayout.Button("Remove Last Prefab Slot") && roomPrefabs.Count > 0)
        {
            RemoveLastPrefabSlot();
        }
    }

    private void DisplayRoomPrefabs()
    {
        for (int i = 0; i < roomPrefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            roomPrefabs[i] = (GameObject)EditorGUILayout.ObjectField(roomPrefabs[i], typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Room Prefab"))
        {
            roomPrefabs.Add(null);
        }
    }

    private void GenerateDungeon()
    {
        rooms.Clear();

        Room startingRoom = new Room("Starting Room", 0, Vector2Int.zero, GetRandomRoomPrefab());
        rooms.Add(startingRoom);

        for (int i = 1; i < numberOfRoomsToGenerate; i++)
        {
            Room selectedRoom = rooms[Random.Range(0, rooms.Count)];
            Vector2Int direction = directions[Random.Range(0, directions.Length)];

            GameObject randomPrefab = GetRandomRoomPrefab();
            Bounds prefabBounds = GetPrefabBounds(randomPrefab);

            Vector2Int newRoomPosition = CalculateNewRoomPosition(selectedRoom, direction, prefabBounds);

            int attempts = 0;
            while (RoomExistsAtPosition(newRoomPosition) && attempts < 100)
            {
                selectedRoom = rooms[Random.Range(0, rooms.Count)];
                direction = directions[Random.Range(0, directions.Length)];
                newRoomPosition = CalculateNewRoomPosition(selectedRoom, direction, prefabBounds);
                attempts++;
            }

            if (!RoomExistsAtPosition(newRoomPosition))
            {
                Room newRoom = new Room("Room " + (i), i, newRoomPosition, randomPrefab);
                rooms.Add(newRoom);
                // Connect the rooms or add the corridor between selectedRoom and newRoom
                // Update your dungeon layout based on the generated rooms and connections
            }
        }

        SpawnDungeon();
    }


    private Vector2Int CalculateNewRoomPosition(Room selectedRoom, Vector2Int direction, Bounds prefabBounds)
    {
        Vector2Int newRoomPosition = selectedRoom.position + direction;

        // Consider the bounds to avoid overlap
        newRoomPosition.x += Mathf.RoundToInt(prefabBounds.size.x) * direction.x;
        newRoomPosition.y += Mathf.RoundToInt(prefabBounds.size.z) * direction.y;

        return newRoomPosition;
    }


    private Bounds GetPrefabBounds(GameObject prefab)
    {
        if (prefab != null)
        {
            Collider collider = prefab.GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds;
            }
            else
            {
                Debug.LogWarning("Prefab doesn't have a collider: " + prefab.name);
            }
        }
        return new Bounds(Vector3.zero, Vector3.zero);
    }

    private void SpawnDungeon()
    {
        foreach (Room room in rooms)
        {
            if (room.roomPrefab != null)
            {
                GameObject instantiatedRoom = Instantiate(room.roomPrefab, new Vector3(room.position.x, 0f, room.position.y), Quaternion.identity);
                spawnedRooms.Add(instantiatedRoom); // Track spawned rooms
                // Adjust the position or any other setup based on the generated room's prefab
                // You might need to modify the room's position, scale, or other properties after instantiation
            }
            else
            {
                Debug.LogWarning("Room prefab is null for room: " + room.roomName);
            }
        }
    }

    private void RemoveLastPrefabSlot()
    {
        roomPrefabs.RemoveAt(roomPrefabs.Count - 1);
    }

    private void ClearDungeon()
    {
        // Destroy all spawned rooms to clear the dungeon
        foreach (GameObject room in spawnedRooms)
        {
            DestroyImmediate(room); // Destroy the instantiated rooms
        }
        spawnedRooms.Clear(); // Clear the list of spawned rooms
    }

    private bool RoomExistsAtPosition(Vector2Int position)
    {
        foreach (Room room in rooms)
        {
            if (room.position == position)
            {
                return true;
            }
        }
        return false;
    }

    private GameObject GetRandomRoomPrefab()
    {
        if (roomPrefabs.Count > 0)
        {
            return roomPrefabs[Random.Range(0, roomPrefabs.Count)];
        }
        else
        {
            Debug.LogWarning("No room prefabs assigned!");
            return null;
        }
    }
}
