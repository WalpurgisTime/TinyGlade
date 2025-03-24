Shader "Custom/RoadShader"
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

            struct appdata
            {
                float4 vertex : POSITION;
                float3 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 bbx_bounds : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            // === UTILITAIRES ===
            float sample_terrain_texture_ws(float2 pos_ws)
            {
                float2 uv = pos_ws / 20.0 + 0.5;
                return tex2Dlod(_TerrainTex, float4(uv, 0, 0)).r;
            }

            float sample_path_texture_ws(float2 pos_ws)
            {
                float2 uv = pos_ws / 20.0 + 0.5;
                return tex2Dlod(_PathTex, float4(uv, 0, 0)).r;
            }

            float random_f(float x)
            {
                return frac(sin(x * 12.9898) * 43758.5453);
            }

            float fit01(float x, float min, float max)
            {
                return x * (max - min) + min;
            }

            // === VERTEX ===
            v2f vert(appdata v)
            {
                v2f o;
                float3 pos_ws = mul(unity_ObjectToWorld, v.vertex).xyz;

                float2 bbx_min = v.bbx_bounds.xy;
                float2 bbx_max = v.bbx_bounds.zw;

                // === SAMPLE AROUND THE BOUNDING BOX CORNERS ===
                float avg_value = 0.0;
                avg_value += sample_path_texture_ws(bbx_min);
                avg_value += sample_path_texture_ws(float2(bbx_max.x, bbx_min.y));
                avg_value += sample_path_texture_ws(float2(bbx_min.x, bbx_max.y));
                avg_value += sample_path_texture_ws(bbx_max);
                avg_value /= 4.0;

                float seed = bbx_min.x + bbx_min.y + bbx_max.x + bbx_max.y;
                float threshold = fit01(random_f(seed), 0.2, 0.4);

                if (avg_value > threshold)
                {
                    pos_ws.y = sample_terrain_texture_ws(pos_ws.xz) + 0.01;
                }
                else
                {
                    pos_ws = float3(0.0, 0.0, 0.0);
                }

                float random_color = fit01(random_f(seed + 50.0), 0.086, 0.14);
                float h = sample_terrain_texture_ws(v.vertex.xz) + 0.4;
                h = fit01(h * h * sign(h), 0.1, 3.0);
                h = clamp(h, 0.1, 0.8);

                o.pos = UnityObjectToClipPos(float4(pos_ws, 1.0));
                o.color = float3(random_color, random_color, random_color) * fit01(h, 0.0, 2.0);
                o.color.b *= 1.0 - h;
                o.uv = v.uv;

                return o;
            }

            // === FRAGMENT ===
            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(i.color, 1.0);
            }

            ENDCG
        }
    }
}
