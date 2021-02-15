using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MOARMaths : MonoBehaviour
{
    /// <summary>
    /// Get the square magnitude for a single vector
    /// Its faster then getting square root
    /// </summary>
    /// <param name="p_val">Vector to measure</param>
    /// <returns></returns>
    public static float SqrMagnitude(Vector3 p_val)
    {
        return p_val.x * p_val.x + p_val.y * p_val.y + p_val.z * p_val.z;
    }

    /// <summary>
    /// Get the square distance between two Vectors
    /// Its faster then getting square root
    /// </summary>
    /// <param name="p_lhs">First Vector</param>
    /// <param name="p_rhs">Second Vector</param>
    /// <returns></returns>
    public static float SqrDistance(Vector3 p_lhs, Vector3 p_rhs)
    {
        Vector3 distance = p_rhs - p_lhs;
        return SqrMagnitude(distance);
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

    /// <summary>
    /// Returns true if the scene 'name' exists and is in your Build settings, false otherwise
    /// </summary>
    public static bool DoesSceneExist(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            int lastSlash = scenePath.LastIndexOf("/");
            string sceneName = scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1);

            if (string.Compare(name, sceneName, true) == 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Given the "2D" nature of the game, Vector3 can be converted into its horizontal and vertical components
    /// </summary>
    /// <param name="p_in"></param>
    /// <returns></returns>
    public static Vector2 ConvertFromVector3ToVector2(Vector3 p_in)
    {
        return new Vector2(Mathf.Sqrt(p_in.x * p_in.x + p_in.z * p_in.z), p_in.y);
    }
}
