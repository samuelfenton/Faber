using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class Navigation_Trigger : MonoBehaviour
{
    static Vector3 triggerSize = new Vector3(1,5,0);

    //Determines the plane of collision
    protected Vector3 m_globalEntranceVector = Vector3.zero;
    protected Vector4 m_planeEquation = Vector4.zero;

    protected void SetupBoxCollider()
    {
        BoxCollider boxCol = GetComponent<BoxCollider>();

        boxCol.size = triggerSize;
        boxCol.center = new Vector3(0, triggerSize.y/2, 0);
    }

    protected virtual void Start()
    {
        m_globalEntranceVector = transform.forward;

        Vector3 triggerPos = transform.position;
        float planeZ = (-triggerPos.x * m_globalEntranceVector.x) + (-triggerPos.y * m_globalEntranceVector.y) + (-triggerPos.z * m_globalEntranceVector.z);

        m_planeEquation = new Vector4(m_globalEntranceVector.x, m_globalEntranceVector.y, m_globalEntranceVector.z, planeZ);
    }

    protected void OnTriggerStay(Collider p_other)
    {
        //Compare dot products of character and entrance vector to see if character is entering or leaving
        Character collidingCharacter = p_other.GetComponent<Character>();
        if (collidingCharacter != null)
        {
            //Determine what side of the plane the character is
            // Using the equation ax + by + cz + d where a,b,c,d are determiened from the plane equation
            Vector3 characterPosition = collidingCharacter.transform.position;
            float expandedDot = m_planeEquation.x * characterPosition.x + m_planeEquation.y * characterPosition.y + m_planeEquation.z * characterPosition.z + m_planeEquation.w;

            if (expandedDot >= 0) //Entering
            {
                HandleTrigger(collidingCharacter, TRIGGER_DIRECTION.ENTERING);
            }
            else//Exiting
            {
                HandleTrigger(collidingCharacter, TRIGGER_DIRECTION.EXITING);
            }
        }
    }

    public enum TRIGGER_DIRECTION {ENTERING, EXITING }
    protected virtual void HandleTrigger(Character p_character, TRIGGER_DIRECTION p_direction)
    {

    }

#if UNITY_EDITOR
    private void Update()
    {
        SetupBoxCollider();
    }
#endif

}
