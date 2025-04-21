using System.Collections.Generic;
using UnityEngine;

public class AreasController : MonoBehaviour
{
    private List<Rooms> allAreas = new List<Rooms>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        GetAllAreas();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GetAllAreas(){
        foreach(Transform child in this.transform){
            if (child.GetComponent<Rooms>() != null)
            {
                allAreas.Add(child.GetComponent<Rooms>());
            }
        }
    }

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
}
