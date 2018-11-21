using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationTrigger : MonoBehaviour {

    //Determines the plane of collision
    public Vector3 m_globalEntranceVector = Vector3.zero;
    protected Vector4 m_planeEquation = Vector4.zero;

    public Vector3 m_globalExitRotation = Vector3.zero;

    protected NavigationControl m_navigationController = null;

    protected void Start()
    {
        m_globalEntranceVector = transform.forward;

        Vector3 triggerPos = transform.position;
        float planeZ = (-triggerPos.x * m_globalEntranceVector.x) + (-triggerPos.y * m_globalEntranceVector.y) + (-triggerPos.z * m_globalEntranceVector.z);

        m_planeEquation = new Vector4(m_globalEntranceVector.x, m_globalEntranceVector.y, m_globalEntranceVector.z, planeZ);

        m_navigationController = transform.parent.GetComponent<NavigationControl>();

#if UNITY_EDITOR
        if (m_navigationController == null)
            Debug.Log("Trigger has no parent");
#endif
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
                m_navigationController.AddCharacter(collidingCharacter);
            }
            else //Exiting
            {
                m_navigationController.RemoveCharacter(collidingCharacter, this);
            }
        }
    }
}
