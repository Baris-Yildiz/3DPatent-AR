using UnityEngine;

public static class PivotSetter 
{
    public static Vector3 ChangeToOneOneMode(GameObject patent ,Transform camTransform, float offSet)
    {
        if (patent == null)
        {
            Debug.Log("Patent is null");
            return Vector3.positiveInfinity;
        }

        Physics.SyncTransforms();
            
        Vector3 calculatedOffSet = Vector3.zero;
        patent.transform.localScale = Vector3.one;
        Bounds bounds = GetSpawnBounds(patent);
        
        Vector3 mins = bounds.min;
        Vector3 maxs = bounds.max;
        Vector3 center = bounds.center;
        
        float minX = mins.x;
        float maxX = maxs.x;
        float minZ = mins.z;
        float maxZ = maxs.z;

        float camX = camTransform.position.x;
        float camZ = camTransform.position.z;
        
        
        bool isInsideX = camX >= minX && camX <= maxX;
        bool isInsideZ = camZ >= minZ && camZ <= maxZ;

        if (isInsideX && isInsideZ)
        {

            Vector2 boundsCenterXZ = new Vector2(center.x, center.z);
            Vector2 cameraXZ = new Vector2(camX, camZ);
            
           // Vector2 pushDir = (boundsCenterXZ - cameraXZ).normalized;
           Vector2 pushDir = new Vector2(camTransform.forward.x, camTransform.forward.z).normalized;
            if (pushDir == Vector2.zero) pushDir = new Vector2(camTransform.forward.x, camTransform.forward.z).normalized;
            float distToEdgeX = (Mathf.Abs(pushDir.x) > 0.001f) 
                ? bounds.extents.x / Mathf.Abs(pushDir.x) 
                : float.MaxValue;

            
            float distToEdgeZ = (Mathf.Abs(pushDir.y) > 0.001f) 
                ? bounds.extents.z / Mathf.Abs(pushDir.y) 
                : float.MaxValue;
            
            float exactDistanceToEdge = Mathf.Min(distToEdgeX, distToEdgeZ);
            float targetDistance = exactDistanceToEdge + offSet;
            
            Vector2 newCenterXZ = cameraXZ + (pushDir * targetDistance);
            Vector2 movementDelta = newCenterXZ - boundsCenterXZ;
            
            patent.transform.position += new Vector3(movementDelta.x, 0, movementDelta.y);
        }
        
        return calculatedOffSet;
    }

    public static Bounds GetSpawnBounds(GameObject patent)
    {
        MeshRenderer[] meshRenderers = patent.GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length == 0)
        {
            return new Bounds(patent.transform.position, Vector3.zero);
        }

        Bounds bounds = meshRenderers[0].bounds;
        foreach (var mesh in meshRenderers)
        {
            bounds.Encapsulate(mesh.bounds);
        }

        return bounds;
    }

    public static Bounds GetBounds(GameObject patent)
    {
        MeshFilter[] filters = patent.GetComponentsInChildren<MeshFilter>();
        if (filters.Length == 0)
            return new Bounds(Vector3.zero, Vector3.one * 0.1f);

        Matrix4x4 worldToRoot = patent.transform.worldToLocalMatrix;
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

    public static void SnapToYOffset(GameObject patent , bool toGround , bool usePivot)
    {
        if (patent == null)
        {
            Debug.Log("patent is null from pivot setter");
            return;
        }

        Bounds bounds = GetSpawnBounds(patent);
        Vector3 center = usePivot ? patent.transform.position : bounds.center;
        float target = toGround ? bounds.min.y : center.y;
        Vector2 xzCenter = new Vector2(center.x, center.z);
        Transform t = patent.transform;
        t.position = new Vector3(2*t.position.x - xzCenter.x, 2 * t.position.y - target, 2*t.position.z - xzCenter.y);
    }



}
