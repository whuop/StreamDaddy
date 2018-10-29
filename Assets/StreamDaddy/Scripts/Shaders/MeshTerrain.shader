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
			// Splat Map Control Texture
			_Control("Control (RGBA)", 2D) = "red" {}
			// Textures
			_Splat3("Layer 3 (B)", 2D) = "white" {}
			_Splat2("Layer 2 (G)", 2D) = "white" {}
			_Splat1("Layer 1 (R)", 2D) = "white" {}
			_Splat0("Layer 0 (A)", 2D) = "white" {}
		}
		SubShader{
		  Tags {
			"SplatCount" = "4"
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		  }

		//LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;

		struct Input {
		  float2 uv_Control : TEXCOORD0;
		  float2 uv_Splat0 : TEXCOORD1;
		  float2 uv_Splat1 : TEXCOORD2;
		  float2 uv_Splat2 : TEXCOORD3;
		  float2 uv_Splat3 : TEXCOORD4;

		  float3 worldPos;
		  float3 worldNormal;
		};

		sampler2D _Control;
		sampler2D _Splat0,_Splat1,_Splat2,_Splat3;

		void surf(Input IN, inout SurfaceOutput o) 
		{
			// Find our UVs for each axis based on world position of the fragment.
			half2 yUV = IN.worldPos.xz; /// _TextureScale;
			// Now do texture samples from our diffuse map with each of the 3 UV set's we've just made.
			//half3 yDiff = tex2D(_DiffuseMap, yUV);
			yUV = yUV / 10;
			// Get the absolute value of the world normal.
			// Put the blend weights to the power of BlendSharpness, the higher the value, 
			// the sharper the transition between the planar maps will be.
			//half3 blendWeights = pow(abs(IN.worldNormal), _TriplanarBlendSharpness);

		   fixed4 splat_control = tex2D(_Control, IN.worldPos);
		   fixed3 col;
		   col = splat_control.r * tex2D(_Splat0, yUV).rgb;
		   col += splat_control.g * tex2D(_Splat1, yUV).rgb;
		   col += splat_control.b * tex2D(_Splat2, yUV).rgb;
		   col += splat_control.a * tex2D(_Splat3, yUV).rgb;

		   o.Albedo = float3(yUV.x, yUV.y, 0);//col;
		   o.Alpha = 1;
		}
		ENDCG
	}
		FallBack "Diffuse"
}