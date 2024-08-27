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
    int pixelation = 2;
    float4 colour = tex2D(uImage0, Round(coords, accuracy));
    float4 wColour = tex2D(uImage0, Round(coords,2));
    int colorlimit = 6d;
    float colorCurve = 1;

    float4 ColorD = tex2D(uImage0, Round(coords, 2) + float2(pixelation / ScreenResolution.x, 0));
    float4 ColorR = tex2D(uImage0, Round(coords, 2) + float2(0, pixelation / ScreenResolution.y));
    float4 ColorU = tex2D(uImage0, Round(coords, 2) + float2(-pixelation / ScreenResolution.x, 0));
    float4 ColorL = tex2D(uImage0, Round(coords, 2) + float2(0, -pixelation / ScreenResolution.y));

    float alphaS = abs(ColorL.a - wColour.a) + abs(ColorR.a - wColour.a);
    float alphaV = abs(ColorU.a - wColour.a) + abs(ColorD.a - wColour.a);

    colour.r = floor((pow(1 + colour.r,colorCurve) - 1) * colorlimit) / colorlimit;
    colour.g = floor((pow(1 + colour.g,colorCurve) - 1) * colorlimit) / colorlimit;
    colour.b = floor((pow(1 + colour.b,colorCurve) - 1) * colorlimit) / colorlimit;

    float colorOptionsR[6] = { 0, 28, 54, 139, 177, 255 }; 
    float colorOptionsG[6] = { 0, 28, 72, 84, 140, 255 };
    float colorOptionsB[6] = { 0, 37, 64, 96, 146, 255 };

    int noOfColors = 6;

    colour.r = colorOptionsR[clamp(floor(noOfColors * colour.r), 0, noOfColors - 1)] / 255;
    colour.g = colorOptionsG[clamp(floor(noOfColors * colour.g), 0, noOfColors - 1)] / 255;
    colour.b = colorOptionsB[clamp(floor(noOfColors * colour.b), 0, noOfColors - 1)] / 255;

    float4 add = clamp((alphaS + alphaV), 0, 10) * float4(1, 1, 1, 1);
    return (colour + add) * colour.a;
}

technique Technique1
{
    pass P1
    {
        PixelShader = compile ps_3_0 P1();
    }
}