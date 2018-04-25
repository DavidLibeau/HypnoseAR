Shader "Custom/ParamDeformation" {
	Properties {
		_MainTex ("Primary Texture", 2D) = "white" {}
		_SecTex("Secondary Texture", 2D) = "white" {}
		_Dist("Distorsion Amplitude", Range(0,1)) = 0.5
		_Speed("Wave Velocity", Range(0,10)) = 1
		_WNb("Wave Number", Range(0,10)) = 1
		_TransPercent("Transition Percentage", Range(0,1)) = 0
		_AmbLight("Ambient Lighting", Range(0,2)) = 1
		_DiffLight("Diffuse Lighting", Range(0,2))= 1
		_SpecLight("Specular Lighting", Range(0.1,10)) = 1
	}
	SubShader
	{
		//Tags { "RenderType"="Opaque" }
		Tags {"LightMode"="ForwardBase"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 diff : COLOR0;
			};

			sampler2D _MainTex;
			sampler2D _SecTex;
			float4 _MainTex_ST;
			float _Dist;
			float _Speed;
			float _WNb;
			float _TransPercent;
			float _AmbLight;
			float _DiffLight;
			float _SpecLight;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.vertex.y += _Dist*sin(worldPos.x*_WNb + _Speed*_Time.w);

				o.uv = v.texcoord;//TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);

				// Ambient lighting
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

				o.diff = _AmbLight * _LightColor0;
				//o.diff = nl * _LightColor0 * _AmbLight;

				// Specular lighting
				float4 lightDirection = normalize(_WorldSpaceLightPos0);
				float4 cameraDirection = normalize(float4(_WorldSpaceCameraPos,1) - o.vertex);
				float RdotV = max(0.,dot(reflect(-lightDirection,worldNormal),cameraDirection));
				float4 NdotL = max(0.,dot(worldNormal,lightDirection)*_LightColor0);

				pow(_SpecLight,_SpecLight);
				fixed4 spec = pow(RdotV,_SpecLight) * _LightColor0 * ceil(NdotL);
				o.diff.rgb += spec;


				// Diffuse lighting
				o.diff.rgb += NdotL * _DiffLight * _LightColor0;
				//o.diff.rgb += ShadeSH9(half4(worldNormal,1)) * _DiffLight;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 colPrim = tex2D(_MainTex, i.uv);
				fixed4 colSec = tex2D(_SecTex, i.uv);

				fixed4 col = colPrim*(1-_TransPercent) + colSec*_TransPercent;
				// shadow
				col*= i.diff;

				return col;
			}


			ENDCG
		}
	}
}
