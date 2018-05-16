Shader "Custom/OutlineBuffer"
{
    Properties
    {
        //_MainTex ("Main Texture", 2D) = "black" {}
    }
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
		    //Blend DstColor SrcColor
		    Name "Outline"
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
				fixed4 vertex : POSITION;
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
                v.vertex.xyz += 0.005 * v.normal;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed3 _Color;
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = 0.35;
                return c;
            }
            ENDCG
           }
           Pass
           {
			Name "Fill"
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
				fixed4 vertex : POSITION;
				//float2 uv : TEXCOORD0;
			};
            
            struct v2f
            {
                fixed4 vertex : SV_POSITION;
                //float2 uv : TEXCOORD0;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = o.vertex.xy;
                //o.uv *= 2;
                return o;
            }
            
            fixed3 _Color;
            //sampler2D _MainTex;
            
            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 t = tex2D(_MainTex, i.uv);
                
                fixed4 c = 0.15;
                return c;
            }
            ENDCG
           }
		}
}
