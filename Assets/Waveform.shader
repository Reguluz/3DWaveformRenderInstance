Shader "Unlit/Waveform"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma target 4.5
            #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            struct WaveformBlock
            {
                float worldPosY;
                float3 color;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _Color;

       
            void ConfigureProcedural ()
            {
                #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				    float position = _WaveformBlocks[unity_InstanceID].worldPosY;
                    unity_ObjectToWorld = 0.0;
				    unity_ObjectToWorld._m03_m13_m23_m33 = float4(0, position,  _WaveformBlocks[unity_InstanceID].color.x * 5, 1.0);
				    unity_ObjectToWorld._m00_m11_m22 = 1;
                    _Color = _WaveformBlocks[unity_InstanceID].color;
			    #endif
            }
            UNITY_INSTANCING_BUFFER_START(Props)
        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			StructuredBuffer<WaveformBlock> _WaveformBlocks;
		#endif
            UNITY_INSTANCING_BUFFER_END(Props)
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                // return col;
                return half4(_Color,1);
            }
            ENDCG
        }
    }
}
