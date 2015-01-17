Shader "Hidden/Kvant/Tunnel/GPGPU"
{
    Properties
    {
        _MainTex("-", 2D) = ""{}
        _Size("-", Vector) = (5, 5, 0, 0)
        _Offset("-", Vector) = (0, 0, 0, 0)
        _Period("-", Vector) = (1, 1, 0, 0)
        _Density("-", Vector) = (1, 1, 0, 0)
        _Displace("-", Vector) = (0.3, 0.3, 0.3, 0)
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "ClassicNoise2D.cginc"

    #define PI2 6.28318530718

    sampler2D _MainTex;
    float2 _MainTex_TexelSize;
    float2 _Size;
    float2 _Offset;
    float2 _Period;
    float2 _Density;
    float3 _Displace;

    // Base shape (cylinder).
    float3 cylinder(float2 uv)
    {
        float x = cos(uv.x * PI2);
        float y = sin(uv.x * PI2);
        float z = uv.y - 0.5;
        return float3(x, y, z) * _Size.xxy;
    }

    // Pass0: position
    float4 frag_pos(v2f_img i) : SV_Target 
    {
        float3 vp = cylinder(i.uv);

        float2 nc1 = i.uv * _Density + _Offset;
        float2 nc2 = nc1 + float2(124.343, 311.591);
        float2 nc3 = nc1 + float2(273.534, 178.392);

        float2 np = _Period;

        float n1 = pnoise(nc1, np) + pnoise(nc1 * 2, np * 2) * 0.5 + pnoise(nc1 * 4, np * 4) * 0.25 + pnoise(nc1 * 8, np * 8) * 0.125;
        float n2 = pnoise(nc2, np) + pnoise(nc2 * 2, np * 2) * 0.5 + pnoise(nc2 * 4, np * 4) * 0.25 + pnoise(nc1 * 8, np * 8) * 0.125;
        float n3 = pnoise(nc3, np) + pnoise(nc3 * 2, np * 2) * 0.5 + pnoise(nc3 * 4, np * 4) * 0.25 + pnoise(nc1 * 8, np * 8) * 0.125;

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

        float3 n = normalize(cross(v2 - v1, v3 - v1));

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
            Fog { Mode off }    
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
            Fog { Mode off }    
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
            Fog { Mode off }    
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_norm2
            ENDCG
        }
    }
}
