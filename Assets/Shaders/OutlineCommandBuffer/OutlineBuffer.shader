Shader "Custom/OutlineBuffer"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		
		Pass{
		ZWrite On
		ColorMask 0
		}

		Pass
		{
		    //Blend DstColor SrcColor
		    Cull Back
		    ZWrite Off
		    ZTest LEqual
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
                v.vertex.xyz += 0.05 * v.normal;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed3 _Color;
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = 0.3;
                return c;
            }
            ENDCG
           }
           Pass
           {
			Name "Diffuse"
			ZWrite Off
			//ZTest Always
			//Offset 0, -1
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            
            struct appdata
			{
				float4 vertex : POSITION;
			};
            
            struct v2f
            {
                fixed4 vertex : SV_POSITION;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed3 _Color;
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = 0.1;
                return c;
            }
            ENDCG
           }
		}
}
