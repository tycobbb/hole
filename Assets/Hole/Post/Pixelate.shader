Shader "Custom/Pixelate" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Density ("Pixel Density", Int) = 80
    }

    SubShader {
        // no culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex vert
            #pragma fragment frag

            // -- includes --
            #include "UnityCG.cginc"

            // -- props --
            sampler2D _MainTex;
            int _Density;
            float2 _AspectRatio;

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertOut {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // -- program --
            VertOut vert(VertIn v) {
                VertOut o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(VertOut i) : SV_Target {
                const float2 scale = _Density * _AspectRatio;
                i.uv = round(i.uv * scale)/ scale;
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
