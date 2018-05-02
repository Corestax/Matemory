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
        LOD 150
                
        pass
        {
            Tags { "RenderType"="Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
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
                fixed3 normal : NORMAL;
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
            
            Tags { "RenderType"="Opaque" }
            Name "Diffuse"
            CGPROGRAM
            #pragma surface surf MobileLambert exclude_path:prepass noforwardadd nolightmap finalcolor:lightEstimation
    
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
            c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb) * atten;
            UNITY_OPAQUE_ALPHA(c.a);
            return c;
            }
    
            void lightEstimation(Input IN, SurfaceOutput o, inout fixed4 color)
            {
                color.rgb *= _GlobalColorCorrection;
            }
    
            void surf (Input IN, inout SurfaceOutput o)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
                o.Albedo = c.rgb;
            }       
            ENDCG
            }
            Fallback "Mobile/VertexLit"
        }
