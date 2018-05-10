Shader "Custom/OutlineReplaceMiddle"
{
	Properties
	{
		_Color ("Outline Color", Color) = (1,1,1,1)
		_OutlineOffset ("Outline offset distance", Float) = 0.005
		_OutlineAlpha ("Outline Alpha", Range(0,1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100
		
		pass{
		    ColorMask R
		}

		Pass
		{
		    Cull Back
		    ZWrite Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			fixed3 _Color;
			fixed _OutlineOffset;
			fixed _OutlineAlpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				v.vertex.xyz += _OutlineOffset * v.normal;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 c = 0;
				c.rgb += _Color;
				//c.a += _OutlineAlpha * _ColorMask;
				return c;
			}
			ENDCG
		}
	}
}
