using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for AI utility function like pathfinding, 
/// </summary>

public sealed class Graph<T>
{
    public delegate bool NodeCondition(Node<T> node);

    public class Node<T>
    {
        public Node<T> Parent = null;
        public T Data;
        public Vector2 Position;
        public List<Edge<T>> Edges;
        public float GScore;
        public float HScore;
        public float FScore;
    }

    public struct Edge<T>
    {
        public Node<T> Target;
        public float Cost;
    }

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
                    node.Edges.Add(new Edge<T> { Target = other, Cost = 1 });
                    other.Edges.Add(new Edge<T> { Target = node, Cost = 1 });
                }
                if (y > 0)
                { // north connection
                    Node<T> other = _graph[x, y - 1];
                    node.Edges.Add(new Edge<T> { Target = other, Cost = 1 });
                    other.Edges.Add(new Edge<T> { Target = node, Cost = 1 });
                }
                // Set the tile on the grid
                _graph[x, y] = node;
            }
        }
    }

    /// <summary>
    /// Calculates distance between two panels without including diagnols
    /// </summary>
    /// <param name="panel">The panel to start from</param>
    /// <param name="goal">The panel the path ends</param>
    public float CalculateManhattanDistance(Node<T> panel, Node<T> goal)
    {
        return Math.Abs(panel.Position.x - goal.Position.x) + Math.Abs(panel.Position.y - goal.Position.y);
    }

    /// <summary>
    /// A custom heuristic for path finding that gets the world distance between two panels
    /// </summary>
    /// <param name="panel">The starting panel</param>
    /// <param name="goal">The end of the path</param>
    private float CustomHeuristic(Node<T> panel, Node<T> goal)
    {
        return Vector3.Distance(goal.Position, panel.Position);
    }

    /// <summary>
    /// Finds the distance between two panels while including diagnol distance
    /// </summary>
    /// <param name="panel">The panel the path starts at</param>
    /// <param name="goal">The panel the path ends with</param>
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
    private List<Node<T>> SortNodes(List<Node<T>> nodelist)
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
    /// Creates a list of panels that represent the path found
    /// </summary>
    /// <param name="startPanel">The panel the path starts from</param>
    /// <param name="endPanel">The panel the path ends with</param>
    private List<Node<T>> ReconstructPath(Node<T> startPanel, Node<T> endPanel)
    {
        List<Node<T>> currentPath = new List<Node<T>>();

        //Travels backwards from goal node using the node parent until it reaches the starting node
        Node<T> temp = endPanel;
        while (temp != null)
        {
            //Insert each panel at the beginning of the list so that the path is in the correct order
            currentPath.Insert(0, temp);
            temp = temp.Parent;
        }

        return currentPath;
    }

    /// <summary>
    /// Gets whether or not the panel is in the given list
    /// </summary>
    /// <param name="panelNodes">The list to look for the panel in</param>
    /// <param name="data">The data to search for in the list</param>
    /// <returns>Whether or not the panel was within the list</returns>
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
    /// Uses A* to find a path from the starting panel to the end panel
    /// </summary>
    /// <param name="startPos">The panel where the path will start</param>
    /// <param name="endPos">The panel where the path will end</param>
    /// <param name="allowPartialPath">Whether or not the path should avoid panels that are occupied</param>
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

        while (openList.Count > 0)
        {
            openList = SortNodes(openList);
            current = openList[0];

            if (EqualityComparer<T>.Default.Equals(current.Data, end.Data))
                return ReconstructPath(start, current);

            openList.Remove(current);
            closedList.Add(current);

            foreach (Edge<T> edge in current.Edges)
            {
                if (closedList.Contains(edge.Target) || openList.Contains(edge.Target))
                    continue;
                else if (condition?.Invoke(edge.Target) == true)
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

        if (allowPartialPath)
            return ReconstructPath(start, current);

        return new List<Node<T>>();
    }

    /// <summary>
    /// Uses A* to find a path from the starting panel to the end panel
    /// </summary>
    /// <param name="startPos">The panel where the path will start</param>
    /// <param name="endPos">The panel where the path will end</param>
    /// <param name="allowPartialPath">Whether or not the path should avoid panels that are occupied</param>
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

        while (openList.Count > 0)
        {
            openList = SortNodes(openList);
            current = openList[0];

            if (EqualityComparer<T>.Default.Equals(current.Data, end.Data))
                return ReconstructPath(start, current);

            openList.Remove(current);
            closedList.Add(current);

            foreach (Edge<T> edge in current.Edges)
            {
                if (closedList.Contains(edge.Target) || openList.Contains(edge.Target))
                    continue;
                else if (condition?.Invoke(edge.Target) == true)
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

        if (allowPartialPath)
            return ReconstructPath(start, current);

        return new List<Node<T>>();
    }
}
