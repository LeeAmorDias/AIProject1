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
}
