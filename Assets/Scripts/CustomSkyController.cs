using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CustomSkyController : MonoBehaviour
{
    private Material skyboxMat;

    public Light mainLight;

    [Range(5f, 20f)]
    public float kr = 8.4f;

    [ColorUsage(false, false)]
    public Color rayleighColor = Color.white;

    [ColorUsage(false, false)]
    public Color mieColor = Color.white;

    [Range(1, 25)]
    public float scattering = 1;

    [ColorUsage(false, false)]
    public Color luminanceColor = Color.white;

    [Range(0, 5)]
    public float luminance = 1;
    
    public bool isDay = true;

    public Texture lightSourceTexture;

    [Range(0.1f, 2f)]
    public float lightSourceTextureSize = 1;

    [Range(0f, 10f)]
    public float lightSourceTextureIntensity = 1;

    [ColorUsage(false, false)]
    public Color lightSourceTextureColor = Color.white;
    
    public Cubemap starFieldTexture;
    
    public Vector3 starFieldRotation;

    [Range(0f, 5f)]
    public float starFieldIntensity = 1;

    [ColorUsage(false, false)]
    public Color starFieldColor = Color.white;

    public Texture cloudTexture;

    [Range(0f, 1f)]
    public float cloudDensity = 0.5f;
    
    [Range(0f, 0.5f)]
    public float cloudAltitude = 1;
    
    public Vector2 cloudSpeed;
    
    [ColorUsage(false, false)]
    public Color cloudColor1 = Color.white;
    
    [ColorUsage(false, false)]
    public Color cloudColor2 = Color.white;

    [Range(0, 1f)]
    public float cloudEdge = 0.1f;

    [Range(0.01f, 0.5f)]
    public float cloudEdgeRange = 0.1f;
    
    [Range(0f, 10f)]
    public float exposure = 2.0f;

    private static readonly int KrID = Shader.PropertyToID("_Kr");
    private static readonly int RayleighColorID = Shader.PropertyToID("_RayleighColor");
    private static readonly int MieColorID = Shader.PropertyToID("_MieColor");
    private static readonly int ScatteringID = Shader.PropertyToID("_Scattering");
    private static readonly int LuminanceColorID = Shader.PropertyToID("_LuminanceColor");
    private static readonly int LuminanceID = Shader.PropertyToID("_Luminance");
    
    private static readonly int LightSourceTextureID = Shader.PropertyToID("_LightSourceTexture");
    private static readonly int LightSourceTextureSizeID = Shader.PropertyToID("_LightSourceTextureSize");
    private static readonly int LightSourceTextureIntensityID = Shader.PropertyToID("_LightSourceTextureIntensity");
    private static readonly int LightSourceTextureColorID = Shader.PropertyToID("_LightSourceTextureColor");
    private static readonly int LightSourceDirectionMatrixID = Shader.PropertyToID("_LightSourceDirectionMatrix");

    private static readonly int StarFieldTextureID = Shader.PropertyToID("_StarFieldTexture");
    private static readonly int StarFieldRotationMatrixID = Shader.PropertyToID("_StarFieldRotationMatrix");
    private static readonly int StarFieldIntensityID = Shader.PropertyToID("_StarFieldIntensity");
    private static readonly int StarFieldColorID = Shader.PropertyToID("_StarFieldColor");
    
    private static readonly int CloudTextureID = Shader.PropertyToID("_CloudTexture");
    private static readonly int CloudDensityID = Shader.PropertyToID("_CloudDensity");
    private static readonly int CloudAltitudeID = Shader.PropertyToID("_CloudAltitude");
    private static readonly int CloudSpeedID = Shader.PropertyToID("_CloudSpeed");
    private static readonly int CloudColor1ID = Shader.PropertyToID("_CloudColor1");
    private static readonly int CloudColor2ID = Shader.PropertyToID("_CloudColor2");
    private static readonly int CloudEdge1ID = Shader.PropertyToID("_CloudEdge1");
    private static readonly int CloudEdge2ID = Shader.PropertyToID("_CloudEdge2");
    
    private static readonly int ExposureID = Shader.PropertyToID("_Exposure");
    private static readonly int IsDayID = Shader.PropertyToID("_IsDay");

    private static readonly Matrix4x4 DefaultRotate = new Matrix4x4()
    {
        [0] = 1, [1] = 0, [2] = 0, [3] = 0,
        [4] = 0, [5] = 1, [6] = 0, [7] = 0,
        [8] = 0, [9] = 0, [10] = 1, [11] = 0,
        [12] = 0, [13] = 0, [14] = 0, [15] = 1
    };

    private void OnEnable()
    {
        if (!skyboxMat)
        {
            skyboxMat = new Material(Shader.Find("Skybox/CustomSky"));
        }
        UpdateMaterial();
        RenderSettings.skybox = skyboxMat;
    }

    private void Update()
    {
#if UNITY_EDITOR
        UpdateMaterial();
#endif
    }

    private void UpdateMaterial()
    {
        skyboxMat.SetFloat(KrID, kr * 1000f);
        skyboxMat.SetColor(RayleighColorID, rayleighColor.linear);
        skyboxMat.SetColor(MieColorID, mieColor.linear);
        skyboxMat.SetFloat(ScatteringID, scattering * (isDay ? 1 : 0.1f));
        skyboxMat.SetColor(LuminanceColorID, luminanceColor.linear);
        skyboxMat.SetFloat(LuminanceID, luminance);

        skyboxMat.SetFloat(IsDayID, isDay ? 1 : 0);
        skyboxMat.SetMatrix(LightSourceDirectionMatrixID, GetLightSourceDirectionMatrix());
        skyboxMat.SetTexture(LightSourceTextureID, lightSourceTexture);
        skyboxMat.SetFloat(LightSourceTextureSizeID, lightSourceTextureSize);
        skyboxMat.SetFloat(LightSourceTextureIntensityID, lightSourceTextureIntensity);
        skyboxMat.SetColor(LightSourceTextureColorID, lightSourceTextureColor.linear);

        skyboxMat.SetTexture(StarFieldTextureID, starFieldTexture);
        skyboxMat.SetMatrix(StarFieldRotationMatrixID, Matrix4x4.Rotate(quaternion.Euler(starFieldRotation)));
        skyboxMat.SetFloat(StarFieldIntensityID, starFieldIntensity);
        skyboxMat.SetColor(StarFieldColorID, starFieldColor.linear);

        skyboxMat.SetTexture(CloudTextureID, cloudTexture);
        skyboxMat.SetFloat(CloudDensityID, Mathf.Lerp(25f, 0, cloudDensity));
        skyboxMat.SetFloat(CloudAltitudeID, cloudAltitude);
        skyboxMat.SetVector(CloudSpeedID, cloudSpeed);
        skyboxMat.SetColor(CloudColor1ID, cloudColor1.linear);
        skyboxMat.SetColor(CloudColor2ID, cloudColor2.linear);
        skyboxMat.SetFloat(CloudEdge1ID, cloudEdge);
        skyboxMat.SetFloat(CloudEdge2ID, cloudEdge + cloudEdgeRange);

        skyboxMat.SetFloat(ExposureID, -exposure);
    }

    private Matrix4x4 GetLightSourceDirectionMatrix()
    {
        Matrix4x4 matrix = mainLight == null ? DefaultRotate : Matrix4x4.Rotate(mainLight.transform.rotation);
        return matrix;
    }
}