Shader "Custom/vertex_color_indirect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 color : COLOR;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 color : COLOR;
            };

            StructuredBuffer<float4x4> transforms;

            float random(int p)
            {
                return frac(sin(dot(float(p), 311.7)) * 43758.5453);
            }

            float fit01(float x, float min, float max)
            {
                return x * (max - min) + min;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                float4x4 instance_transform = transforms[v.instanceID];

                float4 transformed_pos = mul(instance_transform, v.vertex);
                float4 world_pos = mul(unity_ObjectToWorld, transformed_pos);
                o.positionCS = UnityObjectToClipPos(world_pos);

                float r = fit01(random(v.instanceID), 0.1, 0.5);
                o.color = float3(r, r, r);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(i.color, 1.0);
            }
            ENDCG
        }
    }
}

