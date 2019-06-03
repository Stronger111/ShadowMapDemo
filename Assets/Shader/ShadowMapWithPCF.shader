Shader "ShadowMap/ShadowMapWithPCF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
	    //阴影偏移值
	    _Bias("Bias",Range(0.005,0.05))=0.005
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float4 vertex : SV_POSITION;  //视口坐标下的顶点坐标
				float4 worldPos:TEXCOORD0;
				float2 uv : TEXCOORD1;
            };
			//变量声明
            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4x4 _LightSpaceMatrix; //光空间变换矩阵,将每个世界坐标变换到光源所看见的空间
			sampler2D _DepthTexture;//深度贴图
			half _Bias;//阴影偏移值
			float _TexturePixelWidth;//深度贴图宽度
			float _TexturePixelHeight; //深度贴图高度

            v2f vert (appdata v)
            {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//将顶点从局部坐标系转换到世界坐标系
				float4 worldPos = mul(UNITY_MATRIX_M, v.vertex);
				o.worldPos.xyz = worldPos.xyz;
				o.worldPos.w = 1;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
			 // sample the texture
		     fixed4 col = tex2D(_MainTex, i.uv);
			//将顶点从世界坐标系转换到光空间坐标系下
			fixed4 lightSpacePos = mul(_LightSpaceMatrix,i.worldPos);
			//将光空间片元的位置转换位NDC(裁切空间的标准化设备坐标)
			lightSpacePos.xyz = lightSpacePos.xyz / lightSpacePos.w;
			//将NDC坐标从[-1,1]转换到[0,1]
			float3 pos = lightSpacePos * 0.5 + 0.5;
			//*****计算阴影值*************
			float shadow = 0.0;  //阴影值.1是在阴影中0为不在
			//获取当前深度
			float currentDepth = lightSpacePos.z;
			//应用PCF求阴影值
			float2 texelSize = float2(1.0/_TexturePixelWidth,1.0/ _TexturePixelHeight);//一个纹理像素大小
			for (int x=-1;x<=1;x++) 
			{
				for (int y = -1; y <= 1;y++) {
					float2 samplePos = pos.xy + float2(x, y)*texelSize;//caiyang 坐标
					fixed4 pcfDepthRGBA = tex2D(_DepthTexture, samplePos);
					float pcfDepth = DecodeFloatRGBA(pcfDepthRGBA);
					shadow += currentDepth + _Bias < pcfDepth ? 1.0 : 0.0;
				}
			}
			shadow /= 9.0;
            return (1 - shadow)*col;
            }
            ENDCG
        }
    }
			FallBack "Diffuse"
}
