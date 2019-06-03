Shader "ShadowMap/DepthTextureShader"  //生成深度图纹理
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
			//包含头文件
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION; //顶点局部坐标
            };

            struct v2f
            {
                float2 depth : TEXCOORD0;//将深度保存成深度贴图纹理
                float4 vertex : SV_POSITION;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.depth = o.vertex.zw;//保存深度值
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float depth = i.depth.x / i.depth.y;//把Z值深度从视口坐标空间转换位齐次坐标(除以w)
			    fixed4 col = EncodeFloatRGBA(depth);
                return col;
            }
            ENDCG
        }
    }
		FallBack "Diffuse"
}
