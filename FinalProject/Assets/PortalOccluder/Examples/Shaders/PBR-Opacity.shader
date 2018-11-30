Shader "Custom/PBR-Opacity" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		[PerRendererData]_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Specular("Specular (RGB)", 2D) = "black" {}
		_Normal("Normal", 2D) = "bump" {}
		_Opacity ("Opacity", Range(0,1)) = 1.0
	}
	SubShader {
		Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Specular;
		sampler2D _Normal;

		struct Input {
			float2 uv_MainTex;
		};

		half _Opacity;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 s = tex2D(_Specular, IN.uv_MainTex);
			half3 n = UnpackNormal(tex2D(_Normal, IN.uv_MainTex));
			o.Albedo = c.rgb;
			//o.Albedo = float3(1, 1, 1);
			// Metallic and smoothness come from slider variables
			o.Specular = s.rgb;
			o.Normal = n.rgb;
			o.Smoothness = s.a;
			o.Alpha = c.a * _Opacity;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
