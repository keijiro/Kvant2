Shader "Hidden/Kvant/Spray/Kernels"
{
    Properties
    {
        _MainTex            ("-", 2D)       = ""{}
        _EmitterPos         ("-", Vector)   = (0, 0, 0, 0)
        _EmitterSize        ("-", Vector)   = (1, 1, 1, 0)
        _LifeScaleParams    ("-", Vector)   = (1, 4, 0.1, 1.2)
        _Direction          ("-", Vector)   = (0, 0, 1, 0.2)
        _SpeedParams        ("-", Vector)   = (2, 10, 30, 200)
        _NoiseParams        ("-", Vector)   = (0.2, 5, 0, 0)
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "ClassicNoise3D.cginc"

    #define PI2 6.28318530718

    sampler2D _MainTex;
    float3 _EmitterPos;
    float3 _EmitterSize;
    float4 _LifeScaleParams;
    float4 _Direction;
    float4 _SpeedParams;
    float4 _NoiseParams;

    // PRNG function.
    float nrand(float2 uv)
    {
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    float nrand(float2 uv, float salt)
    {
        return nrand(uv + float2(salt));
    }

    // Quaternion multiplication.
    // http://mathworld.wolfram.com/Quaternion.html
    float4 qmul(float4 q1, float4 q2)
    {
        return float4(
            q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
            q1.w * q2.w - dot(q1.xyz, q2.xyz)
        );
    }

    // Generate a new particle.
    float4 new_particle_position(float2 uv)
    {
        float t = _Time.x;

        // Random position.
        float3 p = float3(nrand(uv, t + 1), nrand(uv, t + 2), nrand(uv, t + 3));
        p = (p - float3(0.5)) * _EmitterSize + _EmitterPos;

        return float4(p, 1); // life = 1
    }

    float4 new_particle_rotation(float2 uv)
    {
        // Random scale factor.
        float s = lerp(_LifeScaleParams.z, _LifeScaleParams.w, nrand(uv, 5));

        // Uniform random unit quaternion.
        // http://tog.acm.org/resources/GraphicsGems/gemsiii/urot.c
        float r = nrand(uv, 6);
        float r1 = sqrt(1.0 - r);
        float r2 = sqrt(r);
        float t1 = PI2 * nrand(uv, 7);
        float t2 = PI2 * nrand(uv, 8);

        // To get the quaternion, 4th component should be 'cos(t2) * r2',
        // but we replace it with the scale factor.
        return float4(sin(t1) * r1, cos(t1) * r1, sin(t2) * r2, s);
    }

    // Velocity function.
    float3 get_velocity(float3 p, float2 uv)
    {
        // Random vector.
        float3 v = float3(nrand(uv, 9), nrand(uv, 10), nrand(uv, 11));
        v = (v - float3(0.5)) * 2;

        // Apply the spread parameter.
        v = lerp(_Direction.xyz, v, _Direction.w);

        // Apply the speed parameter.
        v = normalize(v) * lerp(_SpeedParams.x, _SpeedParams.y, nrand(uv, 12));

        // Add noise vector.
        p = p * _NoiseParams.x + _Time.y;
        float nx = cnoise(p + float3(138.2, 0, 0));
        float ny = cnoise(p + float3(0, 138.2, 0));
        float nz = cnoise(p + float3(0, 0, 138.2));

        return v + float3(nx, ny, nz) * _NoiseParams.y;
    }

    // Get the rotation axis.
    float3 get_rotation_axis(float2 uv)
    {
        // Uniformaly distributed points.
        // http://mathworld.wolfram.com/SpherePointPicking.html
        float u = nrand(uv, 13) * 2 - 1;
        float theta = nrand(uv, 14) * PI2;
        float u2 = sqrt(1 - u * u);
        return float3(u2 * cos(theta), u2 * sin(theta), u);
    }

    // Kernel 0 - initialize position.
    float4 frag_init_position(v2f_img i) : SV_Target 
    {
        return new_particle_position(i.uv);
    }

    // Kernel 1 - initialize rotation.
    float4 frag_init_rotation(v2f_img i) : SV_Target 
    {
        return new_particle_rotation(i.uv);
    }

    // Kernel 2 - update position.
    float4 frag_update_position(v2f_img i) : SV_Target 
    {
        float dt = unity_DeltaTime.x;
        float4 p = tex2D(_MainTex, i.uv);

        if (p.w > 0)
        {
            p.xyz += get_velocity(p.xyz, i.uv) * dt;
            p.w -= lerp(_LifeScaleParams.x, _LifeScaleParams.y, nrand(i.uv, 4)) * dt;
            return p;
        }
        else
        {
            return new_particle_position(i.uv);
        }
    }

    // Kernel 3 - update rotation.
    float4 frag_update_rotation(v2f_img i) : SV_Target 
    {
        float dt = unity_DeltaTime.x;
        float4 r = tex2D(_MainTex, i.uv);

        // Get the delta rotation quaternion.
        float theta = lerp(_SpeedParams.z, _SpeedParams.w, nrand(i.uv, 15)) * dt;
        float4 dq = float4(get_rotation_axis(i.uv) * sin(theta), cos(theta));

        // Get the unit quaternion from the pixel.
        float4 q = float4(r.xyz, sqrt(1.0 - dot(r.xyz, r.xyz)));

        // Apply the delta rotation, and normalize (to avoid rounding error).
        q = normalize(qmul(dq, q));

        // Feed back the xyz component, with flipping when w < 0.
        r.xyz = q.xyz * sign(q.w);

        return r;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            Fog { Mode off }    
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_init_position
            ENDCG
        }
        Pass
        {
            Fog { Mode off }    
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_init_rotation
            ENDCG
        }
        Pass
        {
            Fog { Mode off }    
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_update_position
            ENDCG
        }
        Pass
        {
            Fog { Mode off }    
            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert_img
            #pragma fragment frag_update_rotation
            ENDCG
        }
    }
}
