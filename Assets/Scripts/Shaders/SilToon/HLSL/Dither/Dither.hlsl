#ifndef DITHER_INCLUDED
#define DITHER_INCLUDED

half BayerDither(float2 pos)
{
    uint2 p = uint2(pos);
    return half((p.x + p.y*3)%5) / 5.0h;
}

#endif
