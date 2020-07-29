using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[ExecuteAlways]
public class Texture2DtoPNG : MonoBehaviour
{
    public bool m_convert = false;
    public Texture2D m_textureToConvert = null;

    private void Update()
    {
        if(m_convert)
        {
            m_convert = false;

            if(m_textureToConvert != null)
            {
                byte[] _bytes = m_textureToConvert.EncodeToPNG();

                var dirPath = Application.dataPath + "/ConvertedPNGs/";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                File.WriteAllBytes(dirPath + m_textureToConvert.name + ".png", _bytes);

                AssetDatabase.Refresh();
            }
        }
    }
}
