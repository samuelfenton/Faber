using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerTrigger : MonoBehaviour
{
    //Determines the plane of collision
    public Vector3 m_globalEntranceVector = Vector3.zero;
    private Vector4 m_planeEquation = Vector4.zero;

    public Vector3 m_globalExitRotationVector = Vector3.zero;

    private Tower m_parentTower = null;

    private void Start()
    {
        Vector3 triggerPos = transform.position;
        float planeZ = (-triggerPos.x * m_globalEntranceVector.x) + (-triggerPos.y * m_globalEntranceVector.y) + (-triggerPos.z * m_globalEntranceVector.z);

        m_planeEquation = new Vector4(m_globalEntranceVector.x, m_globalEntranceVector.y, m_globalEntranceVector.z, planeZ);

        m_parentTower = transform.parent.GetComponent<Tower>();

#if UNITY_EDITOR
        if (m_parentTower == null)
            Debug.Log("Tower trigger has no parent");
#endif
    }
    
    private void OnTriggerStay(Collider other)
    {
        //Compare dot products of character and entrance vector to see if character is entering or leaving
        Character collidingCharacter = other.GetComponent<Character>();
        if (collidingCharacter != null)
        {
            //Determine what side of the plane the character is
            // Using the equation ax + by + cz + d where a,b,c,d are determiened from the plane equation
            Vector3 characterPosition = collidingCharacter.transform.position;
            float expandedDot = m_planeEquation.x * characterPosition.x + m_planeEquation.y * characterPosition.y + m_planeEquation.z * characterPosition.z + m_planeEquation.w;

            if (expandedDot > 0) //Entering
            {
                m_parentTower.AddCharacter(collidingCharacter);
            }
            else //Exiting
            {
                m_parentTower.RemoveCharacter(collidingCharacter, this);
            }
        }
    }
}
