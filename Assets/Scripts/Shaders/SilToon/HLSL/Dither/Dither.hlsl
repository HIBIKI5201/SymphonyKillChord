#ifndef DITHER_INCLUDED
#define DITHER_INCLUDED

float BayerDither(float2 pos)
{
    uint2 p = uint2(pos);
    return (p.x + p.y*3)%5/5.0;
}

#endif
