Shader "Custom/GzTransparentDiffuseEx" {
	Properties{
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_AlphaCutOff("AlphaCutOff", Float) = 0.99

		[KeywordEnum(None, Front, Back)] _Cull("Culling", Float) = 2
		[KeywordEnum(Off, On)] _ZWrite("ZWrite", Float) = 0
		[KeywordEnum(Always, NotEqual, Equal, Less, LEqual, Greater, GEqual)] _ZTest("ZTest", Float) = 0
	}

	SubShader{

		Pass {
			Cull Off
			ZWrite[_ZWrite]
			AlphaTest Greater[_AlphaCutOff]
			ColorMask 0
			Lighting OFF
			SetTexture[_MainTex] {
				combine texture * primary, texture
			}
		}

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200

		Cull[_Cull]
		ZTest[_ZTest]

		CGPROGRAM
#pragma surface surf Lambert alpha

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}

	Fallback "Transparent/VertexLit"
}
