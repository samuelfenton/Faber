using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : NavigationControl
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

            //Rotational Perpendicular Vector
            Vector3 rotationVector = new Vector3(-towertoCharacterVector.z, 0.0f, towertoCharacterVector.x);

            character.transform.position = characterPosition;
            character.transform.rotation = Quaternion.LookRotation(rotationVector, Vector3.up);
        }
	}

    public override void AddCharacter(Character p_character)
    {
        if (!m_currentCharacters.Contains(p_character))
            m_currentCharacters.Add(p_character);
    }

    public override void RemoveCharacter(Character p_character, NavigationTrigger p_navigationTrigger)
    {
        base.RemoveCharacter(p_character, p_navigationTrigger);

        //Reset rotation of character
        p_character.transform.rotation = Quaternion.Euler(p_navigationTrigger.m_globalExitRotation);
    }
}
