using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationLogic_Tower : NavigationLogic
{
    public float m_towerRadius = 4.0f;

    private void Update ()
    {
        foreach (Character character in m_currentCharacters)
        {
            //Position is fixed distance from the tower
            //Rotation vector will be the perpendicular vector of the vector from the tower to character

            Vector3 characterPosition = character.transform.position;

            //Tower to character
            Vector3 towertoCharacterVector = characterPosition - transform.position;
            towertoCharacterVector.y = 0;

            towertoCharacterVector = towertoCharacterVector.normalized * m_towerRadius;

            characterPosition.x = transform.position.x + towertoCharacterVector.x;
            characterPosition.z = transform.position.z + towertoCharacterVector.z;

            character.transform.position = characterPosition;

            //Rotational Perpendicular Vector
            Vector3 rotationVector = new Vector3(-towertoCharacterVector.z, 0.0f, towertoCharacterVector.x);

            float dotRotationToForward = Vector3.Dot(rotationVector.normalized, character.transform.forward);
            if (dotRotationToForward >= 0)
                character.transform.rotation = Quaternion.LookRotation(rotationVector, Vector3.up);
            else
                character.transform.rotation = Quaternion.LookRotation(-rotationVector, Vector3.up);
        }
	}

    public override void RemoveCharacter(Character p_character, NavigationTrigger p_navigationTrigger)
    {
        if (m_currentCharacters.Contains(p_character))
        {
            m_currentCharacters.Remove(p_character);

            //Reset rotation of character
            ClampRotationOnExit(p_character);
        }
    }
}
