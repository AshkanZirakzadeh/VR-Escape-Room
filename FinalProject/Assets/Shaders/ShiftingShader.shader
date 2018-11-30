// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/ShiftingShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SecTex ("Texture", 2D) = "black" {}
		_Blend("Mix Lerp Value", Range(0,1) ) = 0.5
		_Bound("Delta of difference to shift image", Range(0,1000) ) = 300

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float blendLerp : TEXCOORD2;
			};

			sampler2D _MainTex;
			sampler2D _SecTex;
			float4 _MainTex_ST;
			float4 _SecTex_ST;
			float _Blend;
			float _Bound;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv, _SecTex);

				float pos = mul (unity_ObjectToWorld, v.vertex).x;

				float lb = pos - _Bound;
				float rb = pos + _Bound;
				float cam = _WorldSpaceCameraPos.x;
								
				

				float temp = (( cam - lb ) / ( pos - lb ) * 0.5)  * step(pos,cam);
				float temp2 = (0.5 + ((cam - pos ) / ( rb - pos ) * 0.5)) * step(cam,pos);

				o.blendLerp = temp+temp2;



				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 col2 = tex2D(_SecTex, i.uv2);
				fixed4 mixed = lerp(col,col2,saturate(i.blendLerp));
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, mixed);
				return mixed;
			}
			ENDCG
		}
	}
}