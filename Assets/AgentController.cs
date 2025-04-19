using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    [SerializeField]
    private BoxCollider area;
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
    private int hunger;
    private int tiredness;
    private int hungerLevel;
    private int tirednessLevel;
    private string agentLayer = "Agent";
    private int pushes = 0; 
    private bool stopped = false;   


    void Start()
    {
        Vector3 bestSpot = FindMostIsolatedSpot();
        agent.SetDestination(bestSpot);
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
        Bounds bounds = area.bounds;
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
        Vector3 closest = area.ClosestPoint(transform.position);

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

    /// <summary>
    /// Randomizes the Hunger and rest but always something greater than 3/4 of the max because everybody comes in with a different amount of hunger and tiredness.
    /// </summary>
    private void RandomizeHungerAndTiredness(){
        hunger -= Random.Range(0,(maxHunger/4));
        tiredness -= Random.Range(0,(tiredness/4));
    }
}