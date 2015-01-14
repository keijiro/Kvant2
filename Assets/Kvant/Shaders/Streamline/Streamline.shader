Shader "Hidden/Kvant/Streamline/GPGPU"
{
    Properties
    {
        _MainTex    ("-", 2D)       = ""{}
        _Range      ("-", Vector)   = (100, 100, 100, 0)
        _Velocity   ("-", Vector)   = (0, 0, -10, 0)
        _Noise      ("-", Vector)   = (0.5, 5, 0, 0)
        _Random     ("-", Float)    = 0.5
        _Life       ("-", Float)    = 3
    }

    CGINCLUDE

    #pragma multi_compile NOISE_OFF NOISE_ON

    #include "UnityCG.cginc"
    #include "ClassicNoise3D.cginc"

    sampler2D _MainTex;
    float3 _Range;
    float3 _Velocity;
    float2 _Noise;
    float _Random;
    float _Life;

    // PRNG function.
    float nrand(float2 uv)
    {
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // Get a new particle.
    float4 new_particle(float2 uv)
    {
        uv += _Time.x;
        float x = nrand(uv               ) - 0.5;
        float y = nrand(uv + float2(1, 0)) - 0.5;
        float z = nrand(uv + float2(0, 1)) - 0.5;
        float w = nrand(uv + float2(1, 1)) * 0.9 + 0.1;
        return float4(x, y, z, w) * float4(_Range, _Life);
    }

    // Noise function.
    float3 position_noise(float3 p)
    {
        p *= _Noise.x;
        float nx = cnoise(p + float3(10, 0, 0));
        float ny = cnoise(p + float3(0, 10, 0));
        float nz = cnoise(p + float3(0, 0, 10)); 
        return float3(nx, ny, nz) * _Noise.y;
    }

    // Pass0: initialization
    float4 frag_init(v2f_img i) : SV_Target 
    {
        return new_particle(i.uv);
    }

    // Pass1: update
    float4 frag_update(v2f_img i) : SV_Target 
    {
        float delta = unity_DeltaTime.x;
        float4 p = tex2D(_MainTex, i.uv);
        if (p.w > 0)
        {
            p.xyz += _Velocity * (1.0 - nrand(i.uv) * _Random) * delta;
#ifdef NOISE_ON
            p.xyz += position_noise(p.xyz) * delta;
#endif
            p.w -= delta;
            return p;
        }
        else
        {
            return new_particle(i.uv);
        }
    }

    ENDCG

    SubShader
    {
        // Pass0: initialization
        Pass
        {
            Fog { Mode off }    
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_init
            ENDCG
        }
        // Pass1: update
        Pass
        {
            Fog { Mode off }    
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_update
            ENDCG
        }
    }
}
