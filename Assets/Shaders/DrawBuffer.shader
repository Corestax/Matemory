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
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
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
