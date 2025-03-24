Shader "Custom/Shadow 1"
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            sampler2D _TerrainTexture;

            float sample_terrain_texture_ws(float2 pos_ws)
            {
                float2 texture_uv = (pos_ws / 20.0) + 0.5;
                return tex2Dlod(_TerrainTexture, float4(texture_uv, 0, 0)).r;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                float3 pos_ws = mul(unity_ObjectToWorld, v.vertex).xyz;
                pos_ws.y = sample_terrain_texture_ws(pos_ws.xz) + 0.02;

                o.positionCS = UnityObjectToClipPos(float4(pos_ws, 1.0));
                o.uv = v.uv;
                o.positionWS = pos_ws;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0.0, 0.0, 0.0, 1.0);
            }
            ENDCG
        }
    }
}
