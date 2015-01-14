Shader "Hidden/Kvant/Streamline/Line"
{
    Properties
    {
        _PositionTex("-", 2D) = ""{}
        _PreviousTex("-", 2D) = ""{}
        _Color("-", Color) = (1, 1, 1, 0.5)
        _Tail("_", Float) = 1
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

    sampler2D _PositionTex;
    sampler2D _PreviousTex;

    half4 _Color;
    float _Tail;

    v2f vert(appdata v)
    {
        v2f o;

        float2 uv = v.texcoord.xy;

        float4 p1 = tex2D(_PositionTex, uv);
        float4 p2 = tex2D(_PreviousTex, uv);
        float sw = v.position.x;

        if (p2.w < 0)
        {
            o.position = mul(UNITY_MATRIX_MVP, float4(p1.xyz, 1));
        }
        else
        {
            float3 p = lerp(p1.xyz, p2.xyz, sw * _Tail);
            o.position = mul(UNITY_MATRIX_MVP, float4(p, 1));
        }

        o.color = _Color;
        o.color.a *= (1.0 - sw);

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
