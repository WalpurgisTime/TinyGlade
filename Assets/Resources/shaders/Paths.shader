Shader "Custom/Paths"
{
        Properties
    {
        _TerrainTex ("Terrain Texture", 2D) = "white" {}
        _PathTex ("Path Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _TerrainTex;
            sampler2D _PathTex;

            float4x4 UNITY_MATRIX_M;
            float4x4 UNITY_MATRIX_V;
            float4x4 UNITY_MATRIX_P;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 bbx_bounds : TEXCOORD1; // custom bounding box
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            float SampleTerrainTextureWS(float2 pos_ws)
            {
                float2 uv = (pos_ws / 20.0 + 0.5);
                return tex2D(_TerrainTex, uv).r;
            }

            float SamplePathTextureWS(float2 pos_ws)
            {
                float2 uv = (pos_ws / 20.0 + 0.5);
                return tex2D(_PathTex, uv).r;
            }

            float RandomF(float x)
            {
                return frac(sin(x * 12.9898) * 43758.5453);
            }

            float Fit01(float x, float min, float max)
            {
                return x * (max - min) + min;
            }

            v2f vert(appdata v)
            {
                v2f o;

                float3 pos_ws = mul(UNITY_MATRIX_M, v.vertex).xyz;
                float2 bbx_min = v.bbx_bounds.xy;
                float2 bbx_max = v.bbx_bounds.zw;

                float avg = 0.0;
                avg += SamplePathTextureWS(bbx_min);
                avg += SamplePathTextureWS(float2(bbx_max.x, bbx_min.y));
                avg += SamplePathTextureWS(float2(bbx_min.x, bbx_max.y));
                avg += SamplePathTextureWS(bbx_max);
                avg /= 4.0;

                float seed = bbx_min.x + bbx_min.y + bbx_max.x + bbx_max.y;
                float threshold = RandomF(seed);
                threshold = Fit01(threshold, 0.2, 0.4);

                if (avg > threshold)
                {
                    pos_ws.y = SampleTerrainTextureWS(pos_ws.xz) + 0.01;
                }
                else
                {
                    pos_ws = float3(0,0,0);
                }

                float random_color = RandomF(seed + 50.0);
                random_color = Fit01(random_color, 0.086, 0.14);

                float h = SampleTerrainTextureWS(v.vertex.xz) + 0.4;
                h = Fit01(h * h * sign(h), 0.1, 3.0);
                h = clamp(h, 0.1, 0.8);

                o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(pos_ws, 1.0)));
                o.color = float3(random_color, random_color, random_color) * Fit01(h, 0.0, 2.0);
                o.color.b *= (1.0 - h);
                o.uv = v.uv;
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