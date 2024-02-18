// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/SmokeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Additive" "PreviewType"="Plane"}
        ZWrite Off
        BlendOp RevSub
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

            StructuredBuffer<float3> positionbuffer;
            StructuredBuffer<float> opacitybuffer;
            StructuredBuffer<float> temperaturebuffer;
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
                    vert += float4(positionbuffer[id], 0) + float4(1,0,0,0);
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
                    float3 pos = positionbuffer[id] * 100;
                    pos = max(pos, float3(0, 0, 0));
                    pos = min(pos, float3(61, 121, 61));
                    int positionIndex = (int)pos.z * 121 * 61 + (int)pos.y * 61 + (int)pos.x;
                    float opacity = opacitybuffer[positionIndex];// / 100.0 * 0.2;
                    //if(opacity== 0)opacity += 0.05;

                    col = col * fixed4(opacity, opacity, opacity,0);
                #endif
                return col;
            }
            ENDCG
        }
    }
}


