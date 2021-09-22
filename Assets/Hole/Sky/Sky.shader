Shader "Custom/Sky" {
    Properties {
        _Iterations ("Iterations", Int) = 17
        _Magic("Magic", Float) = 0.53
        _VolSteps("Volume Steps", Int) = 20
        _StepSize("Step Size", Float) = 0.1

        _Brightness("Brightness", Float) = 0.0015
        _Saturation("Saturation", Float) = 0.850
        _DarkMatter("Dark Matter", Float) = 0.300
        _DistFading("Fade Distance", Float) = 0.730
    }

    SubShader {
        Tags {
            "RenderType"="Opaque"
            "PreviewType"="Plane"
        }

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex vert
            #pragma fragment frag

            // -- includes --
            #include "UnityCG.cginc"

            // -- uniforms --
            int _Iterations;
            float _Magic;
            int _VolSteps;
            float _StepSize;

            float _Brightness;
            float _Saturation;
            float _DarkMatter;
            float _DistFading;

            // -- types --
            struct VertIn {
                float4 vert : POSITION;
            };

            struct VertOut {
                float4 pos : SV_POSITION;
                float3 dirView : TEXCOORD0;
            };

            // -- helpers --
            float3 mix(float3 x, float3 y, float a) {
                return x * (1 - a) + y * a;
            }

            // -- program --
            VertOut vert(VertIn v) {
                VertOut o;
                o.pos = UnityObjectToClipPos(v.vert);
                o.dirView = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vert)));
                return o;
            }

            float4 frag(VertOut i) : SV_Target {
                float3 from = float3(1.0, 0.5, -1.5);

                float s = 0.1;
                float fade = 1.0;
                float3 v = float3(0.0, 0.0, 0.0);

                for (int r = 0; r < _VolSteps; r++) {
                    float3 p = from + s * i.dirView * 0.5;
                    float pa = 0.0;
                    float a = 0.0;

                    for (int i = 0; i < _Iterations; i++) {
                        p = abs(p) / dot(p, p) - _Magic; // the magic formula
                        a += abs(length(p) - pa); // absolute sum of average change
                        pa = length(p);
                    }

                    float dm = max(0.0, _DarkMatter - a * a * 0.001); // dark matter
                    a *= a * a; // add contrast
                    if (r>6) {
                        fade *= 1.0 - dm; // dark matter, don't render near
                    }

                    v += fade;
                    v += float3(s, s * s, s * s * s * s) * a * _Brightness * fade; // coloring based on distance
                    fade *= _DistFading; // distance fading
                    s += _StepSize;
                }

                v = mix(float3(length(v), 0.0, 0.0), v, _Saturation); // color adjust

                return float4(v * 0.001, 1.0);
            }
            ENDCG
        }
    }
}
