using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_NPC : Character
{
    [Header("NPC Logic")]
    public List<Navigation_Spline> m_path = new List<Navigation_Spline>();

    protected override void Start()
    {
        base.Start();
    }
}
