Shader "ShadowMap/ShadowMapNormal"
{
    Properties
    {
		//主纹理
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
			//声明顶点着色器和片段着色器
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4x4 _LightSpaceMatrix;//光空间变换矩阵,将每个世界坐标变换到光源所在的空间
			sampler2D _DepthTexture;//深度贴图

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				//将顶点从局部坐标系转换到世界坐标系
				float4 worldPos = mul(UNITY_MATRIX_M,v.vertex);
				o.worldPos.xyz = worldPos.xyz;
				o.worldPos.w = 1;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
			//定义片段着色器
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
				//获取深度贴图颜色值
				fixed4 depthRGBA = tex2D(_DepthTexture,pos.xy);
				//获取深度贴图的深度
				float depth = DecodeFloatRGBA(depthRGBA);
				//获取当前像素深度值
				float currentDepth = lightSpacePos.z;
				shadow = currentDepth < depth ? 1.0 : 0.0;
				//(1-1)位黑色阴影
                return (1 - shadow) * col;
            }
            ENDCG
        }
    }
   FallBack "Diffuse"
}
