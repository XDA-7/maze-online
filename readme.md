# Maze Online
A basic maze generator written in F# and displayed through HTML canvas

# How to run
- Have a fairly recent version of dotnet core installed (at least v2)
- Execute 'dotnet run' in the root directory
- In the browser, navigate to 'localhost:5000/index.html'
- Generating the maze can take up to about a minute
- Your position will be indicated by the blue square, use the arrow keys to move about the maze
- A red destination square can be found somewhere on the maze but is extremely difficult for a human to reach, though a pathfinder would be able to do it easily
- As a bonus, navigating to 'localhost:5000/delaunay.html' will display a random delaunay triangulation
  
# Deployment
This application should be able to run on any server with dotnet core installed with a few tweaks
- The generated mazes stored as local files would need to be moved to a persistant storage (eg Redis)
- The user session data would need to be moved out of working memory to a persistant storage
- Ideally there would be rewrite rules implmented so that the users can use the base url without the '/index.html' extension

# Theory
Maze generation is closely related to graph theory. You can see this by imagining the maze as a grid of square tiles with infinitely thin walls between some of them. The tiles represent nodes on a graph, and two adjacent tiles with no wall between them can be considered to have an edge between their respective nodes on the graph.

In this light, finding a configuration for the maze in which all tiles are accessible from any other tile but not maximally connected (ie a maze with no walls) is similar to finding a spanning tree for the graph.
The most well known algorithms for finding a spanning tree are Kruskal's and Primm's, but these are used to find minimally spanning trees, which is not an important trait for maze generation. Furthermore, the straightforward way that these algorithms select the next possible node to add to the tree leads to mazes with long corridors and less branching.

Wilson's algorithm makes use of random walks to find a random spanning tree with uniform distribution, which makes it highly suitable for the task of maze generation.