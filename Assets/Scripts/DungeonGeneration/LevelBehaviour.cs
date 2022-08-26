using System.Collections.Generic;
using UnityEngine;
using DungeonGeneration;
using System.IO;
using Newtonsoft.Json;
using System;

public class LevelBehaviour : MonoBehaviour
{
    private Graph<RoomDescription> _roomGraph;
    private List<RoomDescription> _openRooms;
    private LevelTemplate _template;
    private Vector2 _startPosition = new Vector2( -1, -1 );
    private Vector2 _exitPosition = new Vector2(-1, -1);
    private Vector2 _travelDirection;
    private Node<RoomDescription> _lastNodeEvaluated;
    private List<Node<RoomDescription>> _playerPath;

    public int Width { get => _template.Width; }
    public int Height { get => _template.Height; }
    public Graph<RoomDescription> RoomGraph { get => _roomGraph; }
    public List<RoomDescription> OpenRooms { get => _openRooms; }
    public List<Node<RoomDescription>> PlayerPath { get => _playerPath; private set => _playerPath = value; }
    public Vector2 StartPosition { get => _startPosition; }
    public Vector2 ExitPosition { get => _exitPosition; }

    private void Awake()
    {
        InitTemplate();
        _roomGraph = new Graph<RoomDescription>(_template.Width, _template.Height);
        GenerateShapes();
        PlaceStartExit();
        FindPath();
        //Mark the nodes at the start an end positions so they can be displayed correctly.
        Node<RoomDescription> _startNode = _roomGraph.GetNode(_startPosition);
        _startNode.Data.stickerType = "Start";
        PlayerPath = new List<Node<RoomDescription>>();
        PlayerPath.Add(_startNode);
        _roomGraph.GetNode(_exitPosition).Data.stickerType = "End";
    }

    /// <summary>
    /// Loads all level templates for the world and picks one at random to be used.
    /// </summary>
    private void InitTemplate()
    {
        LevelTemplate[] templates = Resources.LoadAll<LevelTemplate>("World1/LevelTemplates");

        _template = templates[UnityEngine.Random.Range(0, templates.Length)];
    }

    public void AddNodeToPlayerPath(int x, int y)
    {
        Node<RoomDescription> node = _roomGraph.GetNode(x, y);
        PlayerPath.Add(node);
    }

    private bool CheckInvalidNode(Node<RoomDescription> currentNode, Node<RoomDescription> nextNode)
    {
        _lastNodeEvaluated = nextNode;

        if (nextNode.Position.x - currentNode.Position.x < 0 && _travelDirection == Vector2.right)
            return true;
        if (nextNode.Position.y - currentNode.Position.y < 0 && _travelDirection == Vector2.up)
            return true;
        if (nextNode.Data.inkColor == "Black")
            return true;

        return false;
    }

    /// <summary>
    /// Gets all shapes in the direction folder and chooses one to load at random.
    /// </summary>
    /// <param name="direction">Which side the shape will spawn on the map (North,South,East,West).</param>
    /// <returns>Deserialized data from the ogmo editor that contains all info about the shape.</returns>
    private Root LoadShape(string direction)
    {
        //Loads all shapes in the given direction folder and chooses one at random.
        string[] files = Directory.GetFiles("Assets/Resources/World1/ShapeTemplates/" + direction, "*.json");
        int randChoice = UnityEngine.Random.Range(0, files.Length);
        //Reads all the text from the file and return the deserialized value.
        string jsonDat = File.ReadAllText(files[randChoice]);
        return JsonConvert.DeserializeObject<Root>(jsonDat);
    }

    /// <summary>
    /// Sets the ink color of each node on the graph that should be covered by the shape.
    /// </summary>
    /// <param name="shape">The ogmo data of the shape to place.</param>
    private void PlaceShape(Root shape)
    {
        //Iterates through graph to assign ink color.
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (shape.layers[0].data2D[x, y] != -1)
                    RoomGraph.GetNode(x, y).Data.inkColor = "Black";
            }
        }
    }

    private void PlaceStartExit()
    {
        //Iterates through graph to assign ink color.
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (RoomGraph.GetNode(x, y).Data.inkColor != "Black" && _startPosition == new Vector2(-1, -1))
                    _startPosition = new Vector2(x, y);
                else if (RoomGraph.GetNode(Width - (1 + x), Height - (1 + y)).Data.inkColor != "Black" && _exitPosition == new Vector2(-1, -1))
                    _exitPosition = new Vector2(Width - (1 + x), Height - (1 + y));
                else if (_startPosition != new Vector2(-1, -1) && _exitPosition != new Vector2(-1, -1))
                    return;
            }
        }
    }

    private void FindPath()
    {
        Vector2 currentStartPosition = _startPosition;
        int nodesTravelled = 0;

        //Loop while a valid exit position hasn't been found.
        for (int i = 0; i < _template.Width * Height; i++)
        {
            //Use A* to find a path from the potential start and exit position.
            List<Node<RoomDescription>> path = _roomGraph.GetPath(currentStartPosition, _exitPosition, CheckInvalidNode, true);

            //Return if the exit was found successfully.
            if (path[path.Count - 1].Position == _exitPosition)
                return;
            //Otherwise if the length of the path has exceeded the graphite limit...
            if (path.Count >= _template.DefaultGraphite - nodesTravelled)
            {
                //...place the exit at the farthest node in the path.
                _exitPosition = path[path.Count - 1].Position;
                return;
            }
            
            //If a path couldn't be found, it is most likely due to it being covered by inkblots.
            //This section removes the inkblots that are blocking the path.

            Node<RoomDescription> lastNode = path[path.Count - 1];
            List<Edge<RoomDescription>> obstacleEdges = lastNode.Edges.FindAll(edge => edge.Target.Data.inkColor == "Black");
            Node<RoomDescription> obstacleNode = null;

            //Throws an error if the path couldn't be found even without being blocked by obstacles.
            if (obstacleEdges.Count == 0)
                throw new Exception("Cannot find a valid path to the exit.");

            //Calculate the f score for the first obstacle in the list so that it can be compared against others.
            obstacleNode = obstacleEdges[0].Target;
            obstacleNode.FScore = obstacleEdges[0].Cost + Vector2.Distance(obstacleNode.Position, _exitPosition);
            //Loops and compares each obstacle node to find which is the closes to the goal.
            for (int j = 1; j < obstacleEdges.Count; j++)
            {
                obstacleEdges[j].Target.FScore = obstacleEdges[j].Cost + Vector2.Distance(obstacleEdges[j].Target.Position, _exitPosition);
                if (obstacleEdges[j].Target.FScore < obstacleNode.FScore)
                    obstacleNode = obstacleEdges[j].Target;
            }

            if (obstacleNode != null)
                obstacleNode.Data.inkColor = "White";

            //Update the start position so previous nodes aren't evaluated again.
            currentStartPosition = lastNode.Position;
            nodesTravelled += path.Count;
        }

    }

    /// <summary>
    /// Picks two random directions and loads a shape for each.
    /// </summary>
    private void GenerateShapes()
    {
        int randChoice = UnityEngine.Random.Range(0, 2);

        Root shape1 = null;
        Root shape2 = null;

        if (randChoice == 0)
        {
            shape1 = LoadShape("North");
            shape2 = LoadShape("South");
            _travelDirection = Vector2.right;
        }
        else
        {
            shape1 = LoadShape("West");
            shape2 = LoadShape("East");
            _travelDirection = Vector2.up;
        }

        PlaceShape(shape1);
        PlaceShape(shape2);
    }
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Layer
{
    public string name { get; set; }
    public string _eid { get; set; }
    public int offsetX { get; set; }
    public int offsetY { get; set; }
    public int gridCellWidth { get; set; }
    public int gridCellHeight { get; set; }
    public int gridCellsX { get; set; }
    public int gridCellsY { get; set; }
    public string tileset { get; set; }
    public int[,] data2D { get; set; }
    public int exportMode { get; set; }
    public int arrayMode { get; set; }
}

public class Root
{
    public string ogmoVersion { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int offsetX { get; set; }
    public int offsetY { get; set; }
    public Values values { get; set; }
    public List<Layer> layers { get; set; }
}

public class Values
{
    public string Direction { get; set; }
}

