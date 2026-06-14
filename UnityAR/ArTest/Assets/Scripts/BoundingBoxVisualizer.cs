using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Renders a wireframe bounding box around the active patent model using
/// 12 <see cref="LineRenderer"/> edges. Visibility is toggled via
/// <see cref="PopupMenuManager.OnEnableBoundingBox"/> and the box is
/// automatically hidden when the model is deleted.
/// </summary>
public class BoundingBoxVisualizer : MonoBehaviour
{

    [SerializeField] private Color boxColor = Color.green;
    [SerializeField] private PopupMenuManager popupMenuManager;
    [SerializeField] private Shader lineShader;

    // Corner index layout:
    //  0=(min.x, min.y, min.z)  1=(max.x, min.y, min.z)
    //  2=(min.x, min.y, max.z)  3=(max.x, min.y, max.z)
    //  4=(min.x, max.y, min.z)  5=(max.x, max.y, min.z)
    //  6=(min.x, max.y, max.z)  7=(max.x, max.y, max.z)
    private static readonly int[][] EdgePairs =
    {
        new[]{0,1}, new[]{1,3}, new[]{3,2}, new[]{2,0},  // bottom face
        new[]{4,5}, new[]{5,7}, new[]{7,6}, new[]{6,4},  // top face
        new[]{0,4}, new[]{1,5}, new[]{2,6}, new[]{3,7}   // vertical edges
    };

    private LineRenderer[] _edges;
    private Material _material;
    private bool _isVisible;
    private readonly Vector3[] _worldCorners = new Vector3[8];
    
    /// <summary>
    /// Subscribes to delete and visibility events, creates the shared line material,
    /// and instantiates the 12 edge <see cref="LineRenderer"/> child objects.
    /// The box starts hidden.
    /// </summary>
    private void Start()
    {

       // PatentManager.Instance.patentDeletedEvent += OnPatentDeleted;
        if (ModelTransformManager.Instance != null)
            ModelTransformManager.Instance.OnDeleteModel += OnPatentDeleted;
        else
            Debug.LogError("delete is ulaaa");

        if (popupMenuManager != null)
            popupMenuManager.OnEnableBoundingBox += SetVisible;
        else
            Debug.LogError("PopupMenuManager is null on BoundingBoxVisualizer");

        _material = new Material(lineShader != null ? lineShader : Shader.Find("Unlit/Color"));
        _material.color = boxColor;

        _edges = new LineRenderer[12];
        for (int i = 0; i < 12; i++)
        {
            GameObject obj = new GameObject("Edge_" + i);
            obj.transform.SetParent(transform, false);
            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.material = _material;
            lr.positionCount = 2;
            lr.startWidth = lr.endWidth = 0.005f;
            lr.useWorldSpace = true;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.generateLightingData = false;
            obj.SetActive(false);
            _edges[i] = lr;
        }

        SetVisible(false);
    }

    /// <summary>
    /// Unsubscribes all events and destroys the shared line material to prevent
    /// memory leaks when the component is removed from the scene.
    /// </summary>
    private void OnDestroy()
    {
        if (ModelTransformManager.Instance != null)
            ModelTransformManager.Instance.OnDeleteModel -= OnPatentDeleted;
        if (PatentManager.Instance != null)
            PatentManager.Instance.patentDeletedEvent -= OnPatentDeleted;
        if (popupMenuManager != null)
            popupMenuManager.OnEnableBoundingBox -= SetVisible;
        if (_material != null) Destroy(_material);
    }

    /// <summary>
    /// Records the desired visibility state and hides all edges immediately
    /// when visibility is turned off.
    /// </summary>
    /// <param name="visible"><c>true</c> to allow the box to be drawn; <c>false</c> to hide it.</param>
    private void SetVisible(bool visible)
    {
        _isVisible = visible;
        if (!visible)
            SetEdgesActive(false);
    }

    /// <summary>Hides all edge renderers when the active patent is deleted.</summary>
    private void OnPatentDeleted()
    {
        SetEdgesActive(false);
    }

    /// <summary>
    /// Each frame, when visible, retrieves the active patent and either hides
    /// the edges (no patent present) or refreshes their positions to track the
    /// model as it moves and scales.
    /// </summary>
    private void Update()
    {
        if (!_isVisible) return;

        GameObject patent = PatentManager.Instance?.ActivePatent;

        if (patent == null)
        {
            SetEdgesActive(false);
            return;
        }

        SetEdgesActive(true);
        RefreshEdges(patent);
    }

    /// <summary>
    /// Enables or disables all 12 edge <see cref="LineRenderer"/> GameObjects.
    /// </summary>
    /// <param name="active"><c>true</c> to show edges; <c>false</c> to hide them.</param>
    private void SetEdgesActive(bool active)
    {
        for (int i = 0; i < _edges.Length; i++)
            _edges[i].gameObject.SetActive(active);
    }

    /// <summary>
    /// Recomputes all 8 world-space corner positions of <paramref name="root"/>'s
    /// local bounds, scales line widths relative to the box diagonal, and updates
    /// both endpoints of each of the 12 edge renderers.
    /// </summary>
    /// <param name="root">The patent root GameObject whose bounds define the box.</param>
    private void RefreshEdges(GameObject root)
    {
        Bounds localBounds = ComputeLocalBounds(root);
        Vector3 min = localBounds.min;
        Vector3 max = localBounds.max;
        Matrix4x4 l2w = root.transform.localToWorldMatrix;

        _worldCorners[0] = l2w.MultiplyPoint3x4(new Vector3(min.x, min.y, min.z));
        _worldCorners[1] = l2w.MultiplyPoint3x4(new Vector3(max.x, min.y, min.z));
        _worldCorners[2] = l2w.MultiplyPoint3x4(new Vector3(min.x, min.y, max.z));
        _worldCorners[3] = l2w.MultiplyPoint3x4(new Vector3(max.x, min.y, max.z));
        _worldCorners[4] = l2w.MultiplyPoint3x4(new Vector3(min.x, max.y, min.z));
        _worldCorners[5] = l2w.MultiplyPoint3x4(new Vector3(max.x, max.y, min.z));
        _worldCorners[6] = l2w.MultiplyPoint3x4(new Vector3(min.x, max.y, max.z));
        _worldCorners[7] = l2w.MultiplyPoint3x4(new Vector3(max.x, max.y, max.z));

        Bounds wb = new Bounds(_worldCorners[0], Vector3.zero);
        for (int i = 1; i < 8; i++) wb.Encapsulate(_worldCorners[i]);
        float lw = Mathf.Max(wb.size.magnitude * 0.003f, 0.001f);

        for (int i = 0; i < 12; i++)
        {
            LineRenderer lr = _edges[i];
            lr.startWidth = lr.endWidth = lw;
            lr.SetPosition(0, _worldCorners[EdgePairs[i][0]]);
            lr.SetPosition(1, _worldCorners[EdgePairs[i][1]]);
        }
    }

    /// <summary>
    /// Computes the axis-aligned bounding box of all meshes in <paramref name="root"/>'s
    /// hierarchy, expressed in <paramref name="root"/>'s local space by transforming
    /// each mesh's eight corners through the root's world-to-local matrix.
    /// </summary>
    /// <param name="root">The root GameObject whose child mesh filters are measured.</param>
    /// <returns>
    /// The local-space <see cref="Bounds"/> encapsulating all meshes, or a small
    /// default bounds if no mesh filters are found.
    /// </returns>
    private Bounds ComputeLocalBounds(GameObject root)
    {
        MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
        if (filters.Length == 0)
            return new Bounds(Vector3.zero, Vector3.one * 0.1f);

        Matrix4x4 worldToRoot = root.transform.worldToLocalMatrix;
        bool initialized = false;
        Bounds result = default;

        foreach (MeshFilter mf in filters)
        {
            if (mf.sharedMesh == null) continue;

            Bounds mb = mf.sharedMesh.bounds;
            Matrix4x4 meshToRoot = worldToRoot * mf.transform.localToWorldMatrix;

            for (int x = 0; x <= 1; x++)
            for (int y = 0; y <= 1; y++)
            for (int z = 0; z <= 1; z++)
            {
                Vector3 corner = new Vector3(
                    x == 0 ? mb.min.x : mb.max.x,
                    y == 0 ? mb.min.y : mb.max.y,
                    z == 0 ? mb.min.z : mb.max.z);

                Vector3 rootCorner = meshToRoot.MultiplyPoint3x4(corner);

                if (!initialized) { result = new Bounds(rootCorner, Vector3.zero); initialized = true; }
                else result.Encapsulate(rootCorner);
            }
        }

        return initialized ? result : new Bounds(Vector3.zero, Vector3.one * 0.1f);
    }
}
