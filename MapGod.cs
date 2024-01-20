using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapGod;

public class MapGod : MonoBehaviour
{
    [Serializable]
    public struct Room {
        public int index;
        public Transform Root;
        public Transform Static;
        public Transform Dynamic;
        public ActivateArena StartArena;
        public List<ActivateNextWave> Waves;
        public Transform StartDoorRoot;
        public Transform EndDoorRoot;
        public Door[] StartDoors;
        public Door[] EndDoors;
    }

    public List<CheckPoint> checkpoints = new List<CheckPoint>();
    public List<Room> rooms = new List<Room>();

    public CheckPoint GetCheckPointForRoom(int roomIndice)
    {
        Room room = rooms[roomIndice];
        foreach (CheckPoint pt in checkpoints)
        {
            if (room.Root.gameObject == pt.toActivate)
                return pt;
        }
        return null;
    }

    public Room GetRoomFromCheckPoint(CheckPoint point)
    {
        foreach (Room room in rooms)
        {
            if (room.Root.gameObject == point.toActivate)
                return room;
        }
        return new Room();
    }

    public MapGod.Room GetRoomFromRoot(Transform root)
    {
        foreach (Room room in rooms)
        {
            if (room.Root == root)
            {
                return room;
            }
        }
        return new Room();
    }

    public MapGod.Room GetRoomFromDynamic(Transform dynamic)
    {
        foreach (Room room in rooms)
        {
            if (room.Dynamic == dynamic)
            {
                return room;
            }
        }
        return new Room();
    }
}
