using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshGenerationBehaviour : MonoBehaviour
{
    private NavMeshBuildSettings _buildSettings;

    private NavMeshBuildMarkup _markup;
    [SerializeField, Tooltip("Layers that will be included when building the nav mesh. This is not limited to walkable layers.")]
    private LayerMask _includedLayers;

    [SerializeField, Tooltip("The agent type ID the NavMesh will be baked for.")]
    private int _agentTypeID;

    [SerializeField, Tooltip("The radius of the agent for baking in world units.")]
    private float _agentRadius;

    [SerializeField, Tooltip("The height of the agent for baking in world units.")]
    private float _agentHeight;

    [SerializeField, Tooltip("The maximum slope angle which is walkable (angle in degrees).")]
    private float _agentSlope;

    [SerializeField, Tooltip("The maximum vertical step size an agent can take.")]
    private float _agentClimb;

    

    // Start is called before the first frame update
    void Awake()
    {
        _buildSettings = new NavMeshBuildSettings { agentClimb = _agentClimb, agentHeight = _agentHeight, agentRadius = _agentRadius, agentSlope = _agentSlope, agentTypeID = _agentTypeID };
    }

    public void GenerateNavMesh()
    {
        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
        NavMeshBuildMarkup markup = new NavMeshBuildMarkup { area = 0, root = transform };
        markups.Add(markup);

        NavMeshBuilder.CollectSources(transform, _includedLayers, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);

        NavMeshData data = NavMeshBuilder.BuildNavMeshData(_buildSettings, sources, new Bounds(transform.position, Vector3.one * 500), transform.position, transform.rotation);
        NavMesh.AddNavMeshData(data);
    }
}
