Shader "Hidden/Kvant/Spray/Surface"
{
    Properties
    {
        _PositionTex("-", 2D) = ""{}
        _RotationTex("-", 2D) = ""{}
        _Color("-", Color) = (1, 1, 1, 0.5)
        _BufferOffset("-", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        CGPROGRAM

        #pragma surface surf Lambert vertex:vert
        #pragma glsl

        sampler2D _PositionTex;
        float2 _PositionTex_TexelSize;

        sampler2D _RotationTex;
        float2 _RotationTex_TexelSize;

        float _BufferOffset;

        float4 _Color;

        struct Input
        {
            float dummy;
        };

        // PRNG function.
        float nrand(float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
        }

        // Quaternion multiplication.
        // http://mathworld.wolfram.com/Quaternion.html
        float4 qmul(float4 q1, float4 q2)
        {
            return float4(
                q1.w * q2.xyz + q2.w * q1.xyz + cross(q1.xyz, q2.xyz),
                q1.w * q2.w - dot(q1.xyz, q2.xyz)
            );
        }

        // Rotate a vector with a rotation quaternion.
        // http://mathworld.wolfram.com/Quaternion.html
        float3 rotate_vector(float3 v, float4 r)
        {
            float4 r_c = r * float4(-1, -1, -1, 1);
            return qmul(r, qmul(float4(v, 0), r_c)).xyz;
        }

        void vert(inout appdata_full v)
        {
            float2 uv = v.texcoord + _PositionTex_TexelSize * 0.5;
            uv.y += _BufferOffset;

            float4 p = tex2D(_PositionTex, uv);
            float4 r = tex2D(_RotationTex, uv);
            float s = nrand(uv) * 0.4 + 0.1;

            v.vertex.xyz = rotate_vector(v.vertex.xyz, r) * s + p.xyz;
            v.normal = rotate_vector(v.normal, r);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
            o.Alpha = 1;
        }

        ENDCG
    } 
}
