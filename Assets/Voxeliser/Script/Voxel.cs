using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    protected static Vector3 POOLING_LOCATION = new Vector3(0.0f, -2000, 0.0f);

    protected VoxeliserHandler m_parentVoxeliser = null;

    private float m_randomExplosionFactor = 1.0f;

    protected Vector3 m_scale = Vector3.zero;
    protected Vector3 m_localOffset = Vector3.zero;

    protected Vector3 m_velocity = Vector3.zero;
    protected float m_shrinkStep = 0.1f;

    [HideInInspector]
    public Material m_voxelMaterial = null;

    //--------------------
    //  Initialise voxel varibles defined by parent voxel handler
    //  params:
    //      p_scale - Scale of voxel
    //      p_shrinkTime - How long for voxels to shrink after explosion
    //      p_explosionFactor - The force applied to voxels when exploding.
    //      p_parentVoxeliser - Parent voxeliser handler 
    //--------------------
    public void InitVoxel(float p_scale, float p_shrinkTime, float p_explosionFactor, VoxeliserHandler p_parentVoxeliser)
    {
        m_scale = new Vector3(p_scale, p_scale, p_scale);
        m_shrinkStep = p_scale / p_shrinkTime;
        m_parentVoxeliser = p_parentVoxeliser;
        m_voxelMaterial = GetComponent<MeshRenderer>().material;
        m_randomExplosionFactor = Random.Range(0.0f, p_explosionFactor);

        ResetVoxel();
    }

    //--------------------
    //  Setup voxel for use once "added" to mesh
    //  params:
    //      p_localOffset - Offset from parent
    //      p_color - color of voxel to set material
    //--------------------
    public void SetupVoxel(Vector3 p_localOffset, Vector3 p_color)
    {
        transform.localPosition = p_localOffset;
        transform.rotation = Quaternion.identity; // Always stay at no rotation

        Color voxelColor = new Color
        {
            r = p_color.x,
            g = p_color.y,
            b = p_color.z
        };
        m_voxelMaterial.color = voxelColor;
    }

    //--------------------
    //  Reset scale and location of voxel when not in use.
    //--------------------
    public virtual void ResetVoxel()
    {
        transform.position = POOLING_LOCATION;
        transform.localScale = m_scale;
        transform.rotation = Quaternion.identity; // Always stay at no rotation
    }

    //--------------------
    //  Physics effects to voxel
    //  Only applies movement and gravity
    //--------------------
    protected IEnumerator PhysicsEffect()
    {
        yield return null;
        
        //Add gravity 
        m_velocity.y += -9.8f * Time.deltaTime;
        //TODO random velocity away, looks more explosive
        transform.Translate(m_velocity * Time.deltaTime, Space.World);

        StartCoroutine(PhysicsEffect());
    }

    //--------------------
    //  Shrink Effect to voxel
    //  Once too small, destory voxel as no longer needed
    //--------------------
    protected IEnumerator ShrinkEffect()
    {
        yield return null;

        float newScale = transform.localScale.x - m_shrinkStep * Time.deltaTime;
        transform.localScale = new Vector3(newScale, newScale, newScale);
        if (newScale < 0.01f)//Finished floating away
            Destroy(this.gameObject);
        else
            StartCoroutine(ShrinkEffect());
    }

    //--------------------
    //  Begin shrinking effect
    //--------------------
    public void ApplyShrink()
    {
        StartCoroutine(ShrinkEffect());
    }

    //--------------------
    //  Begin physics effect
    //  Set explosion velocity 
    //--------------------
    public void ApplyPhysics()
    {
        m_velocity = m_randomExplosionFactor * (transform.position - m_parentVoxeliser.transform.position).normalized;
        StartCoroutine(PhysicsEffect());
    }
}
