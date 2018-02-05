Shader "Shadow Detector" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
	}
	SubShader{
	Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }

	Pass
	{
		Tags{ "LightMode" = "ForwardBase" }
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_fwdbase                       // This line tells Unity to compile this pass for forward base.
		#pragma fragmentoption ARB_fog_exp2
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma target 3.0

		#include "UnityCG.cginc"
		#include "AutoLight.cginc"

		struct v2f
		{
			float4  pos         : SV_POSITION;
			float2  uv          : TEXCOORD0;
			float3  viewDir     : TEXCOORD1;
			float3  lightDir    : TEXCOORD2;
			LIGHTING_COORDS(3,4)                            // Macro to send shadow  attenuation to the vertex shader.
		};

		v2f vert(appdata_tan v)
		{
			v2f o;

			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord.xy;
			TANGENT_SPACE_ROTATION;                         // Macro for unity to build the Object>Tangent rotation matrix "rotation".
			o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex));
			o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));

			TRANSFER_VERTEX_TO_FRAGMENT(o);                 // Macro to send shadow  attenuation to the fragment shader.
			return o;
		}

		sampler2D _BumpMap;
		fixed4 _Color;

		fixed4 _LightColor0; // Colour of the light used in this pass.

		fixed4 frag(v2f i) : COLOR
		{
			i.viewDir = normalize(i.viewDir);
			i.lightDir = normalize(i.lightDir);

			fixed atten = LIGHT_ATTENUATION(i); // Macro to get you the combined shadow  attenuation value.

			fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));

			fixed4 c = _Color;
			c = _LightColor0 * atten;
			c.a = 1;
			return c;
		}
		ENDCG
	}

	Pass
	{
		Tags{ "LightMode" = "ForwardAdd" }                       // Again, this pass tag is important otherwise Unity may not give the correct light information.
		Blend One One                                           // Additively blend this pass with the previous one(s). This pass gets run once per pixel light.
		CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdadd_fullshadows                      // This line tells Unity to compile this pass for forward add and give shadow information as well as attenuation. Swap this line for the one above if you want forward add with shadows.
#pragma fragmentoption ARB_precision_hint_fastest
#pragma target 3.0

#include "UnityCG.cginc"
#include "AutoLight.cginc"

		struct v2f
	{
		float4  pos         : SV_POSITION;
		float2  uv          : TEXCOORD0;
		float3  viewDir     : TEXCOORD1;
		float3  lightDir    : TEXCOORD2;
		LIGHTING_COORDS(3,4)                            // Macro to send shadow  attenuation to the vertex shader.
	};

	v2f vert(appdata_tan v)
	{
		v2f o;

		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		TANGENT_SPACE_ROTATION;                         // Macro for unity to build the Object>Tangent rotation matrix "rotation".
		o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex));
		o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));

		TRANSFER_VERTEX_TO_FRAGMENT(o);                 // Macro to send shadow  attenuation to the fragment shader.
		return o;
	}

	fixed4 _Color;

	fixed4 _LightColor0; // Colour of the light used in this pass.

	fixed4 frag(v2f i) : COLOR
	{
		i.viewDir = normalize(i.viewDir);
		i.lightDir = normalize(i.lightDir);

		fixed atten = LIGHT_ATTENUATION(i); // Macro to get you the combined shadow  attenuation value.

		fixed4 c = _Color;
		c = _LightColor0 * atten;
		c.a = atten;
		return c;
	}
		ENDCG
	}
	}
		FallBack "VertexLit"    // Use VertexLit's shadow caster/receiver passes.
}