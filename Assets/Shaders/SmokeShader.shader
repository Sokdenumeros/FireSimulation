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
        BlendOp Add
        Blend SrcColor OneMinusSrcAlpha
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
            StructuredBuffer<float> opacitybuffer;
            StructuredBuffer<float> temperaturebuffer;
            int offset;
            float4 camposition;

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
                    float3 forward = normalize(camposition - positionbuffer[id]);
                    float3 right = cross(forward, float3(0,1,0));
                    float3 up = cross(right, forward);
                    float3x3 rotationMatrix = float3x3(right, up, forward);
                    //vert = mul(unity_ObjectToWorld, float4(mul(rotationMatrix,v.vertex.xyz),v.vertex.w)) + float4(positionbuffer[id], 0);
                    vert += float4(positionbuffer[id], 0);
                #endif
                o.vertex = mul(UNITY_MATRIX_VP,vert);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                //col = fixed4(1,1,1,1);
                //col.w = col.x; col.x = 1; col.y = 1; col.z = 1;
                #ifdef INSTANCING_ON
                    int id = i.instanceID + offset;
                    col = col * colorbuffer[id];
                #endif
                return col;
            }
            ENDCG
        }
    }
}


