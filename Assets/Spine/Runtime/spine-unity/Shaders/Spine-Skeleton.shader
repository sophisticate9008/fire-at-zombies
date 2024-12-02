Shader "Spine/Skeleton" {
    Properties {
        _Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
        [NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
        [Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
        [HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

        // Outline properties
        [Toggle(_ENABLE_OUTLINE)] _EnableOutline("Enable Outline", Float) = 0
        _OutlineWidth("Outline Width", Range(0,8)) = 3.0
        _OutlineColor("Outline Color", Color) = (1,0,0,1)
        _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
        _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
        _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
    }

    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

        Fog { Mode Off }
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Lighting Off

        Stencil {
            Ref[_StencilRef]
            Comp[_StencilComp]
            Pass Keep
        }

        Pass {
            Name "Normal"

            CGPROGRAM
            #pragma shader_feature _ENABLE_OUTLINE
            #pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _OutlineColor;
            float _ThresholdEnd;
            float _OutlineSmoothness;
            float _OutlineWidth;
            int _OutlineReferenceTexWidth;

            struct VertexInput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vertexColor = v.vertexColor;
                return o;
            }

            float4 frag (VertexOutput i) : SV_Target {
                float4 texColor = tex2D(_MainTex, i.uv);

                #if defined(_STRAIGHT_ALPHA_INPUT)
                texColor.rgb *= texColor.a;
                #endif

                // Check if outline is enabled
                #if defined(_ENABLE_OUTLINE)
                    // Edge detection
                    float2 texelSize = float2(_OutlineWidth / _OutlineReferenceTexWidth, _OutlineWidth / _OutlineReferenceTexWidth);
                    float4 texLeft = tex2D(_MainTex, i.uv + float2(-texelSize.x, 0));
                    float4 texRight = tex2D(_MainTex, i.uv + float2(texelSize.x, 0));
                    float4 texUp = tex2D(_MainTex, i.uv + float2(0, texelSize.y));
                    float4 texDown = tex2D(_MainTex, i.uv + float2(0, -texelSize.y));

                    float edge = abs(texLeft.a - texRight.a) + abs(texUp.a - texDown.a);

                    // Apply outline
                    float outline = smoothstep(_ThresholdEnd - _OutlineSmoothness, _ThresholdEnd, edge);
                    float4 outlineColor = _OutlineColor * outline;

                    // Combine base texture and outline
                    return texColor * i.vertexColor + outlineColor * (1 - texColor.a);
                #else
                    // No outline, return base texture
                    return texColor * i.vertexColor;
                #endif
            }
            ENDCG
        }

        Pass {
            Name "Caster"
            Tags { "LightMode"="ShadowCaster" }
            Offset 1, 1
            ZWrite On
            ZTest LEqual

            Fog { Mode Off }
            Cull Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            fixed _Cutoff;

            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float4 uvAndAlpha : TEXCOORD1;
            };

            VertexOutput vert (appdata_base v, float4 vertexColor : COLOR) {
                VertexOutput o;
                o.uvAndAlpha = v.texcoord;
                o.uvAndAlpha.a = vertexColor.a;
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }

            float4 frag (VertexOutput i) : SV_Target {
                fixed4 texcol = tex2D(_MainTex, i.uvAndAlpha.xy);
                clip(texcol.a * i.uvAndAlpha.a - _Cutoff);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }

    CustomEditor "SpineShaderWithOutlineGUI"
}
