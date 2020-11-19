Shader "Unlit/Stencil Striped Object"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color1("Color", Color) = (0, 0, 0, 1)
        _Color2("Color", Color) = (1, 1, 1, 1)
        _Tiling("Tiling", Range(1, 500)) = 10
        _Direction("Direction", Range(0, 1)) = 0
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            Cull off
            ZWrite off
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 100

            Stencil
            {
                Ref 1
                Comp equal
            }

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                // make fog work
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    fixed4 color : COLOR;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed4 _Color1;
                fixed4 _Color2;
                int _Tiling;
                float _Direction;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    o.color = v.color;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                     sample the texture
                    fixed4 col = tex2D(_MainTex, i.uv);
                    col *= _Color;
                    // apply fog
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    float pos = lerp(i.uv.x, i.uv.y, _Direction) * _Tiling;
                    return floor(frac(pos) + 0.5);
                }
                ENDCG
            }
        }
}
