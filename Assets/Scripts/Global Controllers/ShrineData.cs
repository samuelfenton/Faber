using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrineData : MonoBehaviour
{
    public struct ShrineDetails
    {
        public MasterController.SCENE m_scene;
        public int m_shrineID;
        public string m_shrineName;

        public ShrineDetails(MasterController.SCENE p_scene, int p_shrineID, string p_shrineName)
        {
            m_scene = p_scene;
            m_shrineID = p_shrineID;
            m_shrineName = p_shrineName;
        }
    };

    public ShrineDetails[] m_shrineDetails = new ShrineDetails[]
    {
        new ShrineDetails(MasterController.SCENE.LEVEL_GREYBOX1, 0, "New Game"),
        new ShrineDetails(MasterController.SCENE.LEVEL_GREYBOX1, 1, "Tower")
    };

    public void BuildShrineDetail()
    {

    }
}
