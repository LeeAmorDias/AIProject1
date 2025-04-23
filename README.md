# AI Project 1

Lee Dias a22405765

João Fernandes a22304583

## O QUE FOI FEITO POR CADA MEMBRO

Lee Dias: Fez a criação do mapa, o script do agents handler, o script que gere cada area,
a base do script dos agents (no caso tudo feito mas sem a parte do panico) 
que foi feito com a ajuda do navmesh do unity, e 50% do README.

João Fernandes:

## INTRODUÇÃO
Neste trabalho foi nos proposto a criação de um AI para Pessoas em um festival,
em que passado um tempo desse festival estar a decorrer acontece uma explosão
que mata todas as pessoas nessa area e causa panico a toda as pessoas a volta
que causa com que as pessoas fugam e o panico despois espalha para as pessoas
que não viram a explosão.

## METODOLOGIA
A simulação foi feita em 3D, as pessoas utilizam um movimento cinematico, 
o script das pessoas funciona da seguinte forma primeiro verifica o nivel de fome e cansaço.
| Nivel   | Cansaço |
| -------- | ------- |
| 1 | 80% ou mais do maximo  |
| 2 | 50% ou mais do maximo  |
| 3 | 30% ou mais do maximo  | 
| 4 | 10% ou mais do maximo  |
| 5 | 0% ou mais do maximo   |

Depois de ver o nivel de fome e cansaço o player ve a quantidade de vontade que 
ele vai ter de fazer essa ação considerado o nivel.
 No nivel 5 é feito um sistema que faz imediatamente essa função e caso ambas a fome
e o nivel de cansaço esteja no 5 faz um 50/50.
Mas nos restos dos casos funciona da seguinte forma 

| Nivel   | Vontade |
| -------- | ------- |
| 1 | 0  |
| 2 | 2  |
| 3 | 5  |
| 4 | 9  | 

O sistema funciona da seguinte forma para o calculo de a vontade de ele ir se divertir,
a conta é feita com um 15 sendo o maximo do DivertirV porque depois de muitos testes
sobre valores, achamos que esses valor representaria melhor o que seria a vontade de uma pessoa
de se ir divertir consoante os seus niveis de fome e cansaço.

<p>DivertirV = Vontade de divertir
<br>FomeV = Vontade de ir comer
<br>CansaçoV = Vontade de descansar
</p>

DivertirV = 15 - (FomeV + CansaçoV)

Total = DivertirV + FomeV + CansaçoV

E depois as chances ficam algo como DivertirV/Total , FomeV/Total , CansaçoV/Total
<p> Um exemplo seria nivel 3 de fome nivel 4 de cansaço as chances seriam,
5/15(Fome) , 9/15(Cansaço) , 1/15(Divertir)

Em seguida depois de descobrir o que quer fazer a pessoa vai para o sitio e depois determina a quantidade
de tempo que quer ficar no sitio onde esta e passado esse tempo repete o ciclo todo para decidir o que ira 
fazer a seguir.



## RESULTADOS E DISCUSSÃO

## CONCLUSÕES

## REFERÊNCIAS 
