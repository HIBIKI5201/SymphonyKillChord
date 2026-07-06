#ifndef DITHER_INCLUDED
#define DITHER_INCLUDED

float BayerDither(float2 pos)
{
    static const float DITHER_THRESHOLDS[16] =
    {
        12.0 / 17.0,  5.0 / 17.0,  6.0 / 17.0, 13.0 / 17.0,
         4.0 / 17.0,  0.0 / 17.0,  1.0 / 17.0,  7.0 / 17.0,
        11.0 / 17.0,  3.0 / 17.0,  2.0 / 17.0,  8.0 / 17.0,
        15.0 / 17.0, 10.0 / 17.0,  9.0 / 17.0, 14.0 / 17.0
    };
    uint2 p = uint2(pos) & 3;
    return DITHER_THRESHOLDS[p.x * 4 + p.y];
}

#endif
