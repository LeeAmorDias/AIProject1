using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    private enum ActionType { Eating, Resting, Partying }

    [SerializeField]
    private float samplingRadius = 10f;
    [SerializeField]
    private int sampleCount = 10;
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private int maxHunger = 100;
    [SerializeField]
    private int maxTiredness = 100;
    [SerializeField]
    private float timePerUpdate;
    [SerializeField]
    private int MinAmountOfHungerToTakePerUpdate;
    [SerializeField]
    private int MaxAmountOfHungerToTakePerUpdate;
    [SerializeField]
    private int MinAmountOfTirednessToTakePerUpdate;
    [SerializeField]
    private int MaxAmountOfTirednessToTakePerUpdate;

    private BoxCollider areaToGo;
    private Rooms currentArea;
    private int hunger;
    private int tiredness;
    private int hungerLevel;
    private int tirednessLevel;
    private string agentLayer = "Agent";
    private int pushes = 0; 
    private bool stopped = false;   
    private bool inArea;
    private float timeToSpendInArea;

    private Rooms.whatCanDo AgentState;

    private GameObject parentAreas;
    private List<Rooms> allAreas = new List<Rooms>();
    private float timePassed;
    


    private void Awake()
    {
        parentAreas = GameObject.Find("Areas");
        GetAllAreas();
        hunger = maxHunger;
        tiredness = maxTiredness;
        RandomizeHungerAndTiredness();
        FindHungerLevel();
        FindTirednessLevel();
        AgentState = FindWhatToDo();
        DoAgentState();
        Vector3 bestSpot = FindMostIsolatedSpot();
        agent.SetDestination(bestSpot);
    }

    private void Update()
    {
        UpdateAgent();
        if(IsAroundDesiredArea() && timeToSpendInArea >= 0){
            CountTimeToSpendInArea();
            
        }else if(timeToSpendInArea <= 0){
            FindHungerLevel();
            FindTirednessLevel();
            AgentState = FindWhatToDo();
            DoAgentState();
            Vector3 bestSpot = FindMostIsolatedSpot();
            agent.SetDestination(bestSpot);
        }

    }
    private void UpdateAgent(){
        timePassed += Time.deltaTime;
        if(timePassed > timePerUpdate){
            timePassed = 0;
            if(currentArea.WhatToDo != Rooms.whatCanDo.Eat)
                hunger -= Random.Range(MinAmountOfHungerToTakePerUpdate, MaxAmountOfHungerToTakePerUpdate);
            if(currentArea.WhatToDo != Rooms.whatCanDo.Rest)
                tiredness -= Random.Range(MinAmountOfTirednessToTakePerUpdate, MaxAmountOfTirednessToTakePerUpdate);
        }
        if(hunger <= 0){
            hunger = 0;
        }
        if (tiredness <= 0){
            tiredness = 0;
        }
    }
    private void CountTimeToSpendInArea(){
        timeToSpendInArea -= Time.deltaTime;
    }

    private Vector3 FindMostIsolatedSpot()
    {
        Vector3 bestPoint = transform.position;
        float bestScore = -1f;

        for (int i = 0; i < sampleCount; i++)
        {

            Vector3 sample = GetRandomPointInArea();

            if (NavMesh.SamplePosition(sample, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                float minDistance = GetDistanceToClosestAgent(hit.position);
                if (minDistance > bestScore)
                {
                    bestScore = minDistance;
                    bestPoint = hit.position;
                }            
            }
        }

        return bestPoint;
    }

    private float GetDistanceToClosestAgent(Vector3 position)
    {
        Collider[] nearbyAgents = Physics.OverlapSphere(position, samplingRadius, LayerMask.GetMask(agentLayer));

        float closest = float.MaxValue;
        foreach (var agent in nearbyAgents)
        {
            float dist = Vector3.Distance(position, agent.transform.position);
            if (dist < closest)
            {
                closest = dist;
            }
        }

        return closest;
    }

    private Vector3 GetRandomPointInArea()
    {
        Bounds bounds = areaToGo.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != gameObject && other.gameObject.layer == LayerMask.NameToLayer("Agent")&& IsInsideRealDesiredArea())
        {
            pushes += 1;
            if (other.gameObject != gameObject && other.gameObject.layer == LayerMask.NameToLayer("Agent") && pushes >= 3)
            {
                if (IsInsideRealDesiredArea())
                {
                    StopMoving();
                    pushes = 0;
                }
            }
        }

    }

    private bool IsInsideRealDesiredArea()
    {
        Vector3 closest = areaToGo.ClosestPoint(transform.position);

        // Ignore Y axis and just check horizontal distance
        Vector2 agentPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 closest2D = new Vector2(closest.x, closest.z);

        float distance = Vector2.Distance(agentPos2D, closest2D);

        return distance < 0.1f; // Tweak threshold if needed
    }
    private bool IsAroundDesiredArea()
    {
        Vector3 closest = currentArea.WholeArea.ClosestPoint(transform.position);

        // Ignore Y axis and just check horizontal distance
        Vector2 agentPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 closest2D = new Vector2(closest.x, closest.z);

        float distance = Vector2.Distance(agentPos2D, closest2D);

        return distance < 0.1f; // Tweak threshold if needed
    }

    void StopMoving()
    {
        agent.isStopped = true;
        agent.ResetPath();
        if(stopped == false){
            Vector3 bestSpot = FindMostIsolatedSpot();
            agent.SetDestination(bestSpot);
            stopped = true;
        }
    }

    private void FindHungerLevel(){
        float hungerPercent = (float)hunger / (float)maxHunger * 100f;
        if (hungerPercent >= 80f)
            hungerLevel = 1;
        else if (hungerPercent >= 50f)
            hungerLevel = 2;
        else if (hungerPercent >= 30f)
            hungerLevel = 3;
        else if (hungerPercent >= 10f)
            hungerLevel = 4;
        else
            hungerLevel = 5;
    }
    
    private void FindTirednessLevel(){
        float tirednessPercent = (float)tiredness / (float)maxTiredness * 100f;

        if (tirednessPercent >= 80f)
            tirednessLevel = 1;
        else if (tirednessPercent >= 50f)
            tirednessLevel = 2;
        else if (tirednessPercent >= 30f)
            tirednessLevel = 3;
        else if (tirednessPercent >= 10f)
            tirednessLevel = 4;
        else
            tirednessLevel = 5;
    }
    private int GetLevelWeight(int level)
    {
        switch (level)
        {
            case 5: return 5;
            case 4: return 9; 
            case 3: return 4;
            case 2: return 2;
            case 1: return 0;
            default: return 0;
        }
    }
    private Rooms.whatCanDo FindWhatToDo(){
        if (hungerLevel == 5 && tirednessLevel != 5)
            return Rooms.whatCanDo.Eat;

        if (tirednessLevel == 5 && hungerLevel != 5)
            return Rooms.whatCanDo.Rest;

        if (hungerLevel == 5 && tirednessLevel == 5)
            return Random.value < 0.5f ? Rooms.whatCanDo.Eat : Rooms.whatCanDo.Rest;

        int eatWeight = GetLevelWeight(hungerLevel);
        int restWeight = GetLevelWeight(tirednessLevel);
        int funWeight = Mathf.Max(10 - (eatWeight + restWeight), 0);

        int total = eatWeight + restWeight + funWeight;
        int roll = Random.Range(0, total);

        if (roll < eatWeight)
            return Rooms.whatCanDo.Eat;
        else if (roll < eatWeight + restWeight)
            return Rooms.whatCanDo.Rest;
        else
            return Rooms.whatCanDo.Fun;
        
    } 
    /// <summary>
    /// Randomizes the Hunger and rest but always something greater than 3/4 of the max because everybody comes in with a different amount of hunger and tiredness.
    /// </summary>
    private void RandomizeHungerAndTiredness(){
        hunger -= Random.Range(0,(maxHunger/4));
        tiredness -= Random.Range(0,(tiredness/4));
    }
    private void GetAllAreas(){
        foreach(Transform child in parentAreas.transform){
            if (child.GetComponent<Rooms>() != null)
            {
                allAreas.Add(child.GetComponent<Rooms>());
            }
        }
    }
    private void DoAgentState(){
        List<Rooms> matchingRooms = new List<Rooms> ();
        foreach (Rooms room in allAreas){
            if(room.WhatToDo == AgentState){
                matchingRooms.Add(room);
            }
        }
        int roomnumber = Random.Range(0, matchingRooms.Count);
        currentArea = matchingRooms[roomnumber];
        GetAreaInfo();
    }
    private void GetAreaInfo(){
        timeToSpendInArea = Random.Range(currentArea.MinTimeToSpendHere, 1+currentArea.MaxTimeToSpendHere);
        int amountofplaces = 0;
        bool areaDecided = false;
        if(currentArea.CanGoToMoreThanOnePlace){
            for(int i = 0; i < currentArea.PlacesHeCanGo.Count ; i++ )
            {
                amountofplaces += 1;
                if(currentArea.MaxPeoplePerPlace[i] > currentArea.CurrentAmountOfPeople && !areaDecided){
                    areaToGo = currentArea.PlacesHeCanGo[i];
                    areaDecided = true;
                }
            }
        }else{
            areaToGo = currentArea.WhereToGo;
            areaDecided = true;
        }
        if(!areaDecided){
            areaToGo = currentArea.PlacesHeCanGo[amountofplaces];
        }
        Vector3 bestSpot = FindMostIsolatedSpot();
        agent.SetDestination(bestSpot);
        CheckArea();
    }
    private void CheckArea(){
        if(currentArea.WhatToDo == Rooms.whatCanDo.Eat){
            hunger = 100;
        }
        if(currentArea.WhatToDo == Rooms.whatCanDo.Rest){
            tiredness = 100;
        }
    }
}