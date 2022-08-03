using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonGeneration;
using System.IO;
using Newtonsoft.Json;

public class LevelBehaviour : MonoBehaviour
{
    private Graph<RoomDescription> _roomGraph;
    private LevelTemplate _template;

    public int Width { get => _template.Width; }
    public int Height { get => _template.Height; }
    public Graph<RoomDescription> RoomGraph { get => _roomGraph; }

    private void Awake()
    {
        InitTemplate();
        _roomGraph = new Graph<RoomDescription>(_template.Width, _template.Height);
        GenerateShapes();
    }

    /// <summary>
    /// Loads all level templates for the world and picks one at random to be used.
    /// </summary>
    private void InitTemplate()
    {
        LevelTemplate[] templates = Resources.LoadAll<LevelTemplate>("World1/LevelTemplates");

        _template = templates[Random.Range(0, templates.Length)];
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
                {
                    RoomGraph.GetNode(x, y).Data.inkColor = "Black";
                }
            }
        }
    }

    /// <summary>
    /// Picks two random directions and loads a shape for each.
    /// </summary>
    private void GenerateShapes()
    {
        int randChoice = Random.Range(0, 2);

        Root shape1 = randChoice == 0 ? LoadShape("North") : LoadShape("West");

        Root shape2 = randChoice == 0 ? LoadShape("South") : LoadShape("East");

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

