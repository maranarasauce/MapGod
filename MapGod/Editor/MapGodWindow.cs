using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static MapGod;

public class MapGodWindow : EditorWindow
{
    [MenuItem("Tools/Map God")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MapGodWindow), false, "Map God Inspector");
    }

    private float curMessageTime;
    private double lastTime;
    private string currentMessage;
    private MapGod god;
    private void OnGUI()
    {
        if (god == null)
        {
            god = FindObjectOfType<MapGod>();
            if (god == null)
            {
                GUILayout.Label("GOD IS DEAD", EditorStyles.boldLabel);
                GUILayout.Label("Add a Map God to your level, preferrably on the Map Info GameObject.", EditorStyles.label);

                Rect btnRect = GUILayoutUtility.GetLastRect();
                btnRect.y += 30f;
                btnRect.height += 7f;

                if (GUI.Button(btnRect, "Do it for me"))
                {
                    MapInfo info = FindObjectOfType<MapInfo>();
                    if (info == null)
                        return;
                    god = info.gameObject.AddComponent<MapGod>();
                }
                return;
            }
        }

        
        Rect btn = new Rect(5f, buttonSpacing, buttonWidth, buttonHeight);
        GUIStyle title = EditorStyles.boldLabel;
        title.alignment = TextAnchor.MiddleCenter;
        GUI.Label(btn, "<-= Map God =->", title);

        btn = PushRect(btn);
        if (GUI.Button(btn, "Add New Room"))
        {
            NewRoom();
            SetMessage($"New room created.", 3f);
        }

        btn = PushRect(btn);
        if (GUI.Button(btn, "Add Wave to Selected Room"))
        {
            MapGod.Room room = GetSelectedRoom();
            if (IsRoomValid(room))
            {
                room = NewWave(room);
                god.rooms[room.index] = room;
                SetMessage($"Wave added to {room.Root.gameObject.name}.", 3f);
            }
            else
                SetMessage($"Could not find selected room.", 3f);
        }

        btn = PushRect(btn);
        if (GUI.Button(btn, "Toggle Wave Enemies"))
        {
            GameObject curObj = Selection.activeGameObject;
            if (curObj == null)
            {
                SetMessage("Could not find the selected wave.", 2f);
            } else
            {
                ActivateNextWave wave = curObj.GetComponentInParent<ActivateNextWave>();
                if (wave == null)
                {
                    SetMessage("Could not find the selected wave.", 2f);
                }
                else ToggleWaveEnemies(wave);
            }
                
        }
        /*btn = Iterate(btn);
        if (GUI.Button(btn, "Remove Selected Wave"))
        {
            MapGod.Room room = GetSelectedRoom();
            if (IsRoomValid(room))
            {
                GameObject curObj = Selection.activeGameObject;
                ActivateNextWave wave = curObj.GetComponentInParent<ActivateNextWave>();
                room.Waves.Remove(wave);
                Undo.DestroyObjectImmediate(wave.gameObject);
                room = CombRoom(room);
                god.rooms[room.index] = room;
            }
        }*/

        btn = PushRect(btn);
        if (GUI.Button(btn, "Validate Selected Room"))
        {
            MapGod.Room room = GetSelectedRoom();
            if (IsRoomValid(room))
            {
                room = CombRoom(room);
                god.rooms[room.index] = room;
                SetMessage($"Room {room.Root.gameObject.name} validated.", 3f);
            }
            else
                SetMessage($"Could not find selected room.", 3f);
        }

        Rect validateRoom = PushRect(btn);
        validateRoom.width /= 2f;
        Rect validatePoint = validateRoom;
        validatePoint.x += validatePoint.width;

        if (GUI.Button(validateRoom, "Validate All Rooms"))
        {
            for (int i = 0; i < god.rooms.Count; i++)
            {
                MapGod.Room room = god.rooms[i];
                if (room.Root == null)
                {
                    god.rooms.RemoveAt(i);
                    continue;
                }
                room = CombRoom(room);
                god.rooms[i] = room;
            }
            SetMessage("Rooms validated.", 3f);
        }

        if (GUI.Button(validatePoint, "Validate Checkpoints"))
        {
            CheckPoint[] checkpoints = FindObjectsOfType<CheckPoint>();
            god.checkpoints = new List<CheckPoint>();
            god.checkpoints.AddRange(checkpoints);

            foreach (CheckPoint checkpoint in checkpoints)
            {
                CombCheckpoint(checkpoint);
            }
            SetMessage("Checkpoints validated.", 3f);
        }
        btn = PushRect(PushRect(btn));

        double messageDelta = EditorApplication.timeSinceStartup - lastTime;
        lastTime = EditorApplication.timeSinceStartup;

        if (curMessageTime > 0f)
        {
            curMessageTime -= (float)messageDelta;
            GUI.Label(btn, currentMessage, title);
        }
    }

    private void SetMessage(string msg, float time)
    {
        curMessageTime = time;
        currentMessage = msg;
    }

    const float buttonWidth = 300f;
    const float buttonHeight = 30f;
    const float buttonSpacing = 10f;
    Rect PushRect(Rect rect)
    {
        rect.y += buttonHeight;
        rect.y += buttonSpacing;
        return rect;
    }

    private MapGod.Room GetSelectedRoom()
    {
        GameObject curObj = Selection.activeGameObject;
        if (curObj == null)
            return new Room();
        Transform root = curObj.transform.root; 
        MapGod.Room room = god.GetRoomFromRoot(root);
        return room;
    }

    private bool IsRoomValid(MapGod.Room room)
    {
        return room.Root != null;
    }

    private void NewRoom()
    {
        MapGod.Room newRoom = new MapGod.Room();

        Transform root = new GameObject($"{god.rooms.Count + 1} - New Room").transform;

        Transform @static = new GameObject("Static").transform;
        @static.parent = root;
        Transform dynamic = new GameObject("Dynamic").transform;
        dynamic.parent = root;
        dynamic.gameObject.AddComponent<GoreZone>();
        Transform startDoors = new GameObject("Start Doors").transform;
        startDoors.parent = root;
        Transform endDoors = new GameObject("End Doors").transform;
        endDoors.parent = root;

        GameObject activateCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        activateCube.name = "Activate Arena";
        activateCube.layer = 16;
        activateCube.transform.parent = dynamic;

        BoxCollider activateTrigger = activateCube.GetComponent<BoxCollider>();
        activateTrigger.isTrigger = true;

        ActivateArena activateArena = activateCube.AddComponent<ActivateArena>();

        newRoom.Root = root;
        newRoom.Static = @static;
        newRoom.Dynamic = dynamic;
        newRoom.StartArena = activateArena;
        newRoom.StartDoorRoot = startDoors;
        newRoom.EndDoorRoot = endDoors;
        newRoom.index = god.rooms.Count;

        newRoom = NewWave(newRoom);

        god.rooms.Add(newRoom);
    }

    private MapGod.Room NewWave(MapGod.Room room)
    {
        if (room.Waves == null)
            room.Waves = new List<ActivateNextWave>();

        GameObject waveGrp = new GameObject($"Wave {room.Waves.Count + 1}");
        waveGrp.transform.parent = room.Dynamic;
        ActivateNextWave wave = waveGrp.AddComponent<ActivateNextWave>();
        room.Waves.Add(wave);
        room.StartArena.onlyWave = room.Waves.Count == 1;
        EditorUtility.SetDirty(room.StartArena);

        return room;
    }

    private void ToggleWaveEnemies(ActivateNextWave wave)
    {
        GameObject[] enemies = GetEnemies(wave.transform);
        if (enemies.Length == 0)
        {
            SetMessage("No enemies found in wave.", 2f);
            return;
        }
        bool state = !enemies[0].activeSelf;
        string @in = "";
        if (!state)
            @in = "in";
        SetMessage($"Enemies toggled to {@in}active.", 2f);
        foreach (GameObject e in enemies)
        {
            e.SetActive(state);
        }
    }

    private GameObject[] GetEnemies(Transform Root)
    {
        List<GameObject> nextEnemies = new List<GameObject>();
        for (int i = 0; i < Root.childCount; i++)
        {
            nextEnemies.Add(Root.GetChild(i).gameObject);
        }
        return nextEnemies.ToArray();
        /*
        EnemyIdentifier[] nextEids = Root.gameObject.GetComponentsInChildren<EnemyIdentifier>(true);
        foreach (EnemyIdentifier eid in nextEids)
        {
            nextEnemies.Add(eid.gameObject);
        }
        return nextEnemies.ToArray();*/
    }

    private MapGod.Room CombRoom(MapGod.Room room)
    {
        List<Door> startList = new List<Door>();
        List<Door> endList = new List<Door>();
        List<GameObject> activeRooms = new List<GameObject>();
        List<GameObject> inactiveRooms = new List<GameObject>();

        Door[] startFound = room.StartDoorRoot.gameObject.GetComponentsInChildren<Door>(true);
        Door[] endFound = room.EndDoorRoot.gameObject.GetComponentsInChildren<Door>(true);
        startList.AddRange(startFound);
        endList.AddRange(endFound);

        if (room.index - 1 >= 0)
        {
            MapGod.Room lastRoom = god.rooms[room.index - 1];
            startList.AddRange(god.rooms[room.index - 1].EndDoorRoot.GetComponentsInChildren<Door>(true));
            activeRooms.Add(lastRoom.Root.gameObject);
        }
        if (room.index + 1 < god.rooms.Count)
        {
            MapGod.Room nextRoom = god.rooms[room.index + 1];
            endList.AddRange(nextRoom.StartDoorRoot.GetComponentsInChildren<Door>(true));
            activeRooms.Add(nextRoom.Root.gameObject);
        }
        if (room.index - 2 >= 0)
        {
            MapGod.Room secondaryRoom = god.rooms[room.index - 2];
            inactiveRooms.Add(secondaryRoom.Root.gameObject);
        }
        if (room.index + 2 < god.rooms.Count)
        {
            MapGod.Room secondaryRoom = god.rooms[room.index + 2];
            inactiveRooms.Add(secondaryRoom.Root.gameObject);
        }

        room.StartDoors = startList.ToArray();
        room.EndDoors = endList.ToArray();

        List<Door> arenaDoors = new List<Door>();
        arenaDoors.AddRange(startList);
        arenaDoors.AddRange(endList);
        room.StartArena.doors = arenaDoors.ToArray();

        foreach (Door door in endList)
        {
            door.activatedRooms = activeRooms.ToArray();
            door.deactivatedRooms = inactiveRooms.ToArray();
            EditorUtility.SetDirty(door);
        }


        ActivateNextWave lastWave = room.Waves[room.Waves.Count - 1];
        if (lastWave == null)
            room.Waves.RemoveAt(room.Waves.Count - 1);

        for (int i = 0; i < room.Waves.Count - 1; i++)
        {
            ActivateNextWave wave = room.Waves[i];
            if (wave == null)
            {
                room.Waves.RemoveAt(i);
                continue;
            }

            ApostateWave apo = wave.gameObject.GetComponent<ApostateWave>();

            if (apo == null || apo.autoEnemyCount)
                wave.enemyCount = wave.transform.childCount;

            if (apo == null || apo.autoLastWave)
                wave.lastWave = false;

            if (apo == null || apo.autoNextEnemies)
            {
                int nI = i + 1;
                if (nI + 1 <= room.Waves.Count)
                {
                    if (room.Waves[nI] != null)
                        wave.nextEnemies = GetEnemies(room.Waves[nI].transform);
                }
            }

            if (apo == null || apo.autoDoors)
                wave.doors = null;

            EditorUtility.SetDirty(wave);
        }

        room.StartArena.enemies = GetEnemies(room.Waves[0].transform);
        EditorUtility.SetDirty(room.StartArena);

        if (lastWave == null)
            lastWave = room.Waves[room.Waves.Count - 1];
        ApostateWave lastApo = lastWave.gameObject.GetComponent<ApostateWave>();
        if (lastApo == null || lastApo.autoDoors)
            lastWave.doors = room.StartArena.doors;
        if (lastApo == null || lastApo.autoLastWave)
            lastWave.lastWave = true;
        if (lastApo == null || lastApo.autoEnemyCount)
            lastWave.enemyCount = lastWave.transform.childCount;
        EditorUtility.SetDirty(lastWave);

        EditorUtility.SetDirty(god);

        return room;
    }


    private void CombCheckpoint(CheckPoint pt)
    {
        //Finds every room inbetween current checkpoint and the next one, adds the doors and rooms to inherit
        List<Door> futureDoors = new List<Door>();
        List<GameObject> inherits = new List<GameObject>();
        int seek = god.GetRoomFromCheckPoint(pt).index;
        if (seek == -1)
            return;
        for (int i = seek; i < god.rooms.Count; i++)
        {
            MapGod.Room room = god.rooms[i];
            inherits.Add(room.Dynamic.gameObject);

            if (i == seek)
                futureDoors.AddRange(room.EndDoors);

            CheckPoint nPt = god.GetCheckPointForRoom(room.index);
            if (nPt != null)
            {
                futureDoors.AddRange(room.StartDoors);
                break;
            } else
            {
                futureDoors.AddRange(room.EndDoors);
            }
        }
        pt.doorsToUnlock = futureDoors.ToArray();
        pt.roomsToInherit = inherits;

        EditorUtility.SetDirty(pt);
    }
}
