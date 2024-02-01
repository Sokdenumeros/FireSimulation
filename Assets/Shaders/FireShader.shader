// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/FireShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Cull off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                vert /= 10;
                vert.w *= 10;
                vert = mul(unity_ObjectToWorld, vert);
                #ifdef INSTANCING_ON
                    int id = v.instanceID + offset;
                    //vert += float4(positionbuffer[v.instanceID], 0);
                    //unity_ObjectToWorld._14_24_34_44 = float4(positionbuffer[v.instanceID], 1);
                    //vert += float4(v.instanceID*0.001,0,0, 0);
                    vert += float4(positionbuffer[id], 0);
                //vert += float4(float3(id / 61 / 61 * 0.1, ((id / 61) % 61) * 0.1, (id % 61) * 0.1), 0);
                #endif
                //vert = mul(unity_ObjectToWorld, vert);
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


