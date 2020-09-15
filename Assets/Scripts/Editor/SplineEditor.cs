using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework.Constraints;
using UnityEditorInternal;

[ExecuteInEditMode]
[CustomEditor(typeof(Pathing_Node)), CanEditMultipleObjects]
public class SplineEditor : Editor
{
    private string[] m_splineLabels = new string[6] { "Forward Spline Details", "Forward Right Spline Details", "Forward Left Spline Details", "Backward Spline Details", "Backward Right Spline Details", "Backward Lef Spline Detailst" };

    private Pathing_Node m_pathingNodeScript;
    private SerializedProperty m_splineDetailsSP;
    private SerializedProperty m_conjoinedSplineSP;


    private void OnEnable()
    {
        m_splineDetailsSP = serializedObject.FindProperty("m_pathingSplineDetails");
        m_conjoinedSplineSP = serializedObject.FindProperty("m_conjoinedNodes");

        m_pathingNodeScript = (Pathing_Node)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (m_splineDetailsSP == null)
            m_splineDetailsSP = serializedObject.FindProperty("m_pathingSplineDetails");

        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            EditorGUILayout.LabelField(m_splineLabels[splineIndex], EditorStyles.boldLabel);

            SerializedProperty createdFlagSO = m_splineDetailsSP.GetArrayElementAtIndex(splineIndex).FindPropertyRelative("m_createdFlag");

            //Has this spline already been created?
            if (createdFlagSO.boolValue)
            {
                EditorGUILayout.LabelField("Primary Node: " + m_pathingNodeScript.m_pathingSplineDetails[splineIndex].m_nodePrimary);
                EditorGUILayout.LabelField("Secondary Node: " + m_pathingNodeScript.m_pathingSplineDetails[splineIndex].m_nodeSecondary);

                //Options for spline usage
                m_pathingNodeScript.m_pathingSplineDetails[splineIndex].m_splineType = (Pathing_Spline.SPLINE_TYPE)EditorGUILayout.EnumPopup("Spline Type", m_pathingNodeScript.m_pathingSplineDetails[splineIndex].m_splineType);

                switch (m_pathingNodeScript.m_pathingSplineDetails[splineIndex].m_splineType)
                {
                    case Pathing_Spline.SPLINE_TYPE.STRAIGHT:
                        break;
                    case Pathing_Spline.SPLINE_TYPE.BEZIER:
                        SerializedProperty bezierStrengthSP = m_splineDetailsSP.GetArrayElementAtIndex(splineIndex).FindPropertyRelative("m_bezierStrength");
                        EditorGUILayout.PropertyField(bezierStrengthSP);
                        break;
                    case Pathing_Spline.SPLINE_TYPE.CIRCLE:
                        SerializedProperty circleDirSP = m_splineDetailsSP.GetArrayElementAtIndex(splineIndex).FindPropertyRelative("m_circleDir");
                        EditorGUILayout.PropertyField(circleDirSP);
                        SerializedProperty circleAngleSP = m_splineDetailsSP.GetArrayElementAtIndex(splineIndex).FindPropertyRelative("m_circleAngle");
                        EditorGUILayout.PropertyField(circleAngleSP);
                        break;
                    case Pathing_Spline.SPLINE_TYPE.NOT_IN_USE:
                        break;
                    default:
                        break;
                }

                //Apply to other node
                Pathing_Node conjoinedNode = null;
                if (m_pathingNodeScript.m_pathingSplineDetails[splineIndex].m_nodePrimary == m_pathingNodeScript)
                {
                    conjoinedNode = m_pathingNodeScript.m_pathingSplineDetails[splineIndex].m_nodeSecondary;
                }
                else
                {
                    conjoinedNode = m_pathingNodeScript.m_pathingSplineDetails[splineIndex].m_nodePrimary;
                }

                //Something broke
                if(conjoinedNode == null || conjoinedNode.DetermineNodePosition(m_pathingNodeScript) == Pathing_Spline.SPLINE_POSITION.MAX_LENGTH)
                {
                    m_pathingNodeScript.RemoveDetailsAt((Pathing_Spline.SPLINE_POSITION)splineIndex);
                }
                else
                {
                    Pathing_Spline.SPLINE_POSITION conjoinedPosition = conjoinedNode.DetermineNodePosition(m_pathingNodeScript);

                    conjoinedNode.m_pathingSplineDetails[(int)conjoinedPosition] = m_pathingNodeScript.m_pathingSplineDetails[splineIndex];

                    if (GUILayout.Button("Remove Spline"))
                    {
                        m_pathingNodeScript.RemoveDetailsAt((Pathing_Spline.SPLINE_POSITION)splineIndex);
                        conjoinedNode.RemoveDetailsAt(conjoinedPosition);
                    }

                    EditorUtility.SetDirty(m_pathingNodeScript);
                    EditorUtility.SetDirty(conjoinedNode);
                }
            }
            else
            {
                SerializedProperty conjoinedSplineSP = m_conjoinedSplineSP.GetArrayElementAtIndex(splineIndex);
                EditorGUILayout.PropertyField(conjoinedSplineSP, new GUIContent("Conjoined Node", "Node that this position will join to"));
                m_pathingNodeScript.m_conjoinedPosition[splineIndex] = (Pathing_Spline.SPLINE_POSITION)EditorGUILayout.EnumPopup(new GUIContent("Conjoined Position", "What position in conjoined node will this spline connect to"), m_pathingNodeScript.m_conjoinedPosition[splineIndex]);

                if (GUILayout.Button("Create Spline")) //Attemp to make spline
                {
                    Pathing_Node conjoinedNode = m_pathingNodeScript.m_conjoinedNodes[splineIndex];
                    Pathing_Spline.SPLINE_POSITION conjoinedPosition = m_pathingNodeScript.m_conjoinedPosition[splineIndex];

                    if (conjoinedNode != null && conjoinedPosition != Pathing_Spline.SPLINE_POSITION.MAX_LENGTH)
                    {
                        //Clean up old data
                        m_pathingNodeScript.RemoveDetailsAt((Pathing_Spline.SPLINE_POSITION)splineIndex);
                        conjoinedNode.RemoveDetailsAt(conjoinedPosition);

                        //Build new details data
                        Pathing_Node.Spline_Details newDetails = new Pathing_Node.Spline_Details(m_pathingNodeScript, conjoinedNode, m_pathingNodeScript.m_pathingSplineDetails[splineIndex]);

                        m_pathingNodeScript.m_pathingSplineDetails[splineIndex] = newDetails;
                        conjoinedNode.m_pathingSplineDetails[(int)conjoinedPosition] = newDetails;

                        EditorUtility.SetDirty(m_pathingNodeScript);
                        EditorUtility.SetDirty(conjoinedNode);
                    }
                }
            }

            //Apply changes
            serializedObject.ApplyModifiedProperties();
        }
    }
}
