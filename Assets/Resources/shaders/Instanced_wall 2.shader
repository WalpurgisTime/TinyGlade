Shader "Custom/Instanced_wall 2"
{
    Properties
    {
        _MainTex ("Albedo Texture", 2D) = "white" {}
        _ComputeTexture ("Compute Texture", 2D) = "black" {}
        _CameraPosition ("Camera Position", Vector) = (0,0,0)
        _IsArch ("Is Arch", Float) = 0
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

            sampler2D _ComputeTexture;
            float4 _CameraPosition;
            float _IsArch;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.positionCS = UnityObjectToClipPos(v.vertex);
                o.positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normalWS = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
                o.curvePosWS = o.positionWS; 
                o.instanceID = v.instanceID;
                return o;
            }

            float arch_function(float h)
            {
                return 1.0 - exp(-5.0 * h);
            }

            float3 calculateLighting(float3 normal, float3 viewDir, float3 lightDir)
            {
                float3 H = normalize(lightDir + viewDir);
                float NoL = saturate(dot(normal, lightDir));
                float NoH = saturate(dot(normal, H));
                float LoH = saturate(dot(lightDir, H));

                float3 specular = 0.16 * NoH;
                float3 diffuse = NoL;
                return diffuse + specular;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 lightPos = float3(4.0, 8.0, 4.0);
                float lightIntensity = 200.0;
                float3 lightDir = normalize(lightPos - i.positionWS);
                float3 viewDir = normalize(_CameraPosition.xyz - i.positionWS);

                float3 N = normalize(i.normalWS);
                float3 color = calculateLighting(N, viewDir, lightDir);

                if (_IsArch < 0.5)
                {
                    float2 uv = (i.curvePosWS.xz / 20.0) + 0.5;
                    float textureColor = tex2D(_ComputeTexture, uv).r;

                    float heightThreshold = arch_function(textureColor);

                    if (textureColor > 0.01 && i.curvePosWS.y < heightThreshold)
                    {
                        clip(-1.0); // discard
                    }
                }

                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}
