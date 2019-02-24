using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorDouble
{
    public double m_x = 0.0;
    public double m_y = 0.0;
    public double m_z = 0.0;

    public VectorDouble(double p_x = 0.0, double p_y = 0.0, double p_z = 0.0)
    {
        m_x = p_x;
        m_y = p_y;
        m_z = p_z;
    }
    public static bool operator ==(VectorDouble p_lhs, VectorDouble p_rhs)
    {
        if (ReferenceEquals(p_lhs, null))
            return ReferenceEquals(p_rhs, null);
        if (ReferenceEquals(p_rhs, null))
            return ReferenceEquals(p_lhs, null);

        return p_lhs.m_x == p_rhs.m_x && p_lhs.m_y == p_rhs.m_y && p_lhs.m_z == p_rhs.m_z;
    }

    public static bool operator !=(VectorDouble p_lhs, VectorDouble p_rhs)
    {
        if (ReferenceEquals(p_lhs, null))
            return !ReferenceEquals(p_rhs, null);
        if (ReferenceEquals(p_rhs, null))
            return !ReferenceEquals(p_lhs, null);

        return p_lhs.m_x != p_rhs.m_x || p_lhs.m_y != p_rhs.m_y || p_lhs.m_z != p_rhs.m_z;
    }

    public static VectorDouble operator +(VectorDouble p_lhs, VectorDouble p_rhs)
    {
        return new VectorDouble(p_lhs.m_x + p_rhs.m_x, p_lhs.m_y + p_rhs.m_y, p_lhs.m_z + p_rhs.m_z);
    }
    public static VectorDouble operator -(VectorDouble p_lhs, VectorDouble p_rhs)
    {
        return new VectorDouble(p_lhs.m_x - p_rhs.m_x, p_lhs.m_y - p_rhs.m_y, p_lhs.m_z - p_rhs.m_z);
    }

    public static Vector3 GetVector3(VectorDouble p_val)
    {
        return new Vector3((float)(p_val.m_x), (float)(p_val.m_y), (float)(p_val.m_z));
    }

    public static VectorDouble GetVectorDouble(Vector3 p_val)
    {
        return new VectorDouble((double)(p_val.x), (double)(p_val.y), (double)(p_val.z));
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        VectorDouble vectorDouble = obj as VectorDouble;
        return vectorDouble != null && vectorDouble.m_x == m_x && vectorDouble.m_y == m_y && vectorDouble.m_z == m_z;
    }

    public override int GetHashCode()
    {
        return m_x.GetHashCode() ^ m_y.GetHashCode() ^ m_z.GetHashCode();
    }
}
