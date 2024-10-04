using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AStarMind : MonoBehaviour
{
    private List<Node> _openedList = new List<Node>();
    private List<Node> _closedList = new List<Node>();

    private int _acummulatedCost = 0;

    private List<LocomotionController.MoveDirection> _path = new List<LocomotionController.MoveDirection>();
    private Node _currentNode;

    public LocomotionController.MoveDirection GetNextMove(Tile currentPos, Tile goal)
    {
        // Antes de calcular el nuevo movimiento, se reinicia la situación, ya que la meta cambia de manera dinámica
        Repath();
        // Tras ello, se ejecuta el algoritmo de búsqueda
        AStarSearch(currentPos, goal);
        // Si no hay camino calculado, devolver una dirección por defecto (puedes ajustarla según la lógica que quieras)
        if (_path.Count == 0)
        {
            Debug.LogWarning("No se encontró un camino válido.");
            return LocomotionController.MoveDirection.None; // Ajusta esto según tus movimientos
        }
        // Para obtener el mejor movimiento, se debe escoger el último elemento del camino, ya que se realiza de meta a inicio
        LocomotionController.MoveDirection nextMove = _path[_path.Count - 1];

        return nextMove;
    } 

    private void Repath()
    {
        // Se limpia el estado para poder calcular un nuevo camino, en base a la nueva posición del objetivo
        _path.Clear();
        _openedList.Clear();
        _closedList.Clear();
        _acummulatedCost = 0;
    }

    private void AStarSearch(Tile currentCell, Tile goal)
    {
        // PRIMERO SE FORMA UN NODO CON LA POSICIÓN ACTUAL
        // Calcular la heurística
        int heuristic = CalculateHeuristic(currentCell, goal);
        // Se crea el nodo con el coste acumulado y la información de la celda
        Node currentNode = new Node(heuristic, _acummulatedCost, IsGoal(currentCell, goal), currentCell, null);
        // Después se añade el nodo a la lista abierta
        _openedList.Add(currentNode);
        // Se ordena la lista utilizando el comparador creado
        _openedList.Sort(Comparator.CompareNodesByF);

        // SE EXPLORA EL PRIMER NODO DE LA LISTA
        Node firstNode = _openedList[0];
        // Una vez expandido, se añade a la lista cerrada y se saca de la lista abierta
        _closedList.Add(currentNode);
        _openedList.Remove(currentNode);

        // CICLO DE BÚSQUEDA HASTA LLEGAR A LA META
        while(!firstNode.isGoal)
        {
            // Función expandir - se obtienen las casillas caminables destino
            Tile[] walkableNeighbours = GridManager.Instance.GetWalkableNeighbours(currentCell);
        }
    }



    // Se calcula la heurística de una casilla
    public int CalculateHeuristic(Tile posTile, Tile goalTile)
    {
        int column = posTile.xTile;
        int row = posTile.zTile;

        int goalColumn = goalTile.xTile;
        int goalRow = goalTile.zTile;

        // Distancia Manhattan
        return (Mathf.Abs(goalColumn - column) + Mathf.Abs(goalRow - row));
    }

    // Se calcula si el nodo es meta o no
    public bool IsGoal(Tile currentPos, Tile goal)
    {
        if (currentPos.xTile == goal.xTile && currentPos.zTile == goal.zTile)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public class Comparator
{
    // Compara dos nodos de la lista, por su f
    public static int CompareNodesByF(Node a, Node b)
    {
        if (a.f < b.f)
        {
            return -1;
        }
        if (a.f == b.f)
        {
            if (a.h < b.h)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
        else
        {
            return 1;
        }
    }
}
