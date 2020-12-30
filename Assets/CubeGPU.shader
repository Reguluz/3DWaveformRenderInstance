Shader "Custom/CubeGPU"
{
    Properties
    {
        _GradingTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.5

        sampler2D _GradingTex;

        struct Input
        {
            float2 uv_GradingTex;
            float3 worldPos;
        };

        half _Smoothness;
        half _Metallic;
        float4 _Color;
        float _Step;

    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        struct WaveformBlock
        {
            float worldPosY;
            float3 color;
        };

		StructuredBuffer<float3> _WaveformOutputs;
	#endif
        void ConfigureProcedural ()
        {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				float3 position = _WaveformOutputs[unity_InstanceID]/*.worldPosY*/;
                unity_ObjectToWorld = 0.0;
				unity_ObjectToWorld._m03_m13_m23_m33 = float4(0, 0, position.z,1.0);
				unity_ObjectToWorld._m00_m11_m22 = float3(0.1, position.y * 50, 0.1);
                _Color = tex2D (_GradingTex, float2(position.y,0.5))*2;
			#endif
        }

		void ConfigureSurface (Input input, inout SurfaceOutputStandard surface) {
			surface.Albedo = _Color;
			surface.Smoothness = 0;
            surface.Metallic = 0;
		}

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

     
        ENDCG
    }
    FallBack "Diffuse"
}
