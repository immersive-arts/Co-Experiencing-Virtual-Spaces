
Shader "MyShaders/UnlitXRay" {
    Properties {
        _Color ("Color", Color) = (0.07843138,0.3921569,0.7843137,1)     
        _UnlitColor ("Unlit Color", Color) = (0.07843138,0.3921569,0.7843137,1)

    }
    SubShader {
        Tags {
            "RenderType"="Transparent"
        }
        Pass {
            Tags {
                "Queue" = "Transparent"
            }
            
            Stencil {
                Ref 4
                Comp always
                Pass replace
                ZFail keep
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform float4 _Color;
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float3 emissive = _Color.rgb;
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        
          Pass {
            Tags
            {
                "Queue" = "Transparent"
            }
            // Won't draw where it sees ref value 4
            Cull Back // draw front faces
            ZWrite OFF
            ZTest Always
            Stencil
            {
                Ref 4 
                Comp NotEqual
                Pass keep
            }
            Blend SrcAlpha OneMinusSrcAlpha

            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform float4 _UnlitColor;
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float3 emissive = _UnlitColor.rgb;
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
}
