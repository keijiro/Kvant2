Shader "Hidden/Kvant/Line"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 0.5)
        _PositionTex("-", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    struct v2f
    {
        float4 position : SV_POSITION;
    };

    half4 _Color;
    sampler2D _PositionTex;
    float4 _PositionTex_TexelSize;

    v2f vert(appdata_base v)
    {
        float2 uv = v.texcoord.xy;
        uv += _PositionTex_TexelSize.xy * 0.5;

        float4 pos = v.vertex;
        pos.xyz += tex2D(_PositionTex, uv).xyz;

        v2f o;
        o.position = mul(UNITY_MATRIX_MVP, pos);
        return o;
    }

    half4 frag(v2f i) : COLOR
    {
        return _Color;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
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
