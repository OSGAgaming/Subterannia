sampler uImage0 : register(s0);
float2 ScreenResolution = float2(1960, 1080);
int accuracy = 2;

float2 Round(float2 coords, int accuracy)
{
    float2 pixel = float2(ScreenResolution.x / accuracy, ScreenResolution.y / accuracy);
    return float2(floor(coords.x * pixel.x), floor(coords.y * pixel.y)) / pixel;
}

float4 P1(float2 coords : TEXCOORD0) : COLOR0
{
    float4 colour = tex2D(uImage0, Round(coords, accuracy));
    float4 wColour = tex2D(uImage0, Round(coords,2));
    int pixelation = 1;
    int colorlimit = 4;

    float4 ColorD = tex2D(uImage0, Round(coords, 2) + float2(pixelation / ScreenResolution.x, 0));
    float4 ColorR = tex2D(uImage0, Round(coords, 2) + float2(0, pixelation / ScreenResolution.y));
    float4 ColorU = tex2D(uImage0, Round(coords, 2) + float2(-pixelation / ScreenResolution.x, 0));
    float4 ColorL = tex2D(uImage0, Round(coords, 2) + float2(0, -pixelation / ScreenResolution.y));

    float alphaS = abs(ColorL.a - wColour.a) + abs(ColorR.a - wColour.a);
    float alphaV = abs(ColorU.a - wColour.a) + abs(ColorD.a - wColour.a);


    colour.r = floor(colour.r * colorlimit) / colorlimit;
    colour.g = floor(colour.g * colorlimit) / colorlimit;
    colour.b = floor(colour.b * colorlimit) / colorlimit;

    float4 add = clamp((alphaS + alphaV), 0, 10) * float4(1, 1, 1, 1);
    return colour + add;
}

technique Technique1
{
    pass P1
    {
        PixelShader = compile ps_2_0 P1();
    }
}