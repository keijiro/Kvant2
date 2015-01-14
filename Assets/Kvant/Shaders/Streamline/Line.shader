Shader "Hidden/Kvant/Streamline/Line"
{
    Properties
    {
        _PreviousTex("-", 2D) = ""{}
        _PositionTex("-", 2D) = ""{}
        _Color("Color", Color) = (1, 1, 1, 0.5)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    struct appdata
    {
        float4 position : POSITION;
        float2 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 position : SV_POSITION;
        float4 color : COLOR;
    };

    sampler2D _PreviousTex;
    sampler2D _PositionTex;

    half4 _Color;

    v2f vert(appdata v)
    {
        v2f o;

        float2 uv = v.texcoord.xy;

        float4 p1 = tex2D(_PreviousTex, uv);
        float4 p2 = tex2D(_PositionTex, uv);
        float sw = v.position.x;

        if (p1.w < 0)
        {
            o.position = mul(UNITY_MATRIX_MVP, float4(p2.xyz, 1));
        }
        else
        {
            float3 p = lerp(p1.xyz, p2.xyz, sw);
            o.position = mul(UNITY_MATRIX_MVP, float4(p, 1));
        }

        o.color = _Color;
        o.color.a *= sw;

        return o;
    }

    half4 frag(v2f i) : COLOR
    {
        return i.color;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    } 
}
