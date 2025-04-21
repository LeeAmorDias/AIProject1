using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
public class Rooms : MonoBehaviour
{
    // Enum to define the type of room
    public enum whatCanDo { Eat, Rest, Fun }
    // Name of the unit 
    [SerializeField] 
    private string placeName; 
    [SerializeField] 
    private whatCanDo whatToDo; 
    [SerializeField]
    private bool canGoToMoreThanOnePlace;
    [SerializeField, ShowIf(nameof(ShouldShowField))] 
    private BoxCollider whereToGo;
    [SerializeField, ShowIf(nameof(canGoToMoreThanOnePlace))]
    private List<BoxCollider> placesHeCanGo;
    [SerializeField, ShowIf(nameof(canGoToMoreThanOnePlace))]
    private List<int> maxPeoplePerPlace;
    [SerializeField]
    private int minTimeToSpendHere = 30;
    [SerializeField]
    private int maxTimeToSpendHere = 60;
    [SerializeField]
    private BoxCollider wholeArea;

    private int currentAmountOfPeople;

    private bool ShouldShowField() => !canGoToMoreThanOnePlace;
    public bool CanGoToMoreThanOnePlace => canGoToMoreThanOnePlace;
    public whatCanDo WhatToDo => whatToDo;
    public string PlaceName => PlaceName;
    public BoxCollider WhereToGo => whereToGo;
    public List<BoxCollider> PlacesHeCanGo => placesHeCanGo;
    public List<int> MaxPeoplePerPlace => maxPeoplePerPlace;
    public int CurrentAmountOfPeople => currentAmountOfPeople;
    public int MaxTimeToSpendHere => maxTimeToSpendHere;
    public int MinTimeToSpendHere => minTimeToSpendHere;
    public BoxCollider WholeArea => wholeArea;
    



    public void AddAmountOfPeople(){
        currentAmountOfPeople++;
    }

    public void TakeAmountOfPeople(){
        currentAmountOfPeople--;
    }

}
