Shader "Custom/InstancedWall"
{
    Properties
    {
        _MainTex ("Albedo Texture", 2D) = "white" {}
        _ComputeTexture ("Compute Texture", 2D) = "black" {}
        _CameraPosition ("Camera Position", Vector) = (0,0,0)
        _IsArch ("Is Arch", Float) = 0
        _WallLength ("Wall Length", Float) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300
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
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 color : TEXCOORD2;
                float3 curvePosWS : TEXCOORD3;
                uint instanceID : TEXCOORD4;
            };

            struct InstancedWallData {
                float4x4 transform;
                float4 curve_uv_bbx_minmax;
            };

            StructuredBuffer<InstancedWallData> instances;

            float _WallLength;
            float4 _CameraPosition;
            float _IsArch;
            sampler2D _ComputeTexture;

            float random_f(float x)
            {
                return frac(sin(x * 12.9898) * 43758.5453);
            }

            float fit(float x, float from_min, float from_max, float to_min, float to_max)
            {
                float x_01 = (x - from_min) / (from_max - from_min);
                return x_01 * (to_max - to_min) + to_min;
            }

            float2 local_to_curve_space(float2 local, float4 uv_bbx_bounds)
            {
                float2 bbx_min = uv_bbx_bounds.xy;
                float2 bbx_max = uv_bbx_bounds.zw;

                float u = fit(local.x, -0.5, 0.5, bbx_min.x, bbx_max.x);
                float v = fit(local.y, -0.5, 0.5, bbx_min.y, bbx_max.y);

                return float2(u, v);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                float SEED = 112.0;

                InstancedWallData instance = instances[v.instanceID];

                float2 uv_cs = local_to_curve_space(v.vertex.xy, instance.curve_uv_bbx_minmax);
                float4 vertex_ws = mul(instance.transform, float4(v.vertex.xyz, 1.0));

                float row_bby_cs = local_to_curve_space(float2(v.vertex.x, v.vertex.y < 0.0 ? -0.5 : 0.5), instance.curve_uv_bbx_minmax).y;
                row_bby_cs /= 1.6;

                float3 final_p = vertex_ws.xyz;
                if (uv_cs.y > 0.1)
                {
                    float r;
                    if (uv_cs.y > 1.05)
                    {
                        r = float(v.instanceID);
                        r = random_f(r + SEED);
                    }
                    else
                    {
                        r = int(floor(row_bby_cs * 100.0));
                        r = random_f(r + SEED);
                    }

                    float freq = fit(random_f(r), 0.01, 0.5, 2.0, 10.0);
                    float rand_offset = random_f(r * 2.0) * 1000.0;
                    float str = fit(random_f(r * r + 88.0), 0.015, 0.045, 0.02, 0.05);

                    float2 vertex_cs = uv_cs * _WallLength;
                    float sin_wave = sin(vertex_cs.x * freq + rand_offset) * str;

                    final_p.y += sin_wave;
                }

                o.positionWS = final_p;
                o.positionCS = UnityObjectToClipPos(float4(final_p, 1.0));
                o.normalWS = normalize(mul((float3x3)instance.transform, v.normal));
                o.curvePosWS = mul(instance.transform, float4(v.vertex.xy, 0.0, 1.0)).xyz;
                o.instanceID = v.instanceID;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 lightPos = float3(4.0, 8.0, 4.0);
                float lightIntensity = 200.0;
                float3 lightDir = normalize(lightPos - i.positionWS);
                float3 viewDir = normalize(_CameraPosition.xyz - i.positionWS);

                float3 N = normalize(i.normalWS);
                float3 color = saturate(dot(N, lightDir));

                if (_IsArch < 0.5)
                {
                    float2 uv = (i.curvePosWS.xz / 20.0) + 0.5;
                    float textureColor = tex2D(_ComputeTexture, uv).r;

                    if (textureColor > 0.01 && i.curvePosWS.y < 1.0 - exp(-5.0 * textureColor))
                    {
                        clip(-1.0);
                    }
                }

