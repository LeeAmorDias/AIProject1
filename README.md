# AI Project 1

Lee Dias a22405765

João Fernandes a22304583

## WHAT WAS DONE BY EACH MEMBER

Lee Dias: Created the map, the agents handler script, the script that manages each area,
and the base of the agents script (everything done except the panic behavior),
which was implemented using Unity's NavMesh , completed 50% of the README.

João Fernandes: Created Explosion/Fire prefab and Handler as well as panic behavior
namely, finding exits, panic contagion, avoiding hazards, etc.

## INTRODUCTION

In this project, we were tasked with creating AI behavior for people at a festival.
After the festival has been running for some time, an explosion occurs that kills
all the people in that area and causes panic among those nearby.
This panic then spreads to people who did not witness the explosion directly.

We took the teachers recommendation and consulted a few academic papers on
the subject, namely:

* A Multi-agent Model for Panic Behavior in Crowds
(R.S. Fran¸ca, M.G.B. Marietto, M.B. Steinberger, 2009)
* Crowd Panic Behavior Simulation Using Multi-Agent Modeling
(Dumitrescu, C., Radu, V., Gheorghe, R., Tăbîrcă, A.-I., Ștefan, M.-C., & Manea, L., 2024)
* An agent-based simulation system for concert venue crowd evacuation
modeling in the presence of a fire disaster
(Wagner & Agrawal, 2014)

However I must admit we took very little from these texts as far as actual ideas,
techniques or algorithms are concerned. The presented models were either too abstract
or following an approach that was not aligned with what we decided to do in the end.
We did however learn a little about what other research has been made in this field
and that what we ended up implementing is actually quite crude in comparison, but
we still did our best.

In conclusion we ended up sticking mostly to the behavior requested in the paper,
following the techniques taught in class and a few questions to ChatGPT when we
were stuck or wanted an opinion on ways to implement certain features.

## METHODOLOGY

The simulation was done in 3D. The people use kinematic movement.
The agent's script works as follows:

We implemented a simple decision tree using the base given in class, with the
following nodes:

Decision Node: Is the person in panic?

If true, Action Node: [Panic Behavior](#panic-behavior)
If false, Decision Node: Is the person ready to go to another area?

If true, Action Node: [Go To Next Area](#go-to-next-area).
If false, Action Node: [Waiting](#waiting).

### Panic Behavior

A person in panic will look for and go to the closest exit that isn't blocked by
a fire, if a path to an exit is initially valid but subsequently gets fully
blocked by a fire the person will look for a new exit. If none are found the
person will simply stand there and await their inevitable demise.

When does panic occur?
If a person is caught by an explosions' shockwave they get stunned for a while,
when they regain their movement they will be panicked.
If a person walks close enough to a fire or another person in panic they too
will become panicked.

### Waiting

Waits until he has passed his desired time in the area he is at.

### Go To Next Area

| Level   | Hunger/Tiredness |
| -------- | ------- |
| 1 | 80% or more (from total)  |
| 2 | 50% or more (from total)  |
| 3 | 30% or more (from total)  |
| 4 | 10% or more (from total)  |
| 5 | 0% or more (from total)   |

After checking the hunger and tiredness levels, the player evaluates how much desire they
have to perform the action based on those levels.
At level 5, a system triggers the action immediately.
If both hunger and tiredness are at level 5, it performs a 50/50 decision.
In all other cases, it works as follows:

| Level   | Weigh |
| -------- | ------- |
| 1 | 0  |
| 2 | 2  |
| 3 | 5  |
| 4 | 9  |

The system works as follows for calculating the desire to go have fun:
The calculation uses 15 as the maximum funWeight, because after extensive testing with different values,
we found that this number best represents a person's willingness to have fun,
depending on their hunger and tiredness levels.

funWeight = How much he wants to go have fun  
eatWeight = How much he wants to go eat  
restWeight = How much he wants to rest  

funWeight = 15 - (eatWeight + restWeight)

Total = funWeight + eatWeight + restWeight

The chances will be something like funWeight/Total , eatWeight/Total , restWeight/Total  
A example to this is level 3 hunger level 4 Tiredness the chances would be,
5/15(Hunger) , 9/15(Tiredness) , 1/15(Fun)

Next, after deciding what they want to do, the person goes to the location and
then determines how long they want to stay there.

![image](./DecisionTree.png)

## RESULTS AND DISCUSSION

We finished programming the final bit of behavior quite close to the delivery date
so we didn't have much time to test the simulation with other parameters.

We noticed early that a good amount of randomization was important to make the simulation
feel more real. Even though the agents all start in the same spot after a few seconds
we already see them being all spread out doing different activities.

We have a little quirk with our method that has agents find isolated spots to stand in
in green areas, the spot they pick doesn't get updated if someone else reaches it first.
But we like to think that this is simply people trying to make friends, it is a concert
after all.

Finally because our NavMesh has a lot of sharp corners it is common for agents to
get a little stuck in these especially if there's a lot of agents taking the same
path like is the case in evacuation scenarios, thankfully with our settings it never
lead to a death because the propagate so much slower than the agents move.

## CONCLUSIONS

In conclusion we believe we implemented a passable simulation of crowd panic, there's
lots of other methods or techniques we could have used and additional behavior we could
have implemented like a more realistic spreading of fire and it's effects like mentioned
in [7] where even smoke production plays a role in the agents decisions, perhaps a more
nuanced approach to panic propagation where agents don't immediately react to the danger
present and either choose to ignore it or go investigate for themselves in a system
more closely resembling what [5] describes, and so on.

But still, we have concert goers that are well fed, and neither bored nor tired,
and when disaster strikes they display some sense of self preservation.

## REFERENCES

[1] <https://www.youtube.com/watch?v=CHV1ymlw-P8>  
[2] <https://www.youtube.com/watch?v=FkLJ45Pt-mY>  
[3] <https://www.youtube.com/watch?v=u2EQtrdgfNs>  
[4] <https://www.youtube.com/watch?v=igrHPCTFZXM>  
[5] França, R.D., Marietto, M.D., Margarethe, & Steinberger, B. (2009). A Multi-agent Model for Panic Behavior in Crowds.  
[6] Dumitrescu, C., Radu, V., Gheorghe, R., Tăbîrcă, A.-I., Ștefan, M.-C., & Manea, L. (2024). Crowd Panic Behavior Simulation Using Multi-Agent Modeling. Electronics, 13(18), 3622. <https://doi.org/10.3390/electronics13183622>  
[7] Wagner, N., Agrawal, V. (2014) An agent-based simulation system for concert venue crowd evacuation modeling in the presence of a fire disaster. <https://doi.org/10.1016/j.eswa.2013.10.013>  
[8] in the method GetDistanceToClosestAgent() AI was used to help on the code but the ideia was ours.  
[9] in the method FindMostIsolatedSpot() AI was used to help on the code but the ideia was ours.  
[10] we also used AI to help us debug and fix some performance issues we were having. Basically explaining how to make effective use of the Profiler.
