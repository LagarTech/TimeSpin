using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    // Estas variables almacenan las coordenadas de la casilla, tomando el escenario como un tablero
    public int xTile;
    public int zTile;
    // Coordenadas del centro de la casilla, para poder que el agente la pueda tomar como objetivo
    public Vector3 tileCenterPos;
    // Se indica si es caminable o no
    public bool isWalkable;

    public Tile(int xTile, int zTile, Vector3 tileCenterPos)
    {
        this.xTile = xTile;
        this.zTile = zTile;
        this.tileCenterPos = tileCenterPos;
        this.isWalkable = true;
    }

    public void MakeNonWalkable()
    {
        isWalkable = false;
    }
}
