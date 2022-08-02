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

    private void InitTemplate()
    {
        LevelTemplate[] templates = Resources.LoadAll<LevelTemplate>("World1/LevelTemplates");

        _template = templates[Random.Range(0, templates.Length)];
    }

    private Root LoadShape(string direction)
    {
        string[] files = Directory.GetFiles("Assets/Resources/World1/ShapeTemplates/" + direction, "*.json");
        int randChoice = Random.Range(0, files.Length);

        string jsonDat = File.ReadAllText(files[randChoice]);
        return JsonConvert.DeserializeObject<Root>(jsonDat);
    }

    private void PlaceShape(Root shape)
    {
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

