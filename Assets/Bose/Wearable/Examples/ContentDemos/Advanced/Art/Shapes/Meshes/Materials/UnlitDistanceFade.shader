Shader "Unlit/UnlitDistanceFade"
{
	Properties
	{
	    _Color ("Color", Color) = (1,1,1,1)
	    _FadeCutoff ("FadeCutoff", Float) = 1.0
	    _FadeWidth ("FadeWidth", Float) = 2.0
	    _BaseAlpha ("BaseAlpha", Float) = 0.1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
		LOD 200

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
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color;
			float _FadeCutoff;
			float _FadeWidth;
			float _BaseAlpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _Color;
				float dist = i.vertex.z;
				col.a = saturate(saturate(10.0f * (_FadeCutoff * 0.1f - dist) / _FadeWidth) + _BaseAlpha);
				return col;
			}
			ENDCG
		}
	}
}
