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
}
