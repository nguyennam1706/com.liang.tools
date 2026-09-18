Shader "Percas/UnlitTextureArray"
{
    Properties
    {
        _Textures ("Texture Array", 2DArray) = "" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _TextureIndex ("Texture Index", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM

            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            UNITY_DECLARE_TEX2DARRAY(_Textures);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float, _TextureIndex)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float textureIndex =
                    UNITY_ACCESS_INSTANCED_PROP(Props, _TextureIndex);

                fixed4 tint =
                    UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

                fixed4 tex =
                    UNITY_SAMPLE_TEX2DARRAY(
                        _Textures,
                        float3(i.uv, textureIndex)
                    );

                return tex * tint;
            }

            ENDCG
        }
    }
}