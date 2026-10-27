
Shader "ShieldEffect/HexShield"
{
    Properties
    {
        [Header(Appearance)]
        [HDR] _BaseColor("Base Color", Color) = (1, 0.85, 0.1, 1)
        _GlowIntensity("Glow Intensity", Float) = 2
        _FresnelPower("Fresnel Power", Range(0.1, 10)) = 3

        [Header(Hex Grid)]
        _HexScale("Hex Scale", Float) = 10
        _LineWidth("Line Width", Range(0, 0.5)) = 0.05
        _ScrollSpeed("Scroll Speed", Float) = 0.1

        [Header(Intersection)]
        _IntersectionRange("Intersection Range", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend One One
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD3;
            };

            fixed4 _BaseColor;
            float _HexScale;
            float _LineWidth;
            float _GlowIntensity;
            float _FresnelPower;
            float _IntersectionRange;
            float _ScrollSpeed;

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            float hexDist(float2 p)
            {
                p = abs(p);
                float r = dot(p, normalize(float2(1, 1.73205)));
                return max(r, p.x);
            }

            float2 myMod(float2 x, float2 y)
            {
                return x - y * floor(x / y);
            }

            float hexGrid(float2 uv)
            {
                float2 r = float2(1, 1.73205);
                float2 h = r * 0.5;
                float2 a = myMod(uv, r) - h;
                float2 b = myMod(uv - h, r) - h;
                float2 g = dot(a, a) < dot(b, b) ? a : b;
                return hexDist(g);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);
                COMPUTE_EYEDEPTH(o.screenPos.z);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float3 normal = normalize(i.worldNormal);

                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), _FresnelPower);

                float2 movingUV = i.uv * _HexScale;
                movingUV.y += _Time.y * _ScrollSpeed;

                float hDist = hexGrid(movingUV);
                float gridLines = smoothstep(0.5 - _LineWidth, 0.52 - _LineWidth, hDist);

                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
                float sceneEyeDepth = LinearEyeDepth(rawDepth);
                float surfaceEyeDepth = i.screenPos.z;

                float intersection = 1.0 - saturate(
                    (sceneEyeDepth - surfaceEyeDepth) / max(_IntersectionRange, 0.0001)
                );
                intersection = pow(intersection, 2.0);

                float alpha = saturate(gridLines * fresnel + intersection + gridLines * 0.2);
                float3 finalColor = _BaseColor.rgb * alpha * _GlowIntensity;

                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }

    FallBack Off
}
