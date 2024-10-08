Shader "UI/Transition"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_SourceTex ("Source Texture", 2D) = "white" {}
		_PatternTex ("_PatternTex", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		_Dissolve("_Dissolve", Range(0, 1)) = 0

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Transition"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			fixed4 _Color;

			float4 _ClipRect;

			half _Dissolve;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color;
				return OUT;
			}

			sampler2D _SourceTex;
			sampler2D _PatternTex;
			half4 _PatternTex_ST;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color_tex = tex2D(_SourceTex, IN.texcoord);				
				half pattern_tex = tex2D(_PatternTex, IN.texcoord*_PatternTex_ST.xy+_PatternTex_ST.zw).r;
				
				half gradient = (IN.texcoord.x+(1-IN.texcoord.y))/2;
				gradient = lerp (-0.25, 1.5, gradient);
				
				half dissolve = lerp(-1.25, 3, _Dissolve);
				dissolve = lerp(dissolve, dissolve-1.4, gradient);				
			
				half pattern_alpha = saturate(pattern_tex - dissolve);
				
				half alpha = IN.color.a * pattern_alpha;				
				alpha *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);				
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (alpha - 0.001);
				#endif
				
				half3 color = color_tex.rgb;
				color = lerp(color.rgb * IN.color.rgb, _Color.rgb, _Color.a);
				
				return half4(color, alpha);
			}
		ENDCG
		}
	}
}
