using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Assets.Scripts;
using LibGameAI.DecisionTrees;

public class AgentController : MonoBehaviour
{

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
    private int minAmountOfHungerToTakePerUpdate;
    [SerializeField]
    private int maxAmountOfHungerToTakePerUpdate;
    [SerializeField]
    private int minAmountOfTirednessToTakePerUpdate;
    [SerializeField]
    private int maxAmountOfTirednessToTakePerUpdate;
    [SerializeField]
    private int maxHungerToRemoveAtStart;
    [SerializeField]
    private int maxTirednessToRemoveAtStart;

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
    // The root of the decision tree
    private IDecisionTreeNode root;

    private Rooms.whatCanDo AgentState;

    private AreasController parentAreas;
    private float timePassed;
    

    /// <summary>
    /// gets the parent of all areas, sets the hunger and tiredness to its max then takes a random amount of both because everyone will have a different amount of each 
    /// then finds the hunger/tiredness level and the updates the agents state then gets the area the agent should go and then sends him there.
    /// </summary>
    private void Awake()
    {
        parentAreas = GameObject.FindFirstObjectByType<AreasController>();
        hunger = maxHunger;
        tiredness = maxTiredness;
        RandomizeHungerAndTiredness();
        /*
        hungerLevel = FindLevel(hunger, maxHunger);
        tirednessLevel = FindLevel(tiredness, maxTiredness);
        AgentState = FindWhatToDo();
        GetAreaInfo();
        Vector3 bestSpot = FindMostIsolatedSpot();
        agent.SetDestination(bestSpot);
        */
        IDecisionTreeNode panicAction = new ActionNode(PanicAction);
        IDecisionTreeNode waitAction = new ActionNode(WaitAction);
        IDecisionTreeNode normalAction = new ActionNode(NormalAction);

        root = new DecisionNode(IsPanicking, panicAction, new DecisionNode(IsStillWaitingInArea, waitAction, normalAction));
    }

    /// <summary>
    /// updates the agent then checks if the agent is where he wants to be and hasnt passed the time he intended to pass there and if thats the case 
    /// it will just count down the time until he spent the time he wants on the area or else it will update the hunger/tiredness level and then updates the agents state 
    /// then gets the area the agent should go and then sends him there.
    /// </summary>
    private void Update()
    {
        UpdateAgent();

        ActionNode actionNode = root.MakeDecision() as ActionNode;
        actionNode.Execute();
        /*
        if(IsAroundDesiredArea() && timeToSpendInArea >= 0){
            WaitAction();
            
        }else if(timeToSpendInArea <= 0){
            NormalAction();
        }
        */

    }
    private void NormalAction(){
        pushes = 0;
        hungerLevel = FindLevel(hunger, maxHunger);
        tirednessLevel = FindLevel(tiredness, maxTiredness);
        AgentState = FindWhatToDo();
        GetAreaInfo();
        Vector3 bestSpot = FindMostIsolatedSpot();
        agent.SetDestination(bestSpot);
    }
    private void PanicAction(){

    }
    private void WaitAction(){
        if (IsAroundDesiredArea())
            timeToSpendInArea -= Time.deltaTime;
    }
    /// <summary>
    /// checks if enough time has passed to take out hunger and tiredness from the agent but of course if the agent is resting he will not be taken out any tiredness.
    /// </summary>
    private void UpdateAgent(){
        timePassed += Time.deltaTime;
        if(timePassed > timePerUpdate){
            timePassed = 0;
            if(currentArea.WhatToDo != Rooms.whatCanDo.Eat)
                hunger -= Random.Range(minAmountOfHungerToTakePerUpdate, maxAmountOfHungerToTakePerUpdate);
            else 
                hunger = 100;
            if(currentArea.WhatToDo != Rooms.whatCanDo.Rest)
                tiredness -= Random.Range(minAmountOfTirednessToTakePerUpdate, maxAmountOfTirednessToTakePerUpdate);
            else 
                tiredness = 100;
        }
        if(hunger <= 0){
            hunger = 0;
        }
        if (tiredness <= 0){
            tiredness = 0;
        }
    }
    /// <summary>
    /// counts down the time he intends to spend per area.
    /// </summary>
    private bool IsStillWaitingInArea(){
        return timeToSpendInArea >= 0;
    }
    private bool IsPanicking()
    {
        return AgentState == Rooms.whatCanDo.Escape;
    }
    /// <summary>
    /// tries to find the most isolated spot around the area it was sent. 
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// Checks the position the agent is set to go and sees how close that position is to a agent
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
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
    /// <summary>
    /// gets a random point in the area the agent is set to go
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomPointInArea()
    {
        Bounds bounds = areaToGo.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
    /// <summary>
    /// checks for collisions inside the area he intends to try to evade pushing around the area so in case he is pushing to much 
    /// he just stops and stays in that spot of the area since that area is already where he wanted to go.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != gameObject && other.gameObject.layer == LayerMask.NameToLayer("Agent")&&IsInsideRealDesiredArea())
        {
            pushes += 1;
            if (other.gameObject != gameObject && other.gameObject.layer == LayerMask.NameToLayer("Agent") && pushes >= 10)
            {
                if (IsInsideRealDesiredArea())
                {
                    StopMoving();
                    pushes = 0;
                }
            }
        }

    }
    /// <summary>
    /// Checks if the agent is in the real area he want to go 
    /// For example in the stage he will always try to go to the front that would be the real area and the area will only be all the stage.
    /// </summary>
    /// <returns></returns>
    private bool IsInsideRealDesiredArea()
    {
        if (areaToGo == null)
            return false;
        Vector3 closest = areaToGo.ClosestPoint(transform.position);

        // Ignore Y axis and just check horizontal distance
        Vector2 agentPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 closest2D = new Vector2(closest.x, closest.z);

        float distance = Vector2.Distance(agentPos2D, closest2D);

        return distance < 0.1f; // Tweak threshold if needed
    }
    /// <summary>
    /// Checks if the agent is in the area he wants to go 
    /// </summary>
    /// <returns></returns>
    private bool IsAroundDesiredArea()
    {
        if (currentArea == null)
            return false;
        Vector3 closest = currentArea.WholeArea.ClosestPoint(transform.position);

        // Ignore Y axis and just check horizontal distance
        Vector2 agentPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 closest2D = new Vector2(closest.x, closest.z);

        float distance = Vector2.Distance(agentPos2D, closest2D);

        return distance < 0.1f; // Tweak threshold if needed
    }
    /// <summary>
    /// stops the movement of the agent but if it is his first time stopping by force in that area he will try to find a better spot
    /// because there are too many pushes where he is 
    /// </summary>
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
    /// <summary>
    /// used to find the Level of hunger/tiredness in this case the level represents how much he wants to do that based on how tired/hungry he is,
    /// 1 being he doesnt want to do it and 5 meaning he needs to do it
    /// </summary>
    /// <param name="value"></param>
    /// <param name="MaxValue"></param>
    /// <returns></returns>
    private int FindLevel(int value, int MaxValue){
        float Percent = (float)value / (float)MaxValue * 100f;
        if (Percent >= 80f)
            return 1;
        else if (Percent >= 50f)
            return 2;
        else if (Percent >= 30f)
            return 3;
        else if (Percent >= 10f)
            return 4;
        else
            return 5;
    }
    /// <summary>
    /// gets the weigh of each level of hunger/tiredness to facilitate and have better calculations on what to do next
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private int GetLevelWeight(int level)
    {
        switch (level)
        {
            case 4: return 9; 
            case 3: return 5;
            case 2: return 2;
            case 1: return 0;
            default: return 0;
        }
    }
    /// <summary>
    /// Based on the hunger level and tiredness level it decides on what to do, but still being random, but this helps to find out what he would want to do the most 
    /// based on his hunger and tiredness levels.
    /// </summary>
    /// <returns></returns>()
    private Rooms.whatCanDo FindWhatToDo(){
        if (hungerLevel == 5 && tirednessLevel != 5)
            return Rooms.whatCanDo.Eat;

        if (tirednessLevel == 5 && hungerLevel != 5)
            return Rooms.whatCanDo.Rest;

        if (hungerLevel == 5 && tirednessLevel == 5)
            return Random.value < 0.5f ? Rooms.whatCanDo.Eat : Rooms.whatCanDo.Rest;

        int eatWeight = GetLevelWeight(hungerLevel);
        int restWeight = GetLevelWeight(tirednessLevel);
        int funWeight = Mathf.Max(15 - (eatWeight + restWeight), 0);

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
        hunger -= Random.Range(0,(maxHungerToRemoveAtStart));
        tiredness -= Random.Range(0,(maxTirednessToRemoveAtStart));
    }
    /// <summary>
    /// First this gets the Area he should go by telling the Parent of all areas im on this state give me a room to go and he gives u a room for your state
    /// then this stores it so we have full access to the current room and we verify if that room has more than one place to go if no it just goes to a 
    /// sets the area to go to the only area the room has else it would check every area and see what is full in case none are full it will send u to the first place
    /// if the first place is full it will send u to the second place and so on and if all of them are full it just sends u to the last one.
    /// </summary>
    private void GetAreaInfo(){
        currentArea = parentAreas.CheckRoomsWithMatchingStateAndGiveRoom(AgentState);
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
    }
}