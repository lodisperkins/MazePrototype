using System.Collections.Generic;
using UnityEngine;
using DungeonGeneration;
using System.IO;
using Newtonsoft.Json;

public class LevelBehaviour : MonoBehaviour
{
    private Graph<RoomDescription> _roomGraph;
    private List<RoomDescription> _openRooms;
    private LevelTemplate _template;
    private Vector2 _startPosition = new Vector2( -1, -1 );
    private Vector2 _exitPosition = new Vector2(-1, -1);
    private Vector2 _travelDirection;
    private Node<RoomDescription> _lastNodeEvaluated;

    public int Width { get => _template.Width; }
    public int Height { get => _template.Height; }
    public Graph<RoomDescription> RoomGraph { get => _roomGraph; }
    public List<RoomDescription> OpenRooms { get => _openRooms; }

    private void Awake()
    {
        InitTemplate();
        _roomGraph = new Graph<RoomDescription>(_template.Width, _template.Height);
        GenerateShapes();
        FindPath();
    }

    /// <summary>
    /// Loads all level templates for the world and picks one at random to be used.
    /// </summary>
    private void InitTemplate()
    {
        LevelTemplate[] templates = Resources.LoadAll<LevelTemplate>("World1/LevelTemplates");

        _template = templates[Random.Range(0, templates.Length)];
    }

    private bool CheckValidNode(Node<RoomDescription> currentNode, Node<RoomDescription> nextNode)
    {
        _lastNodeEvaluated = nextNode;

        if (nextNode.Position.x - currentNode.Position.x < 0 && _travelDirection == Vector2.right)
            return false;
        if (nextNode.Position.y - currentNode.Position.y < 0 && _travelDirection == Vector2.up)
            return false;
        if (nextNode.Data.inkColor == "Black")
            return false;

        return true;
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
        int randChoice = Random.Range(0, files.Length);
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
                else if (_startPosition == new Vector2(-1, -1))
                    _startPosition = new Vector2(x, y);
                else
                    _exitPosition = new Vector2(x, y);
            }
        }
    }

    private void FindPath()
    {
        //TO DO: Loop until the the exit position is either replaced or reached
        bool exitPosFound = false;
        Vector2 currentStartPosition = _startPosition;
        int nodesTravelled = 0;

        //Loop while a valid exit position hasn't been found.
        while (!exitPosFound)
        {
            //Use A* to find a path from the potential start and exit position.
            List<Node<RoomDescription>> path = _roomGraph.GetPath(currentStartPosition, _exitPosition, CheckValidNode, true);

            //If the length of the path has exceeded the graphite limit...
            if (path.Count >= _template.DefaultGraphite - nodesTravelled)
            {
                //...place the exit at the farthest node in the path.
                _exitPosition = path[_template.DefaultGraphite - 1].Position;
                exitPosFound = true;
            }
            //Otherwise if the exit couldn't be reached because of obstacles...
            else if (path[path.Count - 1].Position != _exitPosition)
            {
                //...clear the path.
                Node<RoomDescription> lastNode = path[path.Count - 1];
                foreach (Edge<RoomDescription> edge in lastNode.Edges)
                    edge.Target.Data.inkColor = "White";

                //Update the start position so previous nodes aren't evaluated again.
                currentStartPosition = path[path.Count - 1].Position;
                nodesTravelled = path.Count;
            }
            else
                exitPosFound = true;
        }

        //Mark the nodes at the start an end positions so they can be displayed correctly.
        _roomGraph.GetNode(_startPosition).Data.stickerType = "Start";
        _roomGraph.GetNode(_exitPosition).Data.stickerType = "End";
    }

    /// <summary>
    /// Picks two random directions and loads a shape for each.
    /// </summary>
    private void GenerateShapes()
    {
        int randChoice = Random.Range(0, 2);

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

