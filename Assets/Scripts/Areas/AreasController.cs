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
    /// <returns>The closest available exit, or null if none is found</returns>
    public Rooms GiveFastestValidPathToRoom(Rooms.whatCanDo state, NavMeshAgent agent)
    {
        float bestPathLength = Mathf.Infinity;
        Rooms bestRoom = null;

        List<Rooms> matchingRooms = new List<Rooms> ();
        
        //populates a list of rooms that match the given state
        foreach (Rooms room in allAreas){
            if(room.WhatToDo == state){
                matchingRooms.Add(room);
            }
        }

        for (int i = 0; i<matchingRooms.Count; i++)
        {
            NavMeshPath path = new NavMeshPath();
            //CalculatePath checks if it is possible for the agent to go to a
            //designated position, in order for this to take into account the 
            //explosions as they happen it is required that the explosions 
            //carve the navmesh as they expand, which they do.
            if (agent.CalculatePath(matchingRooms[i].transform.position, path)
                && path.status == NavMeshPathStatus.PathComplete)
                {
                    float pathLength = GetPathLength(path);

                    //stores the shortest path by comparing each with a previous
                    //best and replacing it if it is shorter
                    if(pathLength < bestPathLength)
                    {
                        bestPathLength = pathLength;
                        bestRoom = matchingRooms[i];
                    }
                }
        }
        
        //this method can return null if all exits are blocked by an explosion/fire
        return bestRoom;
    }
    /// <summary>
    /// recieves a navmesh path and calculates the length of it
    /// </summary>
    /// <param name="path"></param>
    /// <returns>the length of the given path</returns>
    private float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        //path.corners contains all the points of a path
        if(path.corners.Length < 2)
            return length;

        //because we check a point and the one after it we need the -1 in
        //the condition so we don't get an IndexOutOfRange Exception
        for(int i = 0; i<path.corners.Length-1; i++)
        {
            length += Vector3.Distance(path.corners[i], path.corners[i+1]);
        }

        return length;
    }
}
