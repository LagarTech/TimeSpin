using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class AStarMind : MonoBehaviour
{
    public LocomotionController.MoveDirection GetNextMove(Tile currentPos, Tile goal)
    {
        return LocomotionController.MoveDirection.None;
    } 
}
