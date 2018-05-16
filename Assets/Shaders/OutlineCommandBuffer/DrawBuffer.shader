Shader "Unlit/DrawBuffer"
{
	SubShader
	{
	
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest		
			#include "UnityCG.cginc"

			struct appdata
			{
				fixed4 vertex : POSITION;
				fixed2 uv : TEXCOORD0;
			};

			struct v2f
			{
				fixed2 uv : TEXCOORD0;
				fixed4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _OutlineTex;
			//float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = o.vertex.xy / 2 + 0.5;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 t = tex2D(_MainTex, i.uv);
				fixed4 g = tex2D(_OutlineTex, i.uv);
				
				fixed4 c = 0;
				c.rgb += t + g;
				//c.a += 1 * t.r; 
				return c;
			}
			ENDCG
        }	
	}
}
