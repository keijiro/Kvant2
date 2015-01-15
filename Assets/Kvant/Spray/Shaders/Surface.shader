Shader "Hidden/Kvant/Spray/Surface"
{
    Properties
    {
        _PositionTex("-", 2D) = ""{}
        _RotationTex("-", 2D) = ""{}
        _Color("-", Color) = (1, 1, 1, 0.5)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        CGPROGRAM

        #pragma surface surf Lambert vertex:vert
        #pragma glsl

        sampler2D _PositionTex;
        sampler2D _PreviousTex;

        float4 _Color;

        struct Input
        {
            float dummy;
        };

        void vert(inout appdata_full v)
        {
            float2 uv = v.texcoord;
            v.vertex.xyz += tex2D(_PositionTex, uv).xyz;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
            o.Alpha = 1;
        }

        ENDCG
    } 
}
