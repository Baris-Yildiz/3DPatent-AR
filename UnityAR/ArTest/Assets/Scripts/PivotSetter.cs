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

    public static Bounds GetBounds(GameObject patent)
    {
        MeshFilter[] meshFilters = patent.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);

        Bounds combinedBounds = new Bounds();
        bool hasBounds = false;

        foreach (MeshFilter mf in meshFilters)
        {
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) continue;

            // Get the mesh's local bounds
            Bounds b = mesh.bounds;

            // Matrix that converts Child-Local to Root-Local
            // We use GetRelativeMatrix to ensure we don't accidentally include 
            // the root's own world scale/rotation in the calculation
            Matrix4x4 childToRootMatrix = patent.transform.InverseTransformPoint(mf.transform.position) == Vector3.zero 
                ? mf.transform.localToWorldMatrix 
                : patent.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix;

            Vector3[] corners = new Vector3[8];
            Vector3 min = b.min;
            Vector3 max = b.max;
        
            corners[0] = new Vector3(min.x, min.y, min.z);
            corners[1] = new Vector3(min.x, min.y, max.z);
            corners[2] = new Vector3(min.x, max.y, min.z);
            corners[3] = new Vector3(min.x, max.y, max.z);
            corners[4] = new Vector3(max.x, min.y, min.z);
            corners[5] = new Vector3(max.x, min.y, max.z);
            corners[6] = new Vector3(max.x, max.y, min.z);
            corners[7] = new Vector3(max.x, max.y, max.z);

            for (int i = 0; i < 8; i++)
            {
                Vector3 cornerInRootSpace = childToRootMatrix.MultiplyPoint3x4(corners[i]);
                if (!hasBounds)
                {
                    combinedBounds = new Bounds(cornerInRootSpace, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(cornerInRootSpace);
                }
            }
        }
        return combinedBounds;
    }

    public static void SnapToYOffset(GameObject patent , bool toGround)
    {
        if (patent == null) return;

        
        Bounds localBounds = GetBounds(patent);

        Vector3 localTargetPoint = new Vector3(
            localBounds.center.x, 
            toGround ? localBounds.min.y : localBounds.center.y, 
            localBounds.center.z
        );


        Vector3 worldTargetPoint = patent.transform.TransformPoint(localTargetPoint);


        Vector3 offset = patent.transform.position - worldTargetPoint;
        
        patent.transform.position += offset;
    }



}
