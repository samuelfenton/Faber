using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MOARMaths : MonoBehaviour
{
    public static float SqrMagnitude(Vector3 p_val)
    {
        return p_val.x * p_val.x + p_val.y * p_val.y + p_val.z * p_val.z;
    }

    public static float SqrDistance(Vector3 p_lhs, Vector3 p_rhs)
    {
        Vector3 distance = p_rhs - p_lhs;
        return distance.x * distance.x + distance.y * distance.y + distance.z * distance.z;
    }

    /// <summary>
    /// Used to determine just how "positive" a vector is
    /// That is, does it travel thorugh the positive quadrant, and its magnitude.
    /// Done by comparing its allignemnt to Vector(1,1,1) and its magnitude.
    /// </summary>
    /// <param name="p_vector">Vector to calculate</param>
    /// <returns>Range from Negitivie Infinity->Inifinity</returns>
    public static float GetPositiveAlignment(Vector3 p_vector)
    {
        float alignment = Vector3.Dot(Vector3.one, p_vector);
        return p_vector.magnitude * alignment;
    }
}
