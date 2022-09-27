using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    NORTH,
    SOUTH,  
    WEST,
    EAST
}

[System.Serializable]
public class Node<T>
{
    public Node<T> Parent = null;
    public T Data;
    public Vector2 Position;
    public List<Edge<T>> Edges = new List<Edge<T>>();
    public float GScore;
    public float HScore;
    public float FScore;

    /// <summary>
    /// Gets the edge that has a target in the given direction
    /// </summary>
    /// <param name="direction">The direction that the edge is pointing towrds</param>
    /// <param name="edge">The edge that was found</param>
    /// <returns>Fals if the edge couldn't be found</returns>
    public bool GetEdgeForDirection(Direction direction, out Edge<T> edge)
    {
        edge = Edges.Find(item => item.DirectionFromParent == direction);

        return !edge.Equals(default(Edge<T>));
    }
}

public struct Edge<T>
{
    public Node<T> Target;
    public Direction DirectionFromParent;
    public float Cost;
}

/// <summary>
/// Class for AI utility function like pathfinding, 
/// </summary>

public class Graph<T>
{
    public delegate bool NodeCondition(Node<T> currentNode, Node<T> nextNode);

    private int _width;
    private int _height;
    private Node<T>[,] _graph;

    public Graph() { }

    public Graph(int width, int height)
    {
        _width = width;
        _height = height;
        _graph = new Node<T>[width, height];
        Generate();
    }

    public Node<T> GetNode(Vector2 position)
    {
        return _graph[(int)position.x, (int)position.y];
    }

    public Node<T> GetNode(int x, int y)
    {
        return _graph[x, y];
    }

    private void Generate()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                // Create the tile
                Node<T> node = new Node<T>();
                // Add node to graph
                node.Position = new Vector2(x, y);
                if (x > 0)
                { // west connection
                    Node<T> other = _graph[x - 1, y];
                    node.Edges.Add(new Edge<T> { Target = other, Cost = 1, DirectionFromParent = Direction.WEST });
                    other.Edges.Add(new Edge<T> { Target = node, Cost = 1, DirectionFromParent = Direction.EAST });
                }
                if (y > 0)
                { // north connection
                    Node<T> other = _graph[x, y - 1];
                    node.Edges.Add(new Edge<T> { Target = other, Cost = 1, DirectionFromParent = Direction.NORTH });
                    other.Edges.Add(new Edge<T> { Target = node, Cost = 1, DirectionFromParent = Direction.SOUTH });
                }
                // Set the tile on the grid
                _graph[x, y] = node;
            }
        }
    }

    /// <summary>
    /// Calculates distance between two nodes without including diagnols
    /// </summary>
    /// <param name="node">The node to start from</param>
    /// <param name="goal">The node the path ends</param>
    public float CalculateManhattanDistance(Node<T> node, Node<T> goal)
    {
        return Math.Abs(node.Position.x - goal.Position.x) + Math.Abs(node.Position.y - goal.Position.y);
    }

    /// <summary>
    /// Calculates distance between two points without including diagnols
    /// </summary>
    /// <param name="position">The point to start from</param>
    /// <param name="goal">The point the path ends</param>
    public float CalculateManhattanDistance(Vector2 position, Vector2 goal)
    {
        return Math.Abs(position.x - goal.x) + Math.Abs(position.y - goal.y);
    }

    /// <summary>
    /// A custom heuristic for path finding that gets the world distance between two nodes
    /// </summary>
    /// <param name="panel">The starting node</param>
    /// <param name="goal">The end of the node</param>
    private float CustomHeuristic(Node<T> panel, Node<T> goal)
    {
        return Vector3.Distance(goal.Position, panel.Position);
    }

    /// <summary>
    /// Finds the distance between two nodes while including diagnol distance
    /// </summary>
    /// <param name="panel">The node the path starts at</param>
    /// <param name="goal">The node the path ends with</param>
    /// <returns></returns>
    public float CalculateDiagnolDistance(Node<T> panel, Node<T> goal)
    {
        float dx = Math.Abs(panel.Position.x - goal.Position.x);
        float dy = Math.Abs(panel.Position.y - goal.Position.y);
        return 2 * (dx + dy) + (3 - 2 * 2) * Math.Min(dx, dy);
    }

    /// <summary>
    /// Sorts nodes to be in order from lowest to highest f score using bubble sort
    /// </summary>
    /// <param name="nodelist">The list of nodes to sort</param>
    /// <returns>The sorted list</returns>
    public List<Node<T>> SortNodes(List<Node<T>> nodelist)
    {
        Node<T> temp;

        for (int i = 0; i < nodelist.Count - 1; i++)
        {
            for (int j = 0; j < nodelist.Count - i - 1; j++)
            {
                if (nodelist[j].FScore > nodelist[j + 1].FScore)
                {
                    temp = nodelist[j + 1];
                    nodelist[j + 1] = nodelist[j];
                    nodelist[j] = temp;
                }
            }
        }

        return nodelist;
    }

    /// <summary>
    /// Creates a list of nodes that represent the path found
    /// </summary>
    /// <param name="startPanel">The node the path starts from</param>
    /// <param name="endPanel">The node the path ends with</param>
    private List<Node<T>> ReconstructPath(Node<T> startPanel, Node<T> endPanel)
    {
        List<Node<T>> currentPath = new List<Node<T>>();

        int iterCount = 0;
        //Travels backwards from goal node using the node parent until it reaches the starting node
        Node<T> temp = endPanel;
        while (temp != null)
        {
            iterCount++;

            if (iterCount > 100)
                throw new Exception("InfiniteLoop");
            //Insert each node at the beginning of the list so that the path is in the correct order
            currentPath.Insert(0, temp);
            temp = temp.Parent;
        }

        return currentPath;
    }

    /// <summary>
    /// Gets whether or not the data is in the given list
    /// </summary>
    /// <param name="panelNodes">The list to look for the data in</param>
    /// <param name="data">The data to search for in the list</param>
    /// <returns>Whether or not the data was found in the list</returns>
    private bool ContainsData(List<Node<T>> panelNodes, T data)
    {
        //Loop until a panel that matches the argument is found
        foreach (Node<T> node in panelNodes)
        {
            if (EqualityComparer<T>.Default.Equals(node.Data, data))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Uses A* to find a path from the starting position to the end position
    /// </summary>
    /// <param name="startNode">The data of the node where the path will start</param>
    /// <param name="endNode">The data of the node where the path will end</param>
    /// <param name="condition">The condition that will be used to determine whether or not a node should be avoided</param>
    /// <param name="allowPartialPath">Whether or not a path to the closest panel possible if the target is unreachable</param>
    /// <returns>A list containing the constructed path</returns>
    public List<Node<T>> GetPath(T startNode, T endNode, NodeCondition condition = null, bool allowPartialPath = false)
    {
        Node<T> current = null;
        List<Node<T>> openList = new List<Node<T>>();
        Node<T> start = new Node<T> { Data = startNode };
        Node<T> end = new Node<T> {Data = endNode };
        openList.Add(start);
        List<Node<T>> closedList = new List<Node<T>>();
        start.FScore =
            CalculateManhattanDistance(start, end);

        start.Parent = null;

        int iterCount = 0;
        while (openList.Count > 0)
        {
            iterCount++;

            if (iterCount > 100)
                throw new Exception("InfiniteLoop");
            openList = SortNodes(openList);
            current = openList[0];

            if (current == end)
                return ReconstructPath(start, current);

            openList.Remove(current);
            closedList.Add(current);

            foreach (Edge<T> edge in current.Edges)
            {
                if (closedList.Contains(edge.Target) || openList.Contains(edge.Target))
                    continue;
                else if (condition?.Invoke(current, edge.Target) == true)
                    continue;
                else
                {
                    edge.Target.GScore += current.GScore;
                    edge.Target.FScore = edge.Target.GScore + CalculateManhattanDistance(edge.Target, end);
                    edge.Target.Parent = current;
                    openList.Add(edge.Target);
                }
            }
        }

        closedList = SortNodes(closedList);
        Node<T> newGoal = closedList[0];
        if (allowPartialPath)
            return GetPath(startNode, newGoal.Data, condition);

        return new List<Node<T>>();
    }

    /// <summary>
    /// Uses A* to find a path from the starting position to the end position
    /// </summary>
    /// <param name="startPos">The position where the path will start</param>
    /// <param name="endPos">The position where the path will end</param>
    /// <param name="condition">The condition that will be used to determine whether or not a node should be avoided</param>
    /// <param name="allowPartialPath">Whether or not a path to the closest panel possible if the target is unreachable</param>
    /// <returns>A list containing the constructed path</returns>
    public List<Node<T>> GetPath(Vector2 startPos, Vector2 endPos, NodeCondition condition = null, bool allowPartialPath = false)
    {
        Node<T> current = null;
        List<Node<T>> openList = new List<Node<T>>();
        Node<T> start = GetNode(startPos);
        Node<T> end = GetNode(endPos);
        openList.Add(start);
        List<Node<T>> closedList = new List<Node<T>>();
        start.FScore =
            CalculateManhattanDistance(start, end);

        start.Parent = null;
        int iterCount = 0;
        while (openList.Count > 0)
        {
            iterCount++;

            if (iterCount > 100)
                throw new Exception("InfiniteLoop");

            openList = SortNodes(openList);
            current = openList[0];

            if (current == end)
                return ReconstructPath(start, current);

            openList.Remove(current);
            closedList.Add(current);

            foreach (Edge<T> edge in current.Edges)
            {
                if (closedList.Contains(edge.Target) || openList.Contains(edge.Target))
                    continue;
                else if (condition?.Invoke(current, edge.Target) == true)
                    continue;
                else
                {
                    edge.Target.GScore += current.GScore;
                    edge.Target.FScore = edge.Target.GScore + CalculateManhattanDistance(edge.Target, end);
                    edge.Target.Parent = current;
                    openList.Add(edge.Target);
                }
            }
        }

        closedList = SortNodes(closedList);
        Node<T> newGoal = closedList[0];
        if (allowPartialPath)
            return GetPath(startPos, newGoal.Position, condition);

        return new List<Node<T>>();
    }
}
