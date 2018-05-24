Shader "ARCore/DiffuseWithLightEstimationOutline"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Outline Color", Color) = (1,1,1,1)
        _OutlineAlpha ("Outline Alpha", Range(0,1)) = 1
        _OutlineOffset ("Outline Offset", Float) = 0.01
    }

    SubShader 
    {
        Tags { "RenderType"="Geometry+1" }
        LOD 150
                
        pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZWrite Off
            Name "Outline"   
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            
            struct appdata
			{
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
			};
            
            struct v2f
            {
                fixed4 vertex : SV_POSITION;
            };
            
            fixed _OutlineOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.xyz += _OutlineOffset * v.normal;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed3 _Color;
            fixed _OutlineAlpha;
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = 0;
                c.rgb += _Color;
                c.a += _OutlineAlpha;
                return c;
            }
            ENDCG
            }
            Name "Diffuse"
            CGPROGRAM
            #pragma surface surf MobileLambert exclude_path:deferred exclude_path:prepass noforwardadd nolightmap nodynlightmap nodirlightmap nofog nolppv finalcolor:lightEstimation
            #pragma shader_feature ARCORELIGHT_ON
            sampler2D _MainTex;
            fixed3 _GlobalColorCorrection;
    
            struct Input
            {
                float2 uv_MainTex;
            };
            
            inline fixed4 LightingMobileLambert (SurfaceOutput s, fixed3 lightDir, fixed atten)
            {
            fixed diff = max (0, dot (s.Normal, lightDir));

            fixed4 c;
            c.rgb = (s.Albedo * _LightColor0.rgb * diff) * atten;
            UNITY_OPAQUE_ALPHA(c.a);
            return c;
            }
    
            void lightEstimation(Input IN, SurfaceOutput o, inout fixed4 color)
            {
            #ifdef ARCORELIGHT_ON
                color.rgb *= _GlobalColorCorrection;
            #else
                color.rgb *= 1;
            #endif
            }
    
            void surf (Input IN, inout SurfaceOutput o)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
                o.Albedo = c.rgb;
            }       
            ENDCG
            }
            CustomEditor "LightEstToggleInspector"
            Fallback "Mobile/VertexLit"
        }
