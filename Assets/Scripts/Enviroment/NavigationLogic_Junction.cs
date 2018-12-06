using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationLogic_Junction : NavigationLogic
{
    public float m_junctionRadius = 4.0f;

    private struct Pathway
    {
        public NavigationController.FACING_DIR m_startingDir;
        public NavigationController.FACING_DIR m_endingDir;
        public Transform m_pivot;
    }

    private List<Pathway> m_characterPathways = new List<Pathway>();

    public NavigationTrigger m_northTrigger = null;
    public NavigationTrigger m_eastTrigger = null;
    public NavigationTrigger m_southTrigger = null;
    public NavigationTrigger m_westTrigger = null;

    private NavigationTrigger[] m_navigationTrigger;

    public Transform m_northEastPivot = null;
    public Transform m_southEastPivot = null;
    public Transform m_southWestPivot = null;
    public Transform m_northWestPivot = null;

    private List<int> m_confirmedPath = new List<int>();

    private void Start()
    {
        m_navigationTrigger = new NavigationTrigger[4];

        m_navigationTrigger[0] = m_northTrigger;
        m_navigationTrigger[1] = m_eastTrigger;
        m_navigationTrigger[2] = m_southTrigger;
        m_navigationTrigger[3] = m_westTrigger;

        for (int i = 0; i < 4; i++)
        {
            if (m_navigationTrigger[i] != null)
            {
                m_confirmedPath.Add(i);
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < m_currentCharacters.Count; i++)
        {
            //Position is fixed distance from the tower
            //Rotation vector will be the perpendicular vector of the vector from the tower to character
            Vector3 characterPosition = m_currentCharacters[i].transform.position;
            Vector3 pivotPosition = m_characterPathways[i].m_pivot.transform.position;
            //Tower to character
            Vector3 pivotToCharacterVector = characterPosition - pivotPosition;
            pivotToCharacterVector.y = 0;

            pivotToCharacterVector = pivotToCharacterVector.normalized * m_junctionRadius;

            characterPosition.x = pivotPosition.x + pivotToCharacterVector.x;
            characterPosition.z = pivotPosition.z + pivotToCharacterVector.z;

            m_currentCharacters[i].transform.position = characterPosition;
            
            //Rotational Perpendicular Vector
            Vector3 rotationVector = new Vector3(-pivotToCharacterVector.z, 0.0f, pivotToCharacterVector.x);

            float dotRotationToForward = Vector3.Dot(rotationVector.normalized, m_currentCharacters[i].transform.forward);
            if(dotRotationToForward >= 0 )
                m_currentCharacters[i].transform.rotation = Quaternion.LookRotation(rotationVector, Vector3.up);
            else
                m_currentCharacters[i].transform.rotation = Quaternion.LookRotation(-rotationVector, Vector3.up);
        }
    }

    public override void AddCharacter(Character p_character)
    {
        if (!m_currentCharacters.Contains(p_character))
        {
            Pathway newPathway = new Pathway();
            newPathway.m_startingDir = NavigationController.GetFacingDir(p_character.m_characterModel.transform.forward);
            newPathway.m_endingDir = p_character.GetDesiredDirection();

            //Check if pathway is closed, defualt to forwards
            if (m_navigationTrigger[(int)newPathway.m_endingDir] == null)
            {
                if (m_navigationTrigger[(int)newPathway.m_startingDir] == null) //Forwards is also closed, defualt to confirmed path
                {
                    if ((int)newPathway.m_startingDir == m_confirmedPath[0]) //Is first defualt path where I just came from? grab second defualt path
                        newPathway.m_endingDir = (NavigationController.FACING_DIR)m_confirmedPath[1];
                    else
                        newPathway.m_endingDir = (NavigationController.FACING_DIR)m_confirmedPath[0]; //Otherwise, first defualt is good to go
                }
                else
                {
                    newPathway.m_endingDir = newPathway.m_startingDir;//Forwards wasnt closed
                }
            }

            //Path is made, get pivot
            if (newPathway.m_startingDir == newPathway.m_endingDir)//just moving forwards, dont need to add character to junction list
            {
                return;
            }

            GetPivot(ref newPathway);

            //logic needs to be added
            m_currentCharacters.Add(p_character);
            m_characterPathways.Add(newPathway);
        }
    }

    private void GetPivot(ref Pathway p_pathway)
    {
        int pivotIndex = (int)p_pathway.m_startingDir - (int)(p_pathway.m_endingDir - p_pathway.m_startingDir);
        switch (pivotIndex)
        {
            case 3:
            case 6:
                p_pathway.m_pivot = m_northEastPivot;
                return;
            case -1:
            case 4:
                p_pathway.m_pivot = m_southEastPivot;
                return;
            case -3:
            case 0:
                p_pathway.m_pivot = m_southWestPivot;
                return;
            case 1:
            case 2:
                p_pathway.m_pivot = m_northWestPivot;
                return;
            default:
                break;
        }
    }

    public override void RemoveCharacter(Character p_character, NavigationTrigger p_navigationTrigger)
    {
        if (m_currentCharacters.Contains(p_character))
        {
            int index = m_currentCharacters.IndexOf(p_character);

            m_currentCharacters.RemoveAt(index);
            m_characterPathways.RemoveAt(index);

            ClampRotationOnExit(p_character);
        }
    }

}
