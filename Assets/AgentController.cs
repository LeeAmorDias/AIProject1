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

    private BoxCollider areaToGo;
    private Rooms currentArea;
    private int hunger;
    private int tiredness;
    private int hungerLevel;
    private int tirednessLevel;
    private string agentLayer = "Agent";
    private int pushes = 0; 
    private bool stopped = false;   

    private ActionType AgentState;
    


    void Start()
    {
        hunger = maxHunger;
        tiredness = maxTiredness;
        RandomizeHungerAndTiredness();
        FindHungerLevel();
        FindTirednessLevel();
        AgentState = FindWhatToDo();
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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != gameObject && other.gameObject.layer == LayerMask.NameToLayer("Agent")&& IsInsideDesiredArea())
        {
            pushes += 1;
            if (other.gameObject != gameObject && other.gameObject.layer == LayerMask.NameToLayer("Agent") && pushes >= 10)
            {
                if (IsInsideDesiredArea())
                {
                    StopMoving();
                    pushes = 0;
                }
            }
        }

    }

    bool IsInsideDesiredArea()
    {
        Vector3 closest = areaToGo.ClosestPoint(transform.position);

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
        Debug.Log("Agent stopped: already in target area.");
    }

    private void FindHungerLevel(){
        float hungerPercent = (hunger / maxHunger) * 100f;
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
        float tirednessPercent = (tiredness / maxTiredness) * 100f;

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
        if (level == 5) return 5; // special case, return 5 for 100% action
        switch (level)
        {
            case 4: return 9; 
            case 3: return 4;
            case 2: return 2;
            case 1: return 0;
            default: return 0;
        }
    }
    private ActionType FindWhatToDo(){
        if (hungerLevel == 5 && tirednessLevel != 5)
            return ActionType.Eating;

        if (tirednessLevel == 5 && hungerLevel != 5)
            return ActionType.Resting;

        if (hungerLevel == 5 && tirednessLevel == 5)
            return Random.value < 0.5f ? ActionType.Eating : ActionType.Resting;

        int eatWeight = GetLevelWeight(hungerLevel);
        int restWeight = GetLevelWeight(tirednessLevel);
        int funWeight = Mathf.Max(10 - (eatWeight + restWeight), 0);

        int total = eatWeight + restWeight + funWeight;
        int roll = Random.Range(0, total);

        if (roll < eatWeight)
            return ActionType.Eating;
        else if (roll < eatWeight + restWeight)
            return ActionType.Resting;
        else
            return ActionType.Partying;
        
    } 
    /// <summary>
    /// Randomizes the Hunger and rest but always something greater than 3/4 of the max because everybody comes in with a different amount of hunger and tiredness.
    /// </summary>
    private void RandomizeHungerAndTiredness(){
        hunger -= Random.Range(0,(maxHunger/4));
        tiredness -= Random.Range(0,(tiredness/4));
    }
}