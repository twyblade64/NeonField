# Neon Field

Geometry Wars-esque inspired warping grid made in Unity using ECS. 
The grid is composed by multiple nodes which can be affected by forces. Nodes are also interconected with a spring-like system making them deform and return to their original positions. All the parameters of the grid and the forces are configurable through the grid bootstrap and the explosion components in the GameObjects.

![Example 1](https://i.imgur.com/7RsoXmg.png)
![Example 2](https://i.imgur.com/xwtqrFk.png)
![Alternative grid construction](https://i.imgur.com/kCZIl2M.png)

## Getting Started

Clone the repo and open it in Unity. Unity version must be at least 2018.2. 
The main scene has a sample setup of the grid with a working player ship and enemies. You can tweak the configuration of the grid in the 'Normal Grid Bootstrap' GameObject or deactivate it and activate the 'Recursive Grid Bootstrap' GameObject for a different kind of grid.

### General System Structure
- **CopyForceFromExplosionSystem:** Fills a ForceGenerator component's data with the data of a GameObjects explosion component. This way we can use traditional GameObjects to interact with the grid.
- **LineFromEntityPairSystem:** Defines two points of a line with the position of two entities.
----
- **ForceInfluenceSystem:** Adds forces created by ForceGenerators to Physical entities.
- **MassSpringForceSystem:** Adds forces created by springs to Physical entities.
- **ForceVelocitySystem:** Accelerates Physical entities by their current force.
----
- **VelocityDampSystem:** Decreases the overall energy of the grid.
- **VelocityLimitSystem:** Limits the maximum speed of each entity.
- **VelocityMovementSystem:** Moves the position of the entities by their velocity.
- **FreezePositionSystem:** Limit the position of the entities on a fixed axis.
----
- **ThickLineMeshBuilderSystem:** Builds the mesh of a line with a given thickness.
- **LineRenderSystem:** Renders the line meshes.

## Issues
- Stability issues when there is a high-density grid with a high elasticity.
- ForceGenerators of the player, bullets and some enemies stop working when a lot of enemies appear on screen.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Keijiro for its Firefly example (https://github.com/keijiro/Firefly)