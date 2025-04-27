using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
/// <summary>
/// This class has all the room information as its name, what should be done here, the time to be spent here,
/// where it is located and if there are more than one places to go here.
/// </summary>
public class Rooms : MonoBehaviour
{
    // Enum to define the type of room
    public enum whatCanDo { Eat, Rest, Fun, Escape }
    
    [SerializeField, Tooltip("What is the name of this place")]
    //stores the Name of the place
    private string placeName; 
    [SerializeField, Tooltip("What should be done here")] 
    //stores the whatToDo in the place
    private whatCanDo whatToDo; 
    [SerializeField, Tooltip("Can he go to one or more places inside this area?")]
    //stores if the place has one or more places to go
    private bool canGoToMoreThanOnePlace;
    [SerializeField, ShowIf(nameof(ShouldShowField)), Tooltip("inside the place is there any place specific he should try to go? if no put the whole area here as well.")] 
    //stores where the agent wants to go in that area
    private BoxCollider whereToGo;
    [SerializeField, ShowIf(nameof(canGoToMoreThanOnePlace)), Tooltip("The amount of places he can go in that area")]
    //stores all the places he can go in that area
    private List<BoxCollider> placesHeCanGo;
    [SerializeField, ShowIf(nameof(canGoToMoreThanOnePlace)), Tooltip("The maximum people per place u set on the placesHeCanGo")]
    //stores the maximum amount of people per place
    private List<int> maxPeoplePerPlace;
    [SerializeField, Tooltip("The minimum amount of time a person would want to spend in this area")]
    //stores the minimum amount of time that will be spen here
    private int minTimeToSpendHere = 30;
    [SerializeField, Tooltip("The maximum amount of time a person would want to spend in this area")]
    //stores the maximum amount of time that will be spen here
    private int maxTimeToSpendHere = 60;
    [SerializeField, Tooltip("The whole area")]
    //stores the whole are
    private BoxCollider wholeArea;

    //stores the current amount of people 
    private int currentAmountOfPeople;

    private bool ShouldShowField() => !canGoToMoreThanOnePlace;
    ///Makes every Variable have a public Read and mantain the Write private
    public bool CanGoToMoreThanOnePlace => canGoToMoreThanOnePlace;
    public whatCanDo WhatToDo => whatToDo;
    public string PlaceName => placeName;
    public BoxCollider WhereToGo => whereToGo;
    public List<BoxCollider> PlacesHeCanGo => placesHeCanGo;
    public List<int> MaxPeoplePerPlace => maxPeoplePerPlace;
    public int CurrentAmountOfPeople => currentAmountOfPeople;
    public int MaxTimeToSpendHere => maxTimeToSpendHere;
    public int MinTimeToSpendHere => minTimeToSpendHere;
    public BoxCollider WholeArea => wholeArea;
    


    /// <summary>
    /// add 1 to currentAmountOfPeople
    /// </summary>
    public void AddAmountOfPeople(){
        currentAmountOfPeople++;
    }
    /// <summary>
    /// takes 1 from currentAmountOfPeople
    /// </summary>
    public void TakeAmountOfPeople(){
        currentAmountOfPeople--;
    }

}
