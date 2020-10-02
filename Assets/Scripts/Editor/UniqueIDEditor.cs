using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(UniqueID))]
public class UniqueIDEditor : Editor
{
    private UniqueID targetID;

    private void OnEnable()
    {
        targetID = (UniqueID)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        targetID.m_val = GetUniqueID();        
        
        EditorGUILayout.LabelField("Unique ID: " + targetID.m_val);

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetID);
    }

    private int GetUniqueID()
    {
        UniqueID[] allUniqueIDs = FindObjectsOfType<UniqueID>();

        List<int> sortedIDs = new List<int>();

        for (int IDIndex = 0; IDIndex < allUniqueIDs.Length; IDIndex++)
        {
            sortedIDs.Add(allUniqueIDs[IDIndex].m_val);
        }

        sortedIDs.Remove(targetID.m_val); //Remove own ID
        sortedIDs.Sort();

        if (targetID.m_val == -1 || sortedIDs.Contains(targetID.m_val)) //In Default, or another unique has this ID, find another
        {
            if (sortedIDs.Count == 0)
                return 0;

            return sortedIDs[sortedIDs.Count - 1] + 1;
        }

        return targetID.m_val;//CurrentID is good to go
    }
}
