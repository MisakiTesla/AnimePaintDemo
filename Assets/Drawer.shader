Shader "Unlit/Drawer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RegionsTexture ("RegionsTexture", 2D) = "white" {}
        [IntRange]_SelectedRegion ("SelectedRegion", Range(-1,32)) = 0
        _SelectedColor ("SelectedColor", Color) = (1,1,1,1)
        //TODO Pack into vector4
        _TouchPos ("_TouchPos", vector) = (0.5,0.5,0,0)
        _Radius ("_Radius", Range(0,1)) = 0

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        
//        Cull[_CullMode]
        ZWrite Off
        Lighting Off
        Fog { Mode Off }
//    ZTest Less
        Blend One OneMinusSrcAlpha
//    ColorMask[_ColorMask]

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _RegionsTexture;
            float4 _MainTex_ST;
            int _SelectedRegion;
            float4 _SelectedColor;
            float4 _TouchPos;
            float _Radius;

            int round(float rInput) {
                float fInput = floor(rInput);
                float cInput = ceil(rInput);
                if (abs(fInput - rInput) > abs(cInput - rInput)) {
                    return int(fInput);
                } else {
                    return int(cInput);
                }
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);


                fixed4 regionColor = tex2D(_RegionsTexture, i.uv);
                //0~1 to 0~255
                fixed4 bytes = regionColor * 255.0;
                //decode region
                int regionIndex = round(bytes.r) * 65536 + round(bytes.g) * 256 + round(bytes.b);

                if (regionIndex < _SelectedRegion)
                {
                    // return col;
                }
                if (regionIndex == _SelectedRegion)
                {
                    float dist = distance(i.uv,_TouchPos.xy);
                    col = _SelectedColor * (dist < _Radius);
                }
                if (regionIndex > _SelectedRegion)
                {
                    col = fixed4(0.0, 0.0, 0.0, 0.0);
                }
                return col;
            }
            ENDCG
        }
    }
}
