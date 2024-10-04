using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Node
{
    public int row;
    public int column;
    public float g;
    public float h;
    public float f;
    public bool isGoal;
    public Node previousNode;
    public Tile nodeInfo;

    public Node(float h_in, float g_in, bool isGoal_in, Tile info, Node prev)
    {
        this.h = h_in;
        this.g = g_in;
        this.f = h + g;
        this.isGoal = isGoal_in;
        this.nodeInfo = info;
        this.row = info.xTile;
        this.column = info.zTile;
        this.previousNode = prev;
    }
}
