using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureDepth : MonoBehaviour
{
    private List<Vector4> _vList = new List<Vector4>();
    /// <summary>
    /// 光源深度图
    /// </summary>
    public RenderTexture depthTexture;
    private Camera mCam;
    private Shader mSampleDepthShader;
    // Update is called once per frame
    private Camera mainCamera;
    void Update()
    {
        mCam = GetComponent<Camera>();
        if (mSampleDepthShader == null)
            mSampleDepthShader = Shader.Find("ShadowMap/DepthTextureShader");

        if (mCam != null)
        {
            mCam.backgroundColor = Color.black;
            mCam.clearFlags = CameraClearFlags.Color;
            mCam.targetTexture = depthTexture;
            mCam.enabled = false;

            Shader.SetGlobalTexture("_DepthTexture", depthTexture);
            Shader.SetGlobalFloat("_TexturePixelWidth", depthTexture.width);
            Shader.SetGlobalFloat("_TexturePixelHeight", depthTexture.height);
            mCam.RenderWithShader(mSampleDepthShader, "RenderType");
        }
        foreach (Camera item in Camera.allCameras)
        {
            if (item.CompareTag("MainCamera"))
            {
                mainCamera = item;
                break;
            }
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null && mCam != null)
        {
            SetLightCamera(mainCamera, mCam);
        }

    }
    /// <summary>
    /// 根据主摄像机,设置灯光摄像机
    /// </summary>
    /// <param name="mainCamera"></param>
    /// <param name="lightCamera"></param>
    void SetLightCamera(Camera mainCamera, Camera lightCamera)
    {
        //1求视锥8顶点,主相机空间中） n平面（aspect * y, tan(r/2)* n,n）  f平面（aspect*y, tan(r/2) * f, f）
        float r = (mainCamera.fieldOfView / 180f) * Mathf.PI;
        //n平面
        Vector4 nLeftUp = new Vector4(-mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.nearClipPlane, Mathf.Tan(r / 2) * mainCamera.nearClipPlane, mainCamera.nearClipPlane, 1);
        Vector4 nRightUp = new Vector4(mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.nearClipPlane, Mathf.Tan(r / 2) * mainCamera.nearClipPlane, mainCamera.nearClipPlane, 1);
        Vector4 nLeftDonw = new Vector4(-mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.nearClipPlane, -Mathf.Tan(r / 2) * mainCamera.nearClipPlane, mainCamera.nearClipPlane, 1);
        Vector4 nRightDonw = new Vector4(mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.nearClipPlane, -Mathf.Tan(r / 2) * mainCamera.nearClipPlane, mainCamera.nearClipPlane, 1);
        //f平面
        Vector4 fLeftUp = new Vector4(-mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.farClipPlane, Mathf.Tan(r / 2) * mainCamera.farClipPlane, mainCamera.farClipPlane, 1);
        Vector4 fRightUp = new Vector4(mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.farClipPlane, Mathf.Tan(r / 2) * mainCamera.farClipPlane, mainCamera.farClipPlane, 1);
        Vector4 fLeftDonw = new Vector4(-mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.farClipPlane, -Mathf.Tan(r / 2) * mainCamera.farClipPlane, mainCamera.farClipPlane, 1);
        Vector4 fRightDonw = new Vector4(mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.farClipPlane, -Mathf.Tan(r / 2) * mainCamera.farClipPlane, mainCamera.farClipPlane, 1);

        //2、将8个顶点变换到世界空间
        Matrix4x4 mainv2w = mainCamera.transform.localToWorldMatrix;
        Vector4 wnLeftUp = mainv2w * nLeftUp;
        Vector4 wnRightUp = mainv2w * nRightUp;
        Vector4 wnLeftDonw = mainv2w * nLeftDonw;
        Vector4 wnRightDonw = mainv2w * nRightDonw;
        //
        Vector4 wfLeftUp = mainv2w * fLeftUp;
        Vector4 wfRightUp = mainv2w * fRightUp;
        Vector4 wfLeftDonw = mainv2w * fLeftDonw;
        Vector4 wfRightDonw = mainv2w * fRightDonw;

        //将灯光相机设置在mainCamera视锥中心
        Vector4 nCenter = (wnLeftUp + wnRightUp + wnLeftDonw + wnRightDonw) / 4f;
        Vector4 fCenter = (wfLeftUp + wfRightUp + wfLeftDonw + wfRightDonw) / 4f;
        lightCamera.transform.position = (nCenter + fCenter) / 2f;
        //3、	求光view矩阵
        Matrix4x4 lgihtw2v = lightCamera.transform.worldToLocalMatrix;
        //4、	把顶点从世界空间变换到光view空间
        Vector4 vnLeftUp = lgihtw2v * wnLeftUp;
        Vector4 vnRightUp = lgihtw2v * wnRightUp;
        Vector4 vnLeftDonw = lgihtw2v * wnLeftDonw;
        Vector4 vnRightDonw = lgihtw2v * wnLeftDonw;
        //
        Vector4 vfLeftUp = lgihtw2v * wfLeftUp;
        Vector4 vfRightUp = lgihtw2v * wfRightUp;
        Vector4 vfLeftDonw = lgihtw2v * wfLeftDonw;
        Vector4 vfRightDonw = lgihtw2v * wfRightDonw;
        _vList.Clear();
        _vList.Add(vnLeftUp);
        _vList.Add(vnRightUp);
        _vList.Add(vnLeftDonw);
        _vList.Add(vnRightDonw);

        _vList.Add(vfLeftUp);
        _vList.Add(vfRightUp);
        _vList.Add(vfLeftDonw);
        _vList.Add(vfRightDonw);
        //5、	求包围盒 (由于光锥xy轴的对称性，这里求最大包围盒就好，不是严格意义的AABB)
        float maxX = -float.MaxValue;
        float maxY = -float.MaxValue;
        float maxZ = -float.MaxValue;
        float minZ = float.MaxValue;
        for (int i = 0; i < _vList.Count; i++)
        {
            Vector4 v = _vList[i];
            if (Mathf.Abs(v.x) > maxX)
            {
                maxX = Mathf.Abs(v.x);
            }
            if (Mathf.Abs(v.y) > maxY)
            {
                maxY = Mathf.Abs(v.y);
            }
            if (v.z > maxZ)
            {
                maxZ = v.z;
            }
            else if (v.z < minZ)
            {
                minZ = v.z;
            }
        }
        //5.5 优化，如果8个顶点在光锥view空间中的z<0,那么如果n=0，就可能出现应该被渲染depthmap的物体被光锥近裁面剪裁掉的情况，所以z < 0 的情况下要延光照负方向移动光源位置以避免这种情况
        if (minZ < 0)
        {
            lightCamera.transform.position += -lightCamera.transform.forward.normalized * Mathf.Abs(minZ);
            maxZ = maxZ - minZ;
        }

        //6、	根据包围盒确定投影矩阵 包围盒的最大z就是f，Camera.orthographicSize由y max决定 ，还要设置Camera.aspect
        lightCamera.orthographic = true;
        lightCamera.aspect = maxX / maxY;
        lightCamera.orthographicSize = maxY;
        lightCamera.nearClipPlane = 0.0f;
        lightCamera.farClipPlane = Mathf.Abs(maxZ);
    }
}
