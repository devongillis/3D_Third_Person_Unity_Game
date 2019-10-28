Shader "Custom/fourTexturesBlend"
{
	Properties
	{
		_MainTex("main", 2D) = "black" {}
		_SecTex("red", 2D) = "white" {}
		_ThirdTex("green", 2D) = "white" {}
		_FilTex("filter", 2D) = "black"{}
	}

		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert
		#pragma target 4.0
		sampler2D _MainTex;
		sampler2D _SecTex;
		sampler2D _ThirdTex;
		sampler2D _FourthTex;
		sampler2D _FilTex;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_SecTex;
			float2 uv_ThirdTex;
			float2 uv_FourthTex;
			float2 uv_FilTex;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 main = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 sec = tex2D(_SecTex, IN.uv_SecTex);
			fixed4 third = tex2D(_ThirdTex, IN.uv_ThirdTex);
			fixed4 fourth = tex2D(_FourthTex, IN.uv_FourthTex);
			fixed4 filter = tex2D(_FilTex, IN.uv_FilTex);

			fixed3 mix1= lerp(main.rgb, sec.rgb, filter.r);
			fixed3 mix2 = lerp(mix1.rgb, third.rgb, filter.g);
			o.Albedo = mix2;
			o.Alpha = main.a;
		}
		ENDCG
	}
		FallBack "Diffuse"
}
