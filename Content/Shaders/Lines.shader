// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Raptorij/Lines" {
	Properties{
		_Color1("Color 1", Color) = (0,0,0,1)
		_Color2("Color 2", Color) = (1,1,1,1)
		_Tiling("Tiling", Range(0, 10)) = 1
		_Direction("Direction", Range(0, 1)) = 0
		_WarpScale("Warp Scale", Range(0, 1)) = 1
		_WarpTiling("Warp Tiling", Range(1, 10)) = 2


		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
		_ScrollXSpeed("X Scroll Speed",Range(0,10)) = 10
		_ScrollYSpeed("Y Scroll Speed",Range(0,10)) = 10

	}

	SubShader
	{
		 Tags
		 {
			 "Queue" = "Transparent"
			 "IgnoreProjector" = "True"
			 "RenderType" = "Transparent"
			 "PreviewType" = "Plane"
		 }

		 Stencil
		 {
			 Ref[_Stencil]
			 Comp[_StencilComp]
			 Pass[_StencilOp]
			 ReadMask[_StencilReadMask]
			 WriteMask[_StencilWriteMask]
		 }

		 Lighting Off
		 Cull Off
		 ZTest Off
		 ZWrite Off
		 Blend SrcAlpha OneMinusSrcAlpha
		 ColorMask[_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			fixed4 _Color1;
			fixed4 _Color2;
			int _Tiling;
			float _Direction;
			float _WarpScale;
			float _WarpTiling;

			fixed _ScrollXSpeed;
			fixed _ScrollYSpeed;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				v.uv.x += _Time * _ScrollXSpeed;
				v.uv.y += _Time * _ScrollYSpeed;
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				const float PI = 3.14159;

				float2 pos;
				pos.x = lerp(i.uv.x, i.uv.y, _Direction);
				pos.y = lerp(i.uv.y, 1 - i.uv.x, _Direction);

				//pos.x += sin(pos.y * _WarpTiling * PI * 2) * _WarpScale;
				pos.x *= _Tiling / _WarpTiling;

				fixed value = floor(frac(pos.x) + 0.5);
				return lerp(_Color1, _Color2, value);
			}
			ENDCG
		}
	}
}
