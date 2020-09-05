using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework.Constraints;
using UnityEditorInternal;

[ExecuteInEditMode]
[CustomEditor(typeof(Pathing_Node))]
public class SplineEditor : Editor
{
    private string[] m_splineLabels = new string[6] { "Forward Spline Details", "Forward Right Spline Details", "Forward Left Spline Details", "Backward Spline Details", "Backward Right Spline Details", "Backward Lef Spline Detailst" };

    private Pathing_Node pathingNodeScript;
    private SerializedProperty m_splineDetails;

    private void OnEnable()
    {
        m_splineDetails = serializedObject.FindProperty("m_pathingSplineDetails");
        pathingNodeScript = (Pathing_Node)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        if(m_splineDetails == null)
            m_splineDetails = serializedObject.FindProperty("m_pathingSplineDetails");

        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            EditorGUILayout.LabelField(m_splineLabels[splineIndex], EditorStyles.boldLabel);

            SerializedProperty nodePrimary = m_splineDetails.GetArrayElementAtIndex(splineIndex).FindPropertyRelative("m_nodePrimary");
            SerializedProperty nodeSecondary = m_splineDetails.GetArrayElementAtIndex(splineIndex).FindPropertyRelative("m_nodeSecondary");

            EditorGUILayout.PropertyField(nodePrimary);
            EditorGUILayout.PropertyField(nodeSecondary);

            //Options for spline usage
            if (nodePrimary.objectReferenceValue != null)
            {
                pathingNodeScript.m_pathingSplineDetails[splineIndex].m_splineType = (Pathing_Spline.SPLINE_TYPE)EditorGUILayout.EnumPopup("Spline Type", pathingNodeScript.m_pathingSplineDetails[splineIndex].m_splineType);

                switch (pathingNodeScript.m_pathingSplineDetails[splineIndex].m_splineType)
                {
                    case Pathing_Spline.SPLINE_TYPE.STRAIGHT:
                        break;
                    case Pathing_Spline.SPLINE_TYPE.BEZIER:
                        SerializedProperty bezierStrengthSO = m_splineDetails.GetArrayElementAtIndex(splineIndex).FindPropertyRelative("m_bezierStrength");
                        EditorGUILayout.PropertyField(bezierStrengthSO);
                        break;
                    case Pathing_Spline.SPLINE_TYPE.CIRCLE:
                        SerializedProperty circleDirSO = m_splineDetails.GetArrayElementAtIndex(splineIndex).FindPropertyRelative("m_circleDir");
                        EditorGUILayout.PropertyField(circleDirSO);
                        SerializedProperty circleAngleSO = m_splineDetails.GetArrayElementAtIndex(splineIndex).FindPropertyRelative("m_circleAngle");
                        EditorGUILayout.PropertyField(circleAngleSO);
                        break;
                    case Pathing_Spline.SPLINE_TYPE.NOT_IN_USE:
                        break;
                    default:
                        break;
                }
            }

            if(m_splineDetails.GetArrayElementAtIndex(splineIndex).FindPropertyRelative("m_createdFlag").boolValue)
            {
                if (GUILayout.Button("Remove Spline"))
                {

                }
            }
            else
            {
                if (GUILayout.Button("Create Spline"))
                {

                }
            }
        }
        //Updating of splines in scene
        if (Application.isEditor && !Application.isPlaying)
        {

        }
        //Apply changes
        serializedObject.ApplyModifiedProperties();
    }
}
