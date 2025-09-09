Shader "UI/Unlit/AnimatedCheckerBG"
{
    Properties
    {
        _BgColor       ("Background Color", Color) = (0.07,0.20,0.45,1)
        _CellColor     ("Cell Color (Even)", Color)= (1,1,1,1)
        _AltCellColor  ("Cell Color (Odd)",  Color)= (0.90,0.95,1,1)

        // 形状
        _CellSize      ("Cell Size (UV)", Float)   = 0.08
        _CellGap       ("Cell Gap (0~0.45)", Range(0,0.45)) = 0.20
        _CornerRadius  ("Corner Radius (0~0.5)", Range(0,0.5)) = 0.25

        // 斜向整体移动（棋盘）
        _AngleDeg      ("Angle (deg)", Float)      = 45.0
        _Speed         ("Board Scroll Speed (uv/s)", Float) = 0.15

        // 像素化（0 关闭）
        _PixelSize     ("Pixel Size (px; 0=off)", Float) = 3.0

        // —— 扫光带（与移动方向垂直，沿移动方向扫屏）——
        _StripeSpeed   ("Stripe Speed (uv/s)", Float) = 0.22    // 建议略大于 _Speed
        _StripeSpacing ("Stripe Spacing / Period (uv)", Float) = 0.60
        _StripeOffset  ("Stripe Start Offset (uv)", Float) = 0.0
        _StripeWidth   ("Stripe Half Width (uv)", Float) = 0.18  // 影响范围宽度
        _StripeFalloff ("Stripe Falloff Power", Range(0.5,4)) = 1.8

        // 放大幅度（最大等比增量；缩小时不用单独参数，靠距离权重自然回落）
        _AmpUp         ("Max Scale Up (0~1)", Range(0,1)) = 0.28

        _GlobalAlpha   ("Global Alpha", Range(0,1))= 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="False" }
        ZWrite Off
        ZTest  Always
        Cull   Off
        Blend  SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _BgColor, _CellColor, _AltCellColor;
            float  _CellSize, _CellGap, _CornerRadius;
            float  _AngleDeg, _Speed, _PixelSize, _GlobalAlpha;
            float  _StripeSpeed, _StripeSpacing, _StripeOffset, _StripeWidth, _StripeFalloff, _AmpUp;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f     { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };

            v2f vert (appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o; }

            float2 rotate(float2 p, float rad){ float s=sin(rad), c=cos(rad); return float2(c*p.x - s*p.y, s*p.x + c*p.y); }

            float sdRoundedBox(float2 p, float2 b, float r){
                float2 q = abs(p) - (b - r);
                return length(max(q,0.0)) + min(max(q.x,q.y),0.0) - r;
            }

            // 像素化：直接用 uv*_ScreenParams，避免首帧半屏插值问题（全屏 RawImage 最稳）
            float2 PixelateUV(float2 uv){
                if (_PixelSize <= 0.5) return uv;
                float2 px = uv * _ScreenParams.xy;
                px = floor(px/_PixelSize)*_PixelSize + (_PixelSize*0.5);
                return px / _ScreenParams.xy;
            }

            // 环形/周期距离（沿 u 轴）
            float periodicDist(float x, float center, float period){
                float p = max(period, 1e-3);
                float q = frac((x - center)/p + 0.5) - 0.5; // [-0.5,0.5)
                return abs(q) * p; // [0, p/2]
            }

            fixed4 frag (v2f i):SV_Target
            {
                // 1) 像素化
                float2 uv = PixelateUV(i.uv);

                // 2) 旋转坐标：u 轴 = 移动方向；整体平移（棋盘）
                float2 uv0 = uv - 0.5;
                float  ang = radians(_AngleDeg);
                float2 ruv = rotate(uv0, ang);
                float  t   = _Time.x;
                ruv.x += _Speed * t;

                // 3) 棋盘索引与局部坐标
                float cs       = max(_CellSize, 1e-4);
                float2 cellIdx = floor(ruv / cs);
                float2 local   = frac(ruv / cs) - 0.5;
                float  parity  = fmod(cellIdx.x + cellIdx.y, 2.0);

                // 4) 基础半边长（留白）
                float halfSide = max(0.5 * (1.0 - _CellGap*2.0), 0.05);
                float2 bBase   = float2(halfSide, halfSide);

                // 5) 扫光带：与移动方向垂直，沿 u 方向以 _StripeSpeed 扫屏（可快于棋盘）
                float cellCenterU = (cellIdx.x + 0.5) * cs;
                float sweepCenter = _StripeOffset + _StripeSpeed * t;         // 速度可与 _Speed 不同
                float d           = periodicDist(cellCenterU, sweepCenter, max(_StripeSpacing, cs*1.1));

                // 6) 距离 → 权重：中心 1 → 边缘 0；幂次强调中心
                float w = smoothstep(_StripeWidth, 0.0, d);
                w = pow(saturate(w), _StripeFalloff);

                // 7) 仅按距离放大：scale = 1 + _AmpUp * w
                float scale  = 1.0 + _AmpUp * w;

                // 8) 应用到圆角块（整格等比缩放）
                float2 bScaled = min(bBase * scale, 0.495);
                float  rad     = saturate(_CornerRadius) * min(bScaled.x, bScaled.y);

                float sd = sdRoundedBox(local, bScaled, rad);
                float aa = fwidth(sd) * 1.5;
                float mask = smoothstep(0.0, aa, -sd);

                fixed4 cellCol = (parity < 1.0) ? _CellColor : _AltCellColor;
                fixed4 col = lerp(_BgColor, cellCol, mask);

                col.a *= _GlobalAlpha * i.color.a;
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
