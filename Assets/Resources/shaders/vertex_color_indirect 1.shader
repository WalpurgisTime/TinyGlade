Shader "Custom/vertex_color_indirect 1"
{
    Properties
    {
        _TerrainTexture ("Terrain Texture", 2D) = "black" {}
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
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 color : COLOR;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 color : COLOR;
            };

            sampler2D _TerrainTexture;

            float sample_terrain_texture_ws(float2 pos_ws)
            {
                float2 texture_uv = (pos_ws / 20.0) + 0.5;
                return tex2Dlod(_TerrainTexture, float4(texture_uv, 0, 0)).r;
            }

            float fit01(float x, float min, float max)
            {
                return x * (max - min) + min;
            }

            v2f vert(appdata_t v)
            {
                v2f o;

                // Récupération de la hauteur du terrain
                float h = sample_terrain_texture_ws(v.vertex.xz) + 0.5;
                h = fit01(h, 0.0, 1.0);
                h = clamp(h, 0.15, 1.0);

                // Falloff basé sur la distance
                float f = length(v.vertex / 10.0);
                f = pow(f, 3.0);
                f = smoothstep(0.0, 1.0, f);

                // Ajustement de la position Y
                float3 pos_ws = v.vertex.xyz;
                pos_ws.y = sample_terrain_texture_ws(v.vertex.xz);

                o.positionCS = UnityObjectToClipPos(float4(pos_ws, 1.0));

                // Mixage de la couleur du vertex et d’une teinte fixe
                float3 fixed_color = float3(0.120741, 0.120741, 0.120741);
                o.color = lerp(v.color * h, fixed_color, f);

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
