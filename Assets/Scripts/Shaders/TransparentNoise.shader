Shader "Custom/TransparentNoise"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _TransparentMap("TransparentMap", 2D) = "white" {}
        _NoiseMap("NoiseMap", 2D) = "white" {}
        _NoiseModifier("Noise Modifier", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        CGPROGRAM
        #pragma surface surf Lambert alpha
        #pragma target 3.0

        fixed4 _BaseColor;
        sampler2D _TransparentMap;
        sampler2D _NoiseMap;
        float _NoiseModifier;

        struct Input
        {
            float2 uv_MainTex;
        };

        float Fixed4ToAlpha(fixed4 p_val)
        {
            return (p_val.x + p_val.y + p_val.z) / 3.0f;//3*1 values
        }

        float GetAlpha(float2 p_UV)
        {
            fixed4 transparentColor = tex2D(_TransparentMap, p_UV);
            fixed4 noiseColor = tex2D(_NoiseMap, p_UV);

            float NoiseVal = (Fixed4ToAlpha(noiseColor) * 2.0f - 1.0f) * _NoiseModifier; //Change range from 0.0f->1.0f to -1.0f->1.0f

            return clamp(Fixed4ToAlpha(transparentColor) + NoiseVal, 0.0f, 1.0f);
        }

        void surf(Input IN, inout SurfaceOutput o) 
        {
            fixed4 transparentColor = tex2D(_TransparentMap, IN.uv_MainTex);

            o.Albedo = _BaseColor.rgb;
            o.Alpha = GetAlpha(IN.uv_MainTex);
        }

        ENDCG
    }
    FallBack "Diffuse"
}
