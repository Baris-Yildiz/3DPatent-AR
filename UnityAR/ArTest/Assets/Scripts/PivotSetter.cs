using UnityEngine;

public static class PivotSetter 
{
    public static Vector3 CalculateOffSet(MeshRenderer renderer , float offSet)
    {
        if (renderer == null)
        {
            Debug.Log("Renderer null");
            return Vector3.positiveInfinity;
        }

        Vector3 calculatedOffSet = Vector3.zero;
        Bounds bounds = renderer.bounds;

        Vector3 objPosition = renderer.gameObject.transform.position;

        Vector3 mins = bounds.min;
        Vector3 maxs = bounds.max;
        Vector3 center = bounds.center;
        Debug.Log("min max info");
        Debug.Log(mins);
        Debug.Log(maxs);
        if (objPosition.x - center.x >= Mathf.Abs(offSet))
        {
            calculatedOffSet.x = -(objPosition.x - center.x);
        }
        if (objPosition.y - mins.y >= Mathf.Abs(offSet))
        {
            calculatedOffSet.y = -(objPosition.y - mins.y);
        }
        if (objPosition.z - maxs.z >= Mathf.Abs(offSet))
        {
            calculatedOffSet.z = -(objPosition.z - maxs.z);
        }
        return calculatedOffSet;
    }
    
    
    
}
