using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI deathCounterText;  
    [SerializeField]
    private TextMeshProUGUI peoplePerAreaText;  

    private AgentsHandler agentsHandler;
    private AreasController parentAreas;
    /// <summary>
    /// gets the agents handler
    /// </summary>
    private void Awake()
    {
        agentsHandler = FindFirstObjectByType<AgentsHandler>();
        parentAreas = FindFirstObjectByType<AreasController>();
    }
    /// <summary>
    /// updates the Texts
    /// </summary>
    private void Update(){
        UpdateAmountOfDeaths();
        UpdatePeoplePerArea();
    }
    /// <summary>
    /// updates the amount of deaths
    /// </summary>
    private void UpdateAmountOfDeaths(){
        deathCounterText.text = "Amount of Deaths: " + agentsHandler.GetDeathCounter();
    }
    /// <summary>
    /// shows in the ui the amount of people per area and excludes the exits
    /// </summary>
    private void UpdatePeoplePerArea(){
        string tempPeoplePerArea = "";
        foreach (Rooms room in parentAreas.GetAllAreas()){
            if(room.WhatToDo != Rooms.whatCanDo.Escape)
                tempPeoplePerArea += room.PlaceName + ": " + room.CurrentAmountOfPeople + "\n";
        }   
        peoplePerAreaText.text = tempPeoplePerArea;
    }
}
