using UnityEngine;
using UnityMeshSimplifier;

public class GenerateLOD : MonoBehaviour
{
    private LODLevel[] m_LodLevels;
    private bool m_AutoCollectRenderers;
    private SimplificationOptions m_SimplificationOptions;
    private float[] m_LODQualities; //qualities of lods
    private const int LOD_COUNT = 2;

    private float[] m_LODTransitionHeights; //e.g. object will transition from lod0 to lod1 when height of lod box is transitionheights[0]
    // last lod is cull by default

    private void Awake()
    {
        m_LODQualities = new float[LOD_COUNT] { 1.0f, 0.65f};
        m_LODTransitionHeights = new float[LOD_COUNT] { 0.5f, 0.17f};

        m_LodLevels = new LODLevel[LOD_COUNT];
        m_LodLevels[0] = new LODLevel(m_LODTransitionHeights[0], m_LODQualities[0]) //extra options
        {
            CombineMeshes = false,
            CombineSubMeshes = false,
            SkinQuality = SkinQuality.Auto,
            ShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
            ReceiveShadows = true,
            SkinnedMotionVectors = true,
            LightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes,
            ReflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes
        };

        m_LodLevels[1] = new LODLevel(m_LODTransitionHeights[1], m_LODQualities[1])
        {
            CombineMeshes = true,
            CombineSubMeshes = false,
            SkinQuality = SkinQuality.Auto,
            ShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
            ReceiveShadows = true,
            SkinnedMotionVectors = true,
            LightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes,
            ReflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Simple
        };
        
        m_AutoCollectRenderers = true;
        m_SimplificationOptions = SimplificationOptions.Default;

        //debug
        Generate();
    }

    public void Generate()
    {
        LODGroup lodGroup = LODGenerator.GenerateLODs(gameObject, m_LodLevels, m_AutoCollectRenderers, m_SimplificationOptions);

        // AttachLODCollider();
    }

    private void AttachLODCollider()
    {
        //LODGenerator algorithm will instantiate a new child of name _UMS_LODs_ .
        //This will have childs Level00 and Level01 representing LODs.
        // We will place the collider on the last LOD due to performance.


        Transform lastChild = transform.GetChild(transform.childCount - 1);
        Transform lodChild = lastChild.Find("Level01");
        Transform meshChild = lodChild.GetChild(0);

        MeshCollider meshCollider = meshChild.gameObject.AddComponent<MeshCollider>();
        MeshFilter meshFilter = meshChild.gameObject.GetComponent<MeshFilter>();

        meshCollider.sharedMesh = meshFilter.sharedMesh;
        meshCollider.convex = true;
    }
}
