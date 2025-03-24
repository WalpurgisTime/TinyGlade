Shader "Custom/Shadow 2"
{
    Properties
    {
        _ShadowTexture ("Shadow Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            sampler2D _ShadowTexture;

            float sample_texture_ws(float2 pos_ws)
            {
                float2 texture_uv = (pos_ws / 20.0) + 0.5;
                return tex2Dlod(_ShadowTexture, float4(texture_uv, 0, 0)).r;
            }

            float fit01(float x, float min, float max)
            {
                return x * (max - min) + min;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.positionCS = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float v = 1.0 - max(fit01(i.uv.y, -0.3, 1.0), 0.0);

                float road_value = sample_texture_ws(i.positionWS.xz);
                v = v * (1.0 - pow(min(road_value * 2.0, 1.0), 2.0));

                float alpha = pow(v, 3.0) * 0.7;
                return fixed4(0.0, 0.0, 0.0, alpha);
            }
            ENDCG
        }
    }
}
