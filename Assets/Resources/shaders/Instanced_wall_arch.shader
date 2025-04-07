Shader "Custom/SimpleColorRandom"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _ColorVariation ("Variation", Range(0,1)) = 0.5
        _RandomSeed ("Random Seed", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 _BaseColor;
            float _ColorVariation;
            float _RandomSeed;

            float4 vert(float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            float rand(float x)
            {
                return frac(sin(x * 12.9898 + 78.233) * 43758.5453);
            }

            float4 frag() : SV_Target
            {
                float3 randomColor = float3(rand(_RandomSeed), rand(_RandomSeed + 1.0), rand(_RandomSeed + 2.0));
                float3 finalColor = lerp(_BaseColor.rgb, randomColor, _ColorVariation);
                return float4(finalColor, 1.0);
            }

            ENDCG
        }
    }

    FallBack Off
}




// Shader "Custom/Instanced_wall_arch" 
// {
    // Properties 
    // {
        // _Color ("Color", Color) = (1,1,1,1) // couleur de base de la brique 
        // _MainTex ("Albedo (RGB)", 2D) = "white" {} // texture de base (optionnelle) 
        // _Glossiness ("Smoothness", Range(0,1)) = 0.5 // lissage spéculaire 
        // _Metallic ("Metallic", Range(0,1)) = 0.0 // métallique
        // _TopColor ("Top Color", Color) = (0.9, 0.8, 0.7, 1) // couleur des parties hautes
        // _NoiseScale ("Noise Scale", Float) = 4.0 // échelle du bruit
        // _HeightBlend ("Height Blend", Float) = 2.0 // intensité de mélange selon la hauteur
    // }

    // SubShader
    // {
        // Tags { "RenderType"="Opaque" } // opaque pour ombrage standard
        // LOD 200 // niveau de détail

        // CGPROGRAM
        // #pragma surface surf Standard fullforwardshadows // modèle d'éclairage standard
        // #pragma target 3.0 // shader model 3.0 requis

        // sampler2D _MainTex; // texture principale

        // struct Input
        // {
            // float2 uv_MainTex; // coordonnées UV
            // float3 worldPos; // position monde du pixel
        // };

        // half _Glossiness; // lissage
        // half _Metallic; // métal
        // fixed4 _Color; // couleur principale
        // fixed4 _TopColor; // couleur haute
        // float _NoiseScale; // échelle du bruit
        // float _HeightBlend; // contrôle du mélange vertical

        // UNITY_INSTANCING_BUFFER_START(Props)
            // paramètres instanciés par objet (si besoin)
        // UNITY_INSTANCING_BUFFER_END(Props)

        // float hash(float2 p)
        // {
            // return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); // bruit simple
        // }

        // void surf (Input IN, inout SurfaceOutputStandard o)
        // {
            // float2 p = IN.worldPos.xz * _NoiseScale; // position pour bruit
            // float noise = hash(p); // bruit aléatoire
            // float t = saturate(IN.worldPos.y / _HeightBlend + noise * 0.2); // mix hauteur + bruit
            // fixed4 col = lerp(_Color, _TopColor, t); // interpolation des couleurs
            // fixed4 tex = tex2D(_MainTex, IN.uv_MainTex); // texture
            // col *= tex; // multiplication couleur * texture
            // o.Albedo = col.rgb; // couleur finale
            // o.Metallic = _Metallic; // métallique
            // o.Smoothness = _Glossiness; // lissage
            // o.Alpha = col.a; // transparence (inutile ici mais bon)
        // }
        // ENDCG
    // }

    // FallBack "Diffuse" // fallback standard
// }
