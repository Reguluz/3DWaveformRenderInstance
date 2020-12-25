Shader "Custom/PointSurfaceGPU"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
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

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Smoothness;
        half _Metallic;
        fixed4 _Color;
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
				unity_ObjectToWorld._m00_m11_m22 = float3(1, position.y, 1);
                _Color = fixed4(position.yy,1,1);
			#endif
        }

		void ConfigureSurface (Input input, inout SurfaceOutputStandard surface) {
			surface.Albedo = saturate(input.worldPos * 0.5 + 0.5);
			surface.Smoothness = _Smoothness;
		}

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
