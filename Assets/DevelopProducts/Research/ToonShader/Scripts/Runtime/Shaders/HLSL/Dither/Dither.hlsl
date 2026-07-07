#ifndef DITHER_INCLUDED
#define DITHER_INCLUDED

float BayerDither(float2 pos)
{
    pos /= 1;
    float2 uv = pos.xy * pos.xy;
    float DITHER_THRESHOLDS[16] =
    {
        12.0 / 17.0, 5.0 / 17.0, 6.0 / 17.0, 13.0 / 17.0,
     4.0 / 17.0, 0.0 / 17.0, 1.0 / 17.0, 7.0 / 17.0,
    11.0 / 17.0, 3.0 / 17.0, 2.0 / 17.0, 8.0 / 17.0,
    15.0 / 17.0, 10.0 / 17.0, 9.0 / 17.0, 14.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    return DITHER_THRESHOLDS[index];
}

#endif