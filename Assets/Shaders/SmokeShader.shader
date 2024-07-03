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
        LOD 100

        Pass
        {
            Cull off
            CGPROGRAM
            #pragma vertex vert alpha
            #pragma fragment frag alpha
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            StructuredBuffer<float4> colorbuffer;
            StructuredBuffer<float3> positionbuffer;
            int offset;
            int nparts;
            float particleSize;
            int order;
            float4 fw;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 pcolor : PARTICLE_COLOR;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v, uint svInstanceID : SV_InstanceID)
            {
                v2f o;
                float4 vert = v.vertex;
                vert = mul(unity_ObjectToWorld, vert);

                int id = GetIndirectInstanceID(svInstanceID);
                if(order == 1) id = nparts - id;

                float3 forward = fw.xyz;
                float3 right = normalize(cross(forward, float3(0,1,0)));
                float3 up = normalize(cross(right, forward));
                float3x3 rotationMatrix = float3x3(right, up, forward);

                vert = float4(mul(v.vertex.xyz * particleSize,rotationMatrix),v.vertex.w) + float4(positionbuffer[id], 0);
                
                o.vertex = mul(UNITY_MATRIX_VP,vert);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.pcolor = colorbuffer[id];
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 col = tex2D(_MainTex, i.uv);
                return col*i.pcolor;
            }
            ENDCG
        }
    }
}


