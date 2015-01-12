Shader "Hidden/Kvant/Tunnel"
{
    Properties
    {
        _MainTex("-", 2D) = ""{}

        _Radius("-", Float) = 5
        _Height("-", Float) = 5

        _Offset("-", Vector) = (0, 0, 0, 0)
        _Density("-", Vector) = (1, 1, 0, 0)
        _Displace("-", Vector) = (0.3, 0.3, 0.3, 0)
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "ClassicNoise2D.cginc"

    #define PI2 6.28318530718

    sampler2D _MainTex;
    float2 _MainTex_TexelSize;

    float _Radius;
    float _Height;

    float2 _Offset;
    float2 _Density;
    float3 _Displace;

    // Base shape (cylinder).
    float3 cylinder(float2 uv)
    {
        float x = cos(uv.x * PI2) * _Radius;
        float y = sin(uv.x * PI2) * _Radius;
        float z = (uv.y - 0.5) * _Height;
        return float3(x, y, z);
    }

    // Pass0: position
    float4 frag_pos(v2f_img i) : SV_Target 
    {
        float3 vp = cylinder(i.uv);
        float2 np = i.uv * _Density;
        float2 nr = float2(1, 1) * _Density;

        float2 no1 = _Offset;
        float2 no2 = _Offset + float2(0, 31.5912);
        float2 no3 = _Offset + float2(27.534, 0);

        float n1 = pnoise(np + no1, nr) + pnoise(np * 2 + no1, nr * 2) * 0.5 + pnoise(np * 4 + no1, nr * 4) * 0.25;
        float n2 = pnoise(np + no2, nr) + pnoise(np * 2 + no2, nr * 2) * 0.5 + pnoise(np * 4 + no2, nr * 4) * 0.25;
        float n3 = pnoise(np + no3, nr) + pnoise(np * 2 + no3, nr * 2) * 0.5 + pnoise(np * 4 + no3, nr * 4) * 0.25;

        float3 v1 = normalize(vp * float3(-1, -1, 0));
        float3 v2 = float3(0, 0, 1);
        float3 v3 = cross(v1, v2);

        float3 d = v1 * n1 * _Displace.x +
                   v2 * n2 * _Displace.y +
                   v3 * n3 * _Displace.z;

        return float4(vp + d, 0);
    }

    // Pass1: normal vector for the 1st submesh
    float4 frag_norm1(v2f_img i) : SV_Target 
    {
        float2 duv = _MainTex_TexelSize;

        float3 v1 = tex2D(_MainTex, i.uv + float2( 0, 0) * duv).xyz;
        float3 v2 = tex2D(_MainTex, i.uv + float2(-1, 1) * duv).xyz;
        float3 v3 = tex2D(_MainTex, i.uv + float2( 1, 1) * duv).xyz;

        float3 n = normalize(cross(v2 - v1, v3 - v2));

        return float4(n, 0);
    }

    // Pass2: normal vector for the 2nd submesh
    float4 frag_norm2(v2f_img i) : SV_Target 
    {
        float2 duv = _MainTex_TexelSize;

        float3 v1 = tex2D(_MainTex, i.uv + float2(0, 0) * duv).xyz;
        float3 v2 = tex2D(_MainTex, i.uv + float2(1, 1) * duv).xyz;
        float3 v3 = tex2D(_MainTex, i.uv + float2(2, 0) * duv).xyz;

        float3 n = normalize(cross(v2 - v1, v3 - v1));

        return float4(n, 0);
    }

    ENDCG

    SubShader
    {
        // Pass0: position
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_pos
            ENDCG
        }
        // Pass1: normal vector for the 1st submesh
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_norm1
            ENDCG
        }
        // Pass2: normal vector for the 2nd submesh
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_norm2
            ENDCG
        }
    }
}
