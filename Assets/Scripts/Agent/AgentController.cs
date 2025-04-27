using UnityEngine;
using UnityEngine.AI;
using LibGameAI.DecisionTrees;
using Unity.AI.Navigation;
/// <summary>
/// controls the whole agent: by parts what it does is checks his hunger levels and tirednesse levels then sees what he wants to do if he will want to go eat rest or have fun
/// but in case an explosion happens he will enter in the panick state and he will also panick if a close by agent is panicking in case he actually got hit by the explosion he will die
/// or if he was next to the explosion when it happaned he will be stunned and walk slower.
/// </summary>
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
    private float stunTimer = 2f;
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
    private Collider[] buffer = new Collider[20];
    private Renderer agentRenderer;
    private MaterialPropertyBlock materialPropertyBlock;

    //has the agents hunger stored here
    private int hunger;
    //has the agents  tiredness stored here
    private int tiredness;
    //has the agents  hungerLevel stored here
    private int hungerLevel;
    //has the agents  tirednessLevel stored here
    private int tirednessLevel;
    //has the agents layer stored here
    private string agentLayer = "Agent";
    //stores how many times the agent has been pushed
    private int pushes = 0; 
    private bool stopped = false;
    private bool isStunned = false;
    private bool wasStunned = false;
    private bool isPanicking = false;
    private bool inArea;
    private float timeToSpendInArea;
    private float originalSpeed;

    private readonly float checkCooldown = 0.05f;
    private float nextCheckTime = 0f;

    // The root of the decision tree
    private IDecisionTreeNode root;

    private Rooms.whatCanDo AgentState;

    private AreasController parentAreas;
    private AgentsHandler agentsHandler;
    private float timePassed;
    private NavMeshSurface navMeshSurface; // Reference to NavMeshSurface

    private bool personAddedToArea;
    

    /// <summary>
    /// gets the parent of all areas, sets the hunger and tiredness to its max then takes a random amount of both because everyone will have a different amount of each 
    /// then finds the hunger/tiredness level and the updates the agents state then gets the area the agent should go and then sends him there.
    /// </summary>
    private void Awake()
    {
        navMeshSurface = FindFirstObjectByType<NavMeshSurface>();
        parentAreas = FindFirstObjectByType<AreasController>();
        agentsHandler = FindFirstObjectByType<AgentsHandler>();
        agentRenderer = GetComponent<Renderer>();
        materialPropertyBlock = new MaterialPropertyBlock();
        originalSpeed = agent.speed;
        hunger = maxHunger;
        tiredness = maxTiredness;
        RandomizeHungerAndTiredness();
        

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
        ActionNode actionNode = root.MakeDecision() as ActionNode;
        actionNode.Execute();
        UpdateAgent();

    }
    /// <summary>
    /// The agent will do what he desires the most having in to an account his hunger and tiredness levels.
    /// </summary>
    private void NormalAction(){
        personAddedToArea = false;
        if(currentArea != null)
            currentArea.TakeAmountOfPeople();
        SetEmergencyFloorWalkability(false);
        pushes = 0;
        hungerLevel = FindLevel(hunger, maxHunger);
        tirednessLevel = FindLevel(tiredness, maxTiredness);
        AgentState = FindWhatToDo();
        GetAreaInfo();
        Vector3 bestSpot = FindMostIsolatedSpot();
        agent.SetDestination(bestSpot);
    }
    /// <summary>
    /// the agent will enter his panic state and try to run away from the festival
    /// </summary>
    private void PanicAction(){
        if(AgentState!=Rooms.whatCanDo.Escape){
            SetEmergencyFloorWalkability(true);
            AgentState = Rooms.whatCanDo.Escape;
            agent.SetDestination(BestExit());
        }
        if(agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            agent.SetDestination(BestExit());
        }
        if(IsInsideRealDesiredArea()){
            Destroy(gameObject);
        }
        
    }

    /// <summary>
    /// if the agent is in the area and if he is it will count down the time else it will wait for him to get there to start counting
    /// </summary>
    private void WaitAction(){
        if (IsAroundDesiredArea()){
            timeToSpendInArea -= Time.deltaTime;
            if(!personAddedToArea){
                currentArea.AddAmountOfPeople();
                personAddedToArea = true;
            }
        }
    }
    /// <summary>
    /// checks if enough time has passed to take out hunger and tiredness from the agent but of course if the agent is resting he will not be taken out any tiredness.
    /// also checks if it is stunned and if he is it will make him stop for the time in "stunTimer" and checks if he was stunned or wasnt and if is panicking to set his new speed
    /// </summary>
    private void UpdateAgent(){
        timePassed += Time.deltaTime;
        if(isStunned){
            agent.speed = 0f;
            stunTimer -= Time.deltaTime;
        }
        if(isStunned && stunTimer < 0f){
            isStunned = false;
            wasStunned = true;
        }
        if(wasStunned)
            agent.speed = originalSpeed * 0.5f;
        
        if(!wasStunned && isPanicking)
            agent.speed = originalSpeed * 2f;
        
        if(timePassed > timePerUpdate){
            timePassed = 0;
            if(currentArea != null)
            {
                if(currentArea.WhatToDo != Rooms.whatCanDo.Eat)
                    hunger -= Random.Range(minAmountOfHungerToTakePerUpdate, maxAmountOfHungerToTakePerUpdate);
                else 
                    hunger = 100;
                if(currentArea.WhatToDo != Rooms.whatCanDo.Rest)
                    tiredness -= Random.Range(minAmountOfTirednessToTakePerUpdate, maxAmountOfTirednessToTakePerUpdate);
                else 
                    tiredness = 100;
            } 
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
        return timeToSpendInArea > 0;
    }
    /// <summary>
    /// this starts by checking if the agent was stunned and isnt in the panic state yet if that is the case he will 
    /// enter the panic state, then will check every "checkCooldown" if he was near the explosion or was near a panicking agent and if that is the case he will enter the~
    /// panic state
    /// </summary>
    /// <returns></returns>
    private bool IsPanicking()
    {
        
        if (wasStunned && !isPanicking)
        {
            isPanicking = true;
            SetPanicColor();
        }
        if(Time.time >= nextCheckTime)
        {
            SpottedExplosion();
            WasNearPanickingAgent();
            nextCheckTime = Time.time + checkCooldown;
        }
        return isPanicking;
    }
    /// <summary>
    /// if he sees a panicking agent and isnt panicking yet he will enter the panick state
    /// </summary>
    private void WasNearPanickingAgent()
    {
        if(!isPanicking)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, samplingRadius, buffer, LayerMask.GetMask(agentLayer));
            for (int i = 0; i < count; i++)
            {
                Collider agent = buffer[i];
                if(agent.GetComponent<AgentController>().isPanicking)
                {
                    isPanicking = true;
                    SetPanicColor();
                }
            }
        }
    }
    /// <summary>
    /// if he sees a explosion and isnt panicking yet he will enter the panick state
    /// </summary>
    private void SpottedExplosion()
    {
        if(!isPanicking)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, samplingRadius, buffer,LayerMask.GetMask("Explosion"));
            if(count>0){
                isPanicking = true;
                SetPanicColor();
            }
        }
    }

    /// <summary>
    /// tries to find the most isolated spot around the area it was sent. 
    /// This code had some AI help to be developed but the ideia was 100% ours.
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
    /// This code had AI help to be developed but the ideia was 100% ours.
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
        return new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.center.y, Random.Range(bounds.min.z, bounds.max.z));
    }
    /// <summary>
    /// checks for collisions inside the area he intends to try to evade pushing around the area so in case he is pushing to much 
    /// he just stops and stays in that spot of the area since that area is already where he wanted to go.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        //checks if is being pushed by another player and in case he is being pushed too much he will just stop
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

        //If inside explosion, die
        if (other.gameObject != gameObject && other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            Destroy(gameObject);
            agentsHandler.AddDeathToCounter();
        }

        //If inside shockwave, stun
        if (other.gameObject != gameObject && other.gameObject.layer == LayerMask.NameToLayer("Shockwave"))
        {
            isStunned = true;
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
    /// <summary>
    /// sees what is the best exit to go to, so the agent wont get caught by the explosion and if he is trapped by explosions he will just stop
    /// </summary>
    /// <returns></returns>
    private Vector3 BestExit()
    {
        currentArea = parentAreas.GiveFastestValidPathToRoom(AgentState, agent);
        //if no suitable exits are found only thing to do is stand there
        //contemplating their own mortality
        if(currentArea == null)
        {
            return transform.position;
        }
        areaToGo = currentArea.WhereToGo;
        return GetRandomPointInArea();
    }

    /// <summary>
    /// changes the cost of the floor the agent shouldnt walk in a normal cenario so if it recieves false the floor wont be used by the agent if it recieves true it will 
    /// </summary>
    /// <param name="isWalkable"></param>
    private void SetEmergencyFloorWalkability(bool isWalkable)
    {
        if (isWalkable)
        {
            agent.SetAreaCost(NavMesh.GetAreaFromName("Walkable"), 1f);
            agent.SetAreaCost(NavMesh.GetAreaFromName("Emergency"), 1f);
        }
        else
        {
            agent.SetAreaCost(NavMesh.GetAreaFromName("Walkable"), 1f);
            agent.SetAreaCost(NavMesh.GetAreaFromName("Emergency"), 100f);
        }
    }
    /// <summary>
    /// when he starts panicking his color will turn red so the "game" has some feedback
    /// </summary>
    private void SetPanicColor()
    {
        agentRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor("_Color", Color.red);
        agentRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, samplingRadius);
    }
}