Shader "Custom/RoadShader"
{
    Properties
    {
        _TerrainTex ("Terrain Texture", 2D) = "white" {}
        _PathTex ("Path Texture", 2D) = "black" {}
        _IsRoad ("Is Road", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _TerrainTex;
            sampler2D _PathTex;
            float _IsRoad;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 bbx_bounds : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR; // couleur + alpha
                float2 uv : TEXCOORD0;
            };

            float sample_terrain_texture_ws(float2 pos_ws)
            {
                float2 uv = saturate(pos_ws / 20.0 + 0.5);
                return tex2Dlod(_TerrainTex, float4(uv, 0, 0)).r;
            }

            float sample_path_texture_ws(float2 pos_ws)
            {
                float2 uv = saturate(pos_ws / 20.0 + 0.5);
                return tex2Dlod(_PathTex, float4(uv, 0, 0)).r;
            }

            float random_f(float x)
            {
                return frac(sin(x * 12.9898) * 43758.5453);
            }

            float fit01(float x, float minVal, float maxVal)
            {
                return x * (maxVal - minVal) + minVal;
            }

            v2f vert(appdata v)
            {
                v2f o;
                float3 pos_ws = mul(unity_ObjectToWorld, v.vertex).xyz;

                // ✅ Appliquer la hauteur depuis la texture du terrain
                float terrainHeight = sample_terrain_texture_ws(pos_ws.xz);
                pos_ws.y = terrainHeight;

                // ✅ Appliquer une rotation autour de l'axe X sur pos_ws
                float angle = radians(45);
                float cosA = cos(angle);
                float sinA = sin(angle);

                float3 rotatedPos;
                rotatedPos.y = pos_ws.y * cosA - pos_ws.z * sinA;
                rotatedPos.z = pos_ws.y * sinA + pos_ws.z * cosA;
                rotatedPos.x = pos_ws.x;

                pos_ws = rotatedPos;

                // ✅ Lecture de la texture de chemin avec Z inversé
                float2 flipped = float2(pos_ws.x, pos_ws.z);
                float path_value = sample_path_texture_ws(flipped);

                // ✅ Apparence de la pierre (bruit + intensité selon la hauteur)
                float seed = v.vertex.x + v.vertex.z;
                float random_color = fit01(random_f(seed + 50.0), 0.086, 0.14);
                float h = terrainHeight + 0.4;
                h = fit01(h * h * sign(h), 0.1, 3.0);
                h = clamp(h, 0.1, 0.8);

                float3 col = float3(random_color, random_color, random_color) * fit01(h, 0.0, 2.0);
                col.b *= 1.0 - h;

                float alpha = (path_value < 0.5) ? 0.0 : 1.0;

                // ✅ Envoi vers fragment shader
                o.pos = UnityObjectToClipPos(float4(pos_ws, 1.0));
                o.color = float4(col, alpha);
                o.uv = v.uv;
                return o;
            }

            // ✅ Appliquer alpha pour transparence
            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(i.color.rgb, i.color.a);
            }
            ENDCG
        }
    }
}
