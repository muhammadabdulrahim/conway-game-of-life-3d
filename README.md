# Conway's Game of Life 3D

![3D Cube View][thumbnail]

A Unity implementation of [Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life) in 3D space. Built from my [2D implementation](https://github.com/muhammadabdulrahim/conway-game-of-life-2d).

Includes the ability to edit the grid width, height, and depth, as well as simulation settings, such as auto-progression, speed of progression, and general live/die rulesets for the individual sphere objects in the cube. A single prefab is used for the spheres, named a Conway Sphere. Differences in coloring is caused by swapping out dead/living materials on the prefab. The larger your cube, the more intensive the simulation, so be careful regarding performance.

[thumbnail]: https://raw.githubusercontent.com/muhammadabdulrahim/conway-game-of-life-3d/master/thumbnail.PNG
