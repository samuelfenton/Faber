Shader "Custom/SurfaceShader_Fading"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_PlayerWorldPostion("Player World Postion", Vector) = (0,0,0,1)
    }
    SubShader
    { 
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent"}
        LOD 200

		Pass
		{
			ZWrite On
			ColorMask 0
		}

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert alpha fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		float3 _PlayerWorldPostion;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            
			float3 cameraToPlayer = _PlayerWorldPostion - _WorldSpaceCameraPos;
			//cameraToPlayer.y = 0;
			float3 playerToFrag = _PlayerWorldPostion - IN.worldPos;
			//playerToFrag.y = 0;
			float cameraToObjectDot = dot(normalize(cameraToPlayer), normalize(playerToFrag));

			//Only obscure objects which are between player and camera and higher than player, e.g dont obscure ground.
			o.Alpha = cameraToObjectDot >= 0.6 && playerToFrag.y < -0.1 ? 0.3 : 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
