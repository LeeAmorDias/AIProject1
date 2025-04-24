# AI Project 1

Lee Dias a22405765

João Fernandes a22304583

## WHAT WAS DONE BY EACH MEMBER

Lee Dias: Created the map, the agents handler script, the script that manages each area,
and the base of the agents script (everything done except the panic behavior),
which was implemented using Unity's NavMesh , completed 50% of the README.

João Fernandes:

## INTRODUCTION
In this project, we were tasked with creating an AI for people at a festival.
After the festival has been running for some time, an explosion occurs that kills
all the people in that area and causes panic among those nearby.
This panic then spreads to people who did not witness the explosion directly.

## METHODOLOGY
The simulation was done in 3D. The people use kinematic movement.
The people's script works as follows: 

So it was made with a decision tree, and the first question is:

<p>Is the person in panic?

If yes, they will enter the [Panic State](#panic-state)
<br>If no, it will check whether the time they wanted to spend in the current area has passed.

If no, they will be in [Waiting State](#waiting-state).
<br>If yes, they will [Go To Next Area State](#go-to-next-are-state). 
</p>

### Panic State

### Waiting State

Waits until he has passed his desired time in the area he is at.
 
### Go To Next Area State
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



<p>funWeight = How much he wants to go have fun
<br>eatWeight = How much he wants to go eat
<br>restWeight = How much he wants to rest
</p>

funWeight = 15 - (eatWeight + restWeight)

Total = funWeight + eatWeight + restWeight

The chances will be something like funWeight/Total , eatWeight/Total , restWeight/Total
<p> A example to this is level 3 hunger level 4 Tiredness the chances would be, 
5/15(Hunger) , 9/15(Tiredness) , 1/15(Fun)

Next, after deciding what they want to do, the person goes to the location and 
then determines how long they want to stay there.


![image](files://C:/Users/leeam/AI Project 1/DecisionTree.png)


## RESULTS AND DISCUSSION

## CONCLUSIONS

## REFERENCES
https://www.youtube.com/watch?v=CHV1ymlw-P8
<br>https://www.youtube.com/watch?v=FkLJ45Pt-mY
<br>https://www.youtube.com/watch?v=u2EQtrdgfNs
<br>https://www.youtube.com/watch?v=igrHPCTFZXM
