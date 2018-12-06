Shader "Custom/FadingShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Transparancy("Transparancy", Float) = 0.5
		_BaseColor("Base Colour", Color) = (1,1,1,1)
		_PlayerWorldPostion("Player World Postion", Vector) = (0,0,0,1)
	}
	SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 position : SV_POSITION;
				float4 worldPosition : TEXCOORD0;
				float2 UV : TEXCOORD1;
			};

			sampler2D _MainTex;
			float _Transparancy;
			vector _PlayerWorldPostion;
			fixed4 _BaseColor;
			
			v2f vert (float4 p_vertex : POSITION, float3 p_normal : NORMAL, float2 p_UV : TEXCOORD0)
			{
				v2f o;
				o.position = UnityObjectToClipPos(p_vertex);
				o.worldPosition = mul(unity_ObjectToWorld, p_vertex);
				o.UV = p_UV;
				return o;
			}
			
			fixed4 frag (v2f fragIn) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, fragIn.UV) * _BaseColor;
				float3 cameraToPlayer = _PlayerWorldPostion - _WorldSpaceCameraPos;
				cameraToPlayer.y = 0;
				float3 playerToFrag = _PlayerWorldPostion - fragIn.worldPosition;
				playerToFrag.y = 0;
				color.a = dot(normalize(cameraToPlayer), normalize(playerToFrag)) >= 0.2 ? _Transparancy : 1;
				return color;
			}
			ENDCG
		}
	}
}
