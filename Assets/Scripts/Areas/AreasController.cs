using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AreasController : MonoBehaviour
{
    private List<Rooms> allAreas = new List<Rooms>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        allAreas = GetAllAreas();
    }
    /// <summary>
    /// Returns a list with all the areas.
    /// </summary>
    /// <returns></returns>
    public List<Rooms> GetAllAreas(){
        List<Rooms> Areas = new List<Rooms>();
        foreach(Transform child in this.transform){
            if (child.GetComponent<Rooms>() != null)
            {
                Areas.Add(child.GetComponent<Rooms>());
            }
        }
        return Areas;
    }
    /// <summary>
    /// gives a random room according to the state that was recieved.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public Rooms CheckRoomsWithMatchingStateAndGiveRoom(Rooms.whatCanDo state){
        List<Rooms> matchingRooms = new List<Rooms> ();
        foreach (Rooms room in allAreas){
            if(room.WhatToDo == state){
                matchingRooms.Add(room);
            }
        }
        int roomnumber = Random.Range(0, matchingRooms.Count);
        return matchingRooms[roomnumber];
    }
    /// <summary>
    /// recieves a state and sees the rooms with that state and makes a list with that
    /// and with that it checks what room inside the list has the shortest path.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="agent"></param>
    /// <returns></returns>
    public Rooms GiveFastestValidPathToRoom(Rooms.whatCanDo state, NavMeshAgent agent)
    {
        float bestPathLength = Mathf.Infinity;
        Rooms bestRoom = null;

        List<Rooms> matchingRooms = new List<Rooms> ();
        
        foreach (Rooms room in allAreas){
            if(room.WhatToDo == state){
                matchingRooms.Add(room);
            }
        }

        for (int i = 0; i<matchingRooms.Count; i++)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(matchingRooms[i].transform.position, path)
                && path.status == NavMeshPathStatus.PathComplete)
                {
                    float pathLength = GetPathLength(path);

                    if(pathLength < bestPathLength)
                    {
                        bestPathLength = pathLength;
                        bestRoom = matchingRooms[i];
                    }
                }
        }

        return bestRoom;
    }
    /// <summary>
    /// recieves a navmesh path and calculates the length of it
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        if(path.corners.Length < 2)
            return length;

        for(int i = 0; i<path.corners.Length-1; i++)
        {
            length += Vector3.Distance(path.corners[i], path.corners[i+1]);
        }

        return length;
    }
}
