#ifndef NOISE_INCLUDED
#define NOISE_INCLUDED

float2 GradientNoise_dir(float2 p)
{
    p = p % 289;
    float x = (34 * p.x + 1) * p.x % 289 + p.y;
    x = (34 * x + 1) * x % 289;
    x = frac(x / 41) * 2 - 1;
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

float GradientNoise(float2 p)
{
    float2 ip = floor(p);
    float2 fp = frac(p);
    float d00 = dot(GradientNoise_dir(ip), fp);
    float d01 = dot(GradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
    float d10 = dot(GradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
    float d11 = dot(GradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
    fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
    return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
}

float GradientNoise(float2 UV, float Scale)
{
    return GradientNoise(UV * Scale) + 0.5;
}



#endif