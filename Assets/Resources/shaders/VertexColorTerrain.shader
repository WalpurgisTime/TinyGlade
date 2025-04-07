Shader "Custom/VertexColorTerrain"
{
    Properties
    {
        _TerrainTex("Terrain Texture", 2D) = "white" {}
        _DigitIntensity("Digit Brightness", Range(1.0, 5.0)) = 3.91
        _ShadowRadius("Radial Shadow Radius", Float) = 6.0
        _ShadowSmoothness("Shadow Smoothness", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _TerrainTex;
            float _DigitIntensity;
            float _ShadowRadius;
            float _ShadowSmoothness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 pos_ws : TEXCOORD0;
                float3 color : COLOR;
            };

            float sample_terrain_texture_ws(float2 pos_ws)
            {
                float2 uv = pos_ws / 20.0 + 0.5;
                return tex2Dlod(_TerrainTex, float4(uv, 0, 0)).r;
            }

            v2f vert(appdata v)
            {
                v2f o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // Rotation X 45°
                float angle = radians(45.0);
                float cosA = cos(angle);
                float sinA = sin(angle);

                float3 rotatedPos;
                rotatedPos.y = worldPos.y * cosA - worldPos.z * sinA;
                rotatedPos.z = worldPos.y * sinA + worldPos.z * cosA;
                rotatedPos.x = worldPos.x;

                worldPos = rotatedPos;

                float terrainHeight = sample_terrain_texture_ws(worldPos.xz);
                float isDigit = step(0.1, 1.0 - distance(v.color, float3(1, 0, 0)));
                float elevation = isDigit * 0.05;

                worldPos.y = terrainHeight + elevation;

                o.pos = UnityObjectToClipPos(float4(worldPos, 1.0));
                o.pos_ws = worldPos.xz;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 baseGray = float3(0.120741, 0.120741, 0.120741);

                bool isDigit = distance(i.color, float3(1, 0, 0)) < 0.1;

                float3 color;
                if (isDigit)
                {
                    color = float3(1, 1, 1) * _DigitIntensity;
                }
                else
                {
                    // Couleur terrain de base
                    color = i.color;

                    // Dégradé radial : clair au centre, sombre à l'extérieur
                    float dist = length(i.pos_ws); // distance au centre (0,0)
                    float fade = smoothstep(_ShadowRadius - _ShadowSmoothness, _ShadowRadius, dist);
                    color = lerp(color, baseGray * 0.5, fade); // sombre aux bords
                }

                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}
