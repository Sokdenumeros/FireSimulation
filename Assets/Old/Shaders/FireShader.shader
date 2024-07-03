// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/FireShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Additive" "PreviewType"="Plane"}
        ZWrite Off
        BlendOp Add
        Blend SrcColor One
        Cull Off
        LOD 100

        Pass
        {
            Cull off
            CGPROGRAM
            #pragma vertex vert alpha
            #pragma fragment frag alpha
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            StructuredBuffer<float4> colorbuffer;
            StructuredBuffer<float3> positionbuffer;
            int offset;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                float4 vert = v.vertex;
                vert = mul(unity_ObjectToWorld, vert);
                #ifdef INSTANCING_ON
                    int id = v.instanceID + offset;
                    vert += float4(positionbuffer[id], 0);
                #endif
                o.vertex = mul(UNITY_MATRIX_VP, vert);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                #ifdef INSTANCING_ON
                    int id = i.instanceID;
                    col = col * colorbuffer[id];
                #endif
                return col;
            }
            ENDCG
        }
    }
}


