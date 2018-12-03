//------------------------------//
//  SplatLayers.shader  //
//  Written by Jay Kay  //
//  2015/2/19  //
//------------------------------//

// derived from :
//  http://www.blog.radiator.debacle.us/2013/09/hacking-blend-transition-masks-into.html
//  https://alastaira.wordpress.com/2013/12/07/custom-unity-terrain-material-shaders/
//


Shader "Custom/MeshTerrain" {
	Properties{
			_TriplanarTexturing("Triplanar Texturing (Bool)", float) = 0.0
			_TriplanarBlendSharpness("Blend Sharpness (Float)", float) = 1.0
			// Splat Map Control Texture
			_Control("Control (RGBA)", 2D) = "red" {}
			// Textures
			_Splat3("Layer 3 (B)", 2D) = "white" {}
			_Splat2("Layer 2 (G)", 2D) = "white" {}
			_Splat1("Layer 1 (R)", 2D) = "white" {}
			_Splat0("Layer 0 (A)", 2D) = "white" {}
		}
		SubShader
		{
		  Tags 
			{
				"SplatCount" = "4"
				"Queue" = "Geometry-100"
				"RenderType" = "Opaque"
			}

		//LOD 200

		CGPROGRAM
		#pragma surface surf Lambert
#include "UnityCG.cginc"

		//sampler2D _MainTex;

		struct Input 
		{
		  float2 uv_Control : TEXCOORD0;

		  float3 worldPos;
		  float3 worldNormal;
		};

		float _TriplanarTexturing;
		sampler2D _Control;
		sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
		float4 _Splat0_ST;
		float4 _Splat1_ST;
		float4 _Splat2_ST;
		float4 _Splat3_ST;

		float _TriplanarBlendSharpness;
		
		void surf(Input IN, inout SurfaceOutput o) 
		{
			if (!_TriplanarTexturing)
			{
				// Find our UVs for each axis based on world position of the fragment.
				half2 yUV = IN.worldPos.xz;
				fixed4 splat_control = tex2D(_Control, IN.uv_Control);
				fixed3 col;
				col = splat_control.r; //* tex2D(_Splat0, yUV / _Splat0_ST.xy).rgb;
				col += splat_control.g; //* tex2D(_Splat1, yUV / _Splat1_ST.xy).rgb;
				col += splat_control.b; //* tex2D(_Splat2, yUV / _Splat2_ST.xy).rgb;
				col += splat_control.a; //* tex2D(_Splat3, yUV / _Splat3_ST.xy).rgb;

				o.Albedo = col;
			}
			else
			{
				//	Splat control tells us which texture that should be where
				fixed4 splat_control = tex2D(_Control, IN.uv_Control);

				//	values needed for triplanar projection
				float3 worldNormal = normalize(IN.worldNormal);
				float3 projNormal = saturate(pow(worldNormal * 1.5, 4));

				float2 uv;
				float2 uv0;
				float2 uv1;
				float2 uv2;
				float2 uv3;

				fixed3 albedoX;
				fixed3 albedoY;
				fixed3 albedoZ;

				//	Calculate X for all
				
				//	UV
				uv = IN.worldPos.zy;
				uv0 = uv / _Splat0_ST.xy;
				uv1 = uv / _Splat1_ST.xy;
				uv2 = uv / _Splat2_ST.xy;
				uv3 = uv / _Splat3_ST.xy;

				//	Albedo
				albedoX = tex2D(_Splat0, uv0).rgb * splat_control.r;
				albedoX += tex2D(_Splat1, uv1).rgb * splat_control.g;
				albedoX += tex2D(_Splat2, uv2).rgb * splat_control.b;
				albedoX += tex2D(_Splat3, uv3).rgb * splat_control.a;

				//	Calculate Y for all

				//	UV
				uv = IN.worldPos.xz;
				uv0 = uv / _Splat0_ST.xy;
				uv1 = uv / _Splat1_ST.xy;
				uv2 = uv / _Splat2_ST.xy;
				uv3 = uv / _Splat3_ST.xy;

				//	Albedo
				albedoY = tex2D(_Splat0, uv0).rgb * splat_control.r;
				albedoY += tex2D(_Splat1, uv1).rgb * splat_control.g;
				albedoY += tex2D(_Splat2, uv2).rgb * splat_control.b;
				albedoY += tex2D(_Splat3, uv3).rgb * splat_control.a;

				//	Calculate Z for all

				//	UV
				uv = IN.worldPos.xy;
				uv0 = uv / _Splat0_ST.xy;
				uv1 = uv / _Splat1_ST.xy;
				uv2 = uv / _Splat2_ST.xy;
				uv3 = uv / _Splat3_ST.xy;

				//	Albedo
				albedoZ = tex2D(_Splat0, uv0).rgb * splat_control.r;
				albedoZ += tex2D(_Splat1, uv1).rgb * splat_control.g;
				albedoZ += tex2D(_Splat2, uv2).rgb * splat_control.b;
				albedoZ += tex2D(_Splat3, uv3).rgb * splat_control.a;

				o.Albedo = albedoZ;
				o.Albedo = lerp(o.Albedo, albedoY, projNormal.y);
				o.Albedo = lerp(o.Albedo, albedoX, projNormal.x);
			}
			
		}
		ENDCG
	}
		FallBack "Diffuse"
}