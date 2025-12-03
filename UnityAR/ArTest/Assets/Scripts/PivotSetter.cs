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
        Vector3 calculatedOffSet = Vector3.zero;
        Bounds bounds = GetBounds(patent);
        
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
            
            Vector2 pushDir = (boundsCenterXZ - cameraXZ).normalized;
            
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

    public static Bounds GetBounds(GameObject patent)
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

    public static void SnapToYOffset(GameObject patent , bool toGround)
    {
        if (patent == null)
        {
            Debug.Log("patent is null from pivot setter");
            return;
        }

        Bounds bounds = GetBounds(patent);

        float target = toGround ? bounds.min.y : bounds.center.y;
        Vector2 xzCenter = new Vector2(bounds.center.x, bounds.center.z);
        Transform t = patent.transform;
        t.position = new Vector3(2*t.position.x - xzCenter.x, 2 * t.position.y - target, 2*t.position.z - xzCenter.y);
    }



}
