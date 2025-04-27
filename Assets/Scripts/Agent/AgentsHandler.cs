using UnityEngine;
using NaughtyAttributes;

public class AgentsHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject agent;
    [SerializeField]
    private Transform spawnPosition;
    [SerializeField]
    private int amountOfAgentsToSpawn;
    [SerializeField]
    private bool spawnAllAgentsAtOnce;
    [SerializeField, ShowIf(nameof(ShouldShowField)) ]
    private float timeToSpawnAgent;

    private float timePassed;
    private int agentsSpawned = 0;
    private int deathCounter = 0;

    private bool ShouldShowField() => !spawnAllAgentsAtOnce;
    void Awake(){
        timePassed = timeToSpawnAgent;
    }
    // Update is called once per frame
    /// <summary>
    /// this updates will starts by checking the amount of agents that have spawned to see if it should spawn more
    /// then it checks if it should spawnallagents at once or per time and then if it is by time it will spawn 1 once every "timeToSpawnAgent" has passed
    /// </summary>
    void Update()
    {
        if(agentsSpawned < amountOfAgentsToSpawn){
            if(spawnAllAgentsAtOnce){
                Instantiate(agent, spawnPosition.position, spawnPosition.rotation, transform);
                agentsSpawned += 1;
            }else{
                timePassed += Time.deltaTime;
                if (timePassed > timeToSpawnAgent ){
                        Instantiate(agent, spawnPosition.position, spawnPosition.rotation, transform);
                        timePassed = 0;
                        agentsSpawned += 1;
                }
            }
        } 
    }
    /// <summary>
    /// adds 1 to the death counter
    /// </summary>
    public void AddDeathToCounter(){
        deathCounter += 1;
    }
    public int GetDeathCounter(){
        return deathCounter;
    }
}
