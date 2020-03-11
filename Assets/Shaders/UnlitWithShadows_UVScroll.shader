// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/UnlitWithShadows_UVScroll"
{
	Properties
	{
		_MainTex("Diffuse", 2D) = "white" {}
		_UVScrollSpeed("UVScroll Speed", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}
 
		Pass
		{
			Tags{ "RenderType" = "Opaque" "LightMode" = "ForwardBase"}
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

				struct v2f
				{
					float4	pos			: SV_POSITION;
					float2	uv			: TEXCOORD0;
					LIGHTING_COORDS(1,2)
				};

				float4 _MainTex_ST;
				sampler2D _MainTex;
				uniform float _UVScrollSpeed;

				v2f vert (appdata_tan v)
				{
					v2f o;
					
					o.pos = UnityObjectToClipPos( v.vertex);
					o.uv = TRANSFORM_TEX (v.texcoord, _MainTex).xy;
					TRANSFER_VERTEX_TO_FRAGMENT(o);
					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{
					float mulTime12 = _Time.y * _UVScrollSpeed;
					float2 panner1 = ( mulTime12 * float2( 1,0 ) + i.uv);
					float3 col = tex2D( _MainTex, panner1 ).rgb;

					fixed atten = LIGHT_ATTENUATION(i);
					return float4(col, 1) * atten;
				}

			ENDCG
		}
	}
	Fallback "Diffuse"
}