Shader "Custom/shadow"
{
    Properties
    {
        _MainTex ("SDF Texture", 2D) = "white" {}
        _TerrainTex ("Terrain Height Texture", 2D) = "gray" {}
        _AlphaStrength ("Alpha Strength", Range(0,1)) = 0.7
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _TerrainTex;
            float4 _MainTex_ST;
            float _AlphaStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float3 ws  : TEXCOORD1;
            };

            float sampleTerrainHeight(float2 ws)
            {
                float2 uv = ws / 20.0 + 0.5;
                return tex2D(_TerrainTex, uv).r;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 world = mul(unity_ObjectToWorld, v.vertex).xyz;
                world.y = sampleTerrainHeight(world.xz) + 0.02;

                o.pos = UnityObjectToClipPos(float4(world, 1.0));
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.ws  = world;

                return o;
            }

            float fit01(float x, float min, float max)
            {
                return saturate((x - min) / (max - min));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float v = 1.0 - max(fit01(i.uv.y, -0.3, 1.0), 0.0);

                float2 sdfUV = i.ws.xz / 20.0 + 0.5;
                float roadVal = tex2D(_MainTex, sdfUV).r;

                v *= (1.0 - saturate(roadVal * 2.0));
                float alpha = pow(v, 3.0);

                return float4(0, 0, 0, alpha * _AlphaStrength);
            }
            ENDCG
        }
    }
}
