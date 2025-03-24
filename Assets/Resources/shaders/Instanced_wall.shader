Shader "Unlit/Instanced_wall"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _TerrainTex ("Terrain Texture", 2D) = "white" {}
        _ComputeTexture ("Compute Texture", 2D) = "white" {}
        _WallLength ("Wall Length", Float) = 1.0
        _IsArch ("Is Arch", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float3 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 color : COLOR;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 curvePos : TEXCOORD3;
                float instanceID : TEXCOORD4;
            };

            sampler2D _MainTex;
            sampler2D _TerrainTex;
            sampler2D _ComputeTexture;
            float4 _Color;
            float _WallLength;
            float _IsArch;

            struct InstancedWallData
            {
                float4x4 transform;
                float4 curve_uv_bbx_minmax;
            };

            StructuredBuffer<InstancedWallData> _InstancedWallData;

            float sample_terrain_texture_ws(float2 pos_ws)
            {
                float2 texture_uv = (pos_ws / 20.0 + 0.5);
                return tex2D(_TerrainTex, texture_uv).r;
            }

            float randomf(float x)
            {
                return frac(sin(x * 12.9898) * 43758.5453);
            }

            float random( int p ) 
            {
               return fract(sin(dot(float(p), 311.7)) * 43758.5453);
            }

            float fit01(float x, float min, float max)
            {
                return x * (max - min) + min;
            }

            float fit(float x, float from_min, float from_max, float to_min, float to_max)
            {
                float x_01 = (x - from_min) / (from_max - from_min);
                return fit01(x_01, to_min, to_max);
            }

            float2 local_to_curve_space(float2 local, float4 uv_bbx_bounds)
            {
                float2 bbx_min = uv_bbx_bounds.xy;
                float2 bbx_max = uv_bbx_bounds.zw;
                float u = fit(local.x, -0.5, 0.5, bbx_min.x, bbx_max.x);
                float v = fit(local.y, -0.5, 0.5, bbx_min.y, bbx_max.y);
                return float2(u, v);
            }

            float pow5(float x)
            {
                float x2 = x * x;
                return x2 * x2 * x;
            }

            float D_GGX(float roughness, float NoH, float3 h)
            {
                float oneMinusNoHSquared = 1.0 - NoH * NoH;
                float a = NoH * roughness;
                float k = roughness / (oneMinusNoHSquared + a * a);
                float d = k * k * (1.0 / 3.141592653589793);
                return d;
            }

            float getDistanceAttenuation(float3 posToLight, float inverseRadiusSquared)
            {
                float distanceSquare = dot(posToLight, posToLight);
                float factor = distanceSquare * inverseRadiusSquared;
                float smoothFactor = saturate(1.0 - factor * factor);
                float attenuation = smoothFactor * smoothFactor;
                return attenuation * 1.0 / max(distanceSquare, 1e-4);
            }

            float V_SmithGGXCorrelated(float roughness, float NoV, float NoL)
            {
                float a2 = roughness * roughness;
                float lambdaV = NoL * sqrt((NoV - a2 * NoV) * NoV + a2);
                float lambdaL = NoV * sqrt((NoL - a2 * NoL) * NoL + a2);
                float v = 0.5 / (lambdaV + lambdaL);
                return v;
            }

            float3 F_Schlick(float3 f0, float f90, float VoH)
            {
                return f0 + (f90 - f0) * pow5(1.0 - VoH);
            }

            float F_Schlick(float f0, float f90, float VoH)
            {
                return f0 + (f90 - f0) * pow5(1.0 - VoH);
            }

            float3 fresnel(float3 f0, float LoH)
            {
                float f90 = saturate(dot(f0, float3(50.0 * 0.33)));
                return F_Schlick(f0, f90, LoH);
            }

            float3 specular(float3 f0, float roughness, float3 h, float NoV, float NoL, float NoH, float LoH)
            {
                float D = D_GGX(roughness, NoH, h);
                float V = V_SmithGGXCorrelated(roughness, NoV, NoL);
                float3 F = fresnel(f0, LoH);
                return (D * V) * F;
            }

            float Fd_Burley(float roughness, float NoV, float NoL, float LoH)
            {
                float f90 = 0.5 + 2.0 * roughness * LoH * LoH;
                float lightScatter = F_Schlick(1.0, f90, NoL);
                float viewScatter = F_Schlick(1.0, f90, NoV);
                return lightScatter * viewScatter * (1.0 / 3.141592653589793);
            }

            float3 EnvBRDFApprox(float3 f0, float perceptual_roughness, float NoV)
            {
                float4 c0 = float4(-1, -0.0275, -0.572, 0.022);
                float4 c1 = float4(1, 0.0425, 1.04, -0.04);
                float4 r = perceptual_roughness * c0 + c1;
                float a004 = min(r.x * r.x, exp2(-9.28 * NoV)) * r.x + r.y;
                float2 AB = float2(-1.04, 1.04) * a004 + r.zw;
                return f0 * AB.x + AB.y;
            }

            float perceptualRoughnessToRoughness(float perceptualRoughness)
            {
                float clampedPerceptualRoughness = clamp(perceptualRoughness, 0.089, 1.0);
                return clampedPerceptualRoughness * clampedPerceptualRoughness;
            }

            float luminance(float3 v)
            {
                return dot(v, float3(0.2126, 0.7152, 0.0722));
            }

            float3 change_luminance(float3 c_in, float l_out)
            {
                float l_in = luminance(c_in);
                return c_in * (l_out / l_in);
            }

            float3 reinhard_luminance(float3 color)
            {
                float l_old = luminance(color);
                float l_new = l_old / (1.0 + l_old);
                return change_luminance(color, l_new);
            }

            float arch_function(float h)
            {
                return 1.0 - exp(-5.0 * h);
            }

            float nrand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898, 78.233))) * 43758.5453);
            }

            float inv_error_function(float x)
            {
                float y = log(1.0 - x * x);
                float z = 1.41421356 / 0.14 + 0.5 * y;
                return sqrt(sqrt(z * z - y * (1.0 / 0.14)) - z) * sign(x);
            }

            float gaussian_rand(float2 n, float seed)
            {
                float t = frac(seed);
                float x = nrand(n + 0.07 * t);
                return inv_error_function(x * 2.0 - 1.0) * 0.15 + 0.5;
            }

            v2f vert(appdata_t v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                float SEED = 112.0;
                float4x4 instance_transform = _InstancedWallData[instanceID].transform;
                float2 uv_cs = local_to_curve_space(v.vertex.xy, _InstancedWallData[instanceID].curve_uv_bbx_minmax);
                float4 vertex_ws = mul(unity_ObjectToWorld, mul(instance_transform, float4(v.vertex.xyz, 1.0)));

                float row_bby_ms = v.vertex.y < 0.0 ? -0.5 : 0.5;
                float row_bby_cs = local_to_curve_space(float2(v.vertex.x, row_bby_ms), _InstancedWallData[instanceID].curve_uv_bbx_minmax).y;
                row_bby_cs /= 1.6;

                float3 p = vertex_ws.xyz;
                float3 final_p = p;

                if (uv_cs.y > 0.1)
                {
                    float r = uv_cs.y > 1.05 ? randomf(float(instanceID) + SEED) : randomf(int(floor(row_bby_cs * 100.0)) + SEED);
                    float freq = fit01(r, 0.01, 0.5) * 10.0;
                    float rand_offset = randomf(r * 2.0) * 1000.0;
                    float str = fit01(randomf(r * r + 88.0), 0.015, 0.045);
                    float2 vertex_cs = uv_cs * _WallLength;
                    float sin_wave = sin(vertex_cs.x * freq + rand_offset) * str;
                    final_p = float3(p.x, p.y + sin_wave, p.z);
                }

                const float WALL_HEIGHT = 1.4;
                float height_u = final_p.y / WALL_HEIGHT * 0.7;
                float3 terrain_p = final_p;
                terrain_p.y += sample_terrain_texture_ws(final_p.xz);
                final_p = lerp(terrain_p, final_p, height_u);

                vertex_ws = float4(final_p, 1.0);
                o.pos = UnityObjectToClipPos(vertex_ws);
                o.uv = v.uv;
                o.color = v.color;
                o.worldPos = vertex_ws.xyz;
                o.worldNormal = mul((float3x3)instance_transform, v.normal);
                o.curvePos = mul(instance_transform, float4(float3(v.vertex.xy, 0.0), 1.0)).xyz;
                o.instanceID = instanceID;
                return o;
            }

           fixed4 frag(v2f i) : SV_Target
           {
               vec3 light_pos = vec3(4.0, 8.0, 4.0);
               vec4 light_color = vec4(1.0);
               float light_intensity = 200.0;
               float light_radius = 20.0;
               float perceptual_roughness = 0.9;
               float roughness = perceptualRoughnessToRoughness(perceptual_roughness);
               float metallic = 0.0;
               float reflectance = 0.1;

               float r = gaussian_rand(vec2(i.instanceID + 4), 0);
               r = clamp(r, 0.2, 1.0);
               r = fit01(r, 0.1, 0.35);
               vec4 output_color = vec4(vec3(r), 1.0);

               vec3 N = normalize(i.worldNormal);
               vec3 V = normalize(camera_position - i.worldPos);
               float NdotV = max(dot(N, V), 1e-4);

               vec3 diffuseColor = output_color.rgb * (1.0 - metallic);

               vec3 light_accum = vec3(0.0);

               vec3 lightDir = light_pos - i.worldPos;
               vec3 L = normalize(lightDir);

               float inverseRadiusSquared = (1.0 / light_radius) * (1.0 / light_radius);
               float rangeAttenuation = getDistanceAttenuation(lightDir, inverseRadiusSquared) * light_intensity;

               vec3 H = normalize(L + V);
               float NoL = saturate(dot(N, L));
               float NoH = saturate(dot(N, H));
               float LoH = saturate(dot(L, H));

               vec3 F0 = 0.16 * reflectance * reflectance * (1.0 - metallic) + output_color.rgb * metallic;

               vec3 specular = specular(F0, roughness, H, NdotV, NoL, NoH, LoH);
               vec3 diffuse = diffuseColor * Fd_Burley(roughness, NdotV, NoL, LoH);

               light_accum += ((diffuse + specular) * light_color.rgb) * (rangeAttenuation * NoL * 1.2);

               vec3 diffuse_ambient = EnvBRDFApprox(diffuseColor, 1.0, NdotV) * pow((1.0 - NoL), 5.0) * 5.0;
               vec3 specular_ambient = EnvBRDFApprox(F0, perceptual_roughness, NdotV);

               output_color.rgb = light_accum;
               output_color.rgb += (diffuse_ambient + specular_ambient) * 0.075;

               if (_IsArch == 0.0)
               {
                  vec2 texture_uv = (i.curvePos.xz / 20.0 + 0.5);
                  float texture_color = tex2D(_ComputeTexture, texture_uv).r;

                  float height_threshold = arch_function(texture_color);

                  if (texture_color > 0.01 && i.curvePos.y < height_threshold) {discard; }
                }

                return output_color * _Color;
           }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
