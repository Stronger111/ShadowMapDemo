using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureDepth : MonoBehaviour
{
    /// <summary>
    /// 光源深度图
    /// </summary>
    public RenderTexture depthTexture;
    private Camera mCam;
    private Shader mSampleDepthShader;
    // Update is called once per frame
    void Update()
    {
        mCam = GetComponent<Camera>();
        if (mSampleDepthShader == null)
            mSampleDepthShader = Shader.Find("ShadowMap/DepthTextureShader");

        if(mCam!= null)
        {
            mCam.backgroundColor = Color.black;
            mCam.clearFlags = CameraClearFlags.Color;
            mCam.targetTexture = depthTexture;
            mCam.enabled = false;

            Shader.SetGlobalTexture("_DepthTexture", depthTexture);
            Shader.SetGlobalFloat("_TexturePixelWidth", depthTexture.width);
            Shader.SetGlobalFloat("_TexturePixelHeight", depthTexture.height);
            mCam.RenderWithShader(mSampleDepthShader,"RenderType");

        }
    }
}
