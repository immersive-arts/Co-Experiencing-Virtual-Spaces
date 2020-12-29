// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:32815,y:32504,varname:node_3138,prsc:2|custl-6854-OUT,alpha-8345-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32141,y:32446,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_ObjectPosition,id:8181,x:31823,y:32850,varname:node_8181,prsc:2;n:type:ShaderForge.SFN_ViewPosition,id:1940,x:31823,y:32711,varname:node_1940,prsc:2;n:type:ShaderForge.SFN_Distance,id:6036,x:32034,y:32750,varname:node_6036,prsc:2|A-1940-XYZ,B-5741-XYZ;n:type:ShaderForge.SFN_FragmentPosition,id:5741,x:31823,y:32977,varname:node_5741,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2199,x:32243,y:32750,varname:node_2199,prsc:2|A-6036-OUT,B-1070-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1070,x:32084,y:32907,ptovrint:False,ptlb:Density,ptin:_Density,varname:node_1070,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.02;n:type:ShaderForge.SFN_Lerp,id:1828,x:32488,y:32907,varname:node_1828,prsc:2|A-9426-OUT,B-9207-OUT,T-4549-OUT;n:type:ShaderForge.SFN_Vector1,id:5533,x:32217,y:33185,varname:node_5533,prsc:2,v1:0;n:type:ShaderForge.SFN_Clamp01,id:4549,x:32400,y:32750,varname:node_4549,prsc:2|IN-2199-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9426,x:32135,y:33016,ptovrint:False,ptlb:Transparency,ptin:_Transparency,varname:node_9426,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.8;n:type:ShaderForge.SFN_Tex2d,id:8420,x:32141,y:32264,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_8420,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:5621ec244b11a4c3a825e0900ce91a22,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:1319,x:32421,y:32378,varname:node_1319,prsc:2|A-8420-RGB,B-7241-RGB;n:type:ShaderForge.SFN_ValueProperty,id:7481,x:32303,y:32546,ptovrint:False,ptlb:Emission,ptin:_Emission,varname:node_7481,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Multiply,id:6854,x:32592,y:32413,varname:node_6854,prsc:2|A-1319-OUT,B-7481-OUT;n:type:ShaderForge.SFN_Multiply,id:8345,x:32646,y:32845,varname:node_8345,prsc:2|A-8420-A,B-1828-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9207,x:32119,y:33090,ptovrint:False,ptlb:Transparency 2,ptin:_Transparency2,varname:node_9207,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:7241-1070-9426-9207-8420-7481;pass:END;sub:END;*/

Shader "Shader Forge/EnergyWall" {
    Properties {
        _Color ("Color", Color) = (0.07843138,0.3921569,0.7843137,1)
        _Density ("Density", Float ) = 0.02
        _Transparency ("Transparency", Float ) = 0.8
        _Transparency2 ("Transparency 2", Float ) = 0
        _Texture ("Texture", 2D) = "white" {}
        _Emission ("Emission", Float ) = 2
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform float4 _Color;
            uniform float _Density;
            uniform float _Transparency;
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform float _Emission;
            uniform float _Transparency2;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(i.uv0, _Texture));
                float3 finalColor = ((_Texture_var.rgb*_Color.rgb)*_Emission);
                return fixed4(finalColor,(_Texture_var.a*lerp(_Transparency,_Transparency2,saturate((distance(_WorldSpaceCameraPos,i.posWorld.rgb)*_Density)))));
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
