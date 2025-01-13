# Maze-Generator
A maze generator with six maze generation algorithms.
1. Randomized depth-first search (DFS)
2. Randomized Kruskal's algorithm (Kruskal)
3. Randomized Prim's algorithm (Prim)
4. Aldous-Broder algorithm (AldousBroder)
5. Wilson's algorithm (Wilson)
6. Aldous-Broder and Wilson's hybrid (AldousBroderWilson)

The Aldous-Broder and Wilson's hybrid combines the two algorithms, 
leveraging their respective strengths to combat their weaknesses. 
It starts by running Aldous-Broder until a third of all cells have been visited, 
and the switches over to Wilson's. 
If the algorithm still produces a uniform spanning tree or not is left as an exercise to the reader.

## Play
[barashiro.github.io/Maze-Generator/](https://barashiro.github.io/Maze-Generator/)

## Video Demo
https://github.com/user-attachments/assets/6866f828-6b0b-4d68-9181-2ad1ce29e90e

