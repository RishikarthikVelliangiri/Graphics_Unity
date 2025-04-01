Shader "DecalUV2" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_DecalColor ("Decal Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_DecalTex ("Decal (RGBA)", 2D) = "black" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 250
	
CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
sampler2D _DecalTex;
fixed4 _Color;
fixed4 _DecalColor;

struct Input {
	float2 uv_MainTex;
	float2 uv2_DecalTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	half4 decal = tex2D(_DecalTex, IN.uv2_DecalTex);
	c.rgb = lerp (c.rgb, decal.rgb * _DecalColor.rgb, decal.a);
	c *= _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Diffuse"
}
