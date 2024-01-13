Shader "Skybox/CustomSky"
{
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "IgnoreProjector"="True" }
	    Cull Back     // Render side
        ZWrite Off    // Don't draw to depth buffer

        Pass
        {
            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #pragma vertex Vertex
            #pragma fragment Fragment

            // Constants
            #define PI316 0.0596831
            #define PI14 0.07957747
            #define MIEG float3(0.4375f, 1.5625f, 1.5f)
            #define MIE float3(0.0000083721, 0.0000126483, 0.0000190058)
            #define RAYLEIGH float3(0.0000221684, 0.0000517987, 0.0001155897)
            #define RISE 10
            
            #define PI316xRAYLEIGH float3(RAYLEIGH * PI316)
            #define PI14xMIE float3(MIE * PI14)
            #define MIE_ADD_RAYLEIGH float3(MIE + RAYLEIGH)
            
            // Textures
            sampler2D _LightSourceTexture;
            samplerCUBE _StarFieldTexture;
            sampler2D _CloudTexture;

            // Scattering
            half _Kr;
            half _Scattering;
            half _Luminance;
            half _Exposure;
            half3 _RayleighColor;
            half3 _MieColor;
            half3 _LuminanceColor;

            // LightSource
            half _LightSourceTextureSize;
            half _LightSourceTextureIntensity;
            half3 _LightSourceTextureColor;
            float4x4 _LightSourceDirectionMatrix;
            
            // Stars
            half  _StarFieldIntensity;
            half3 _StarFieldColor;
            float4x4 _StarFieldRotationMatrix;

            // Clouds
            half  _CloudAltitude;
            half2 _CloudSpeed;
            half  _CloudDensity;
            half3 _CloudColor1;
            half3 _CloudColor2;
            half _CloudEdge1;
            half _CloudEdge2;

            half _IsDay;

            // Mesh data
            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            // Vertex to fragment
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 lightSourceTexPos : TEXCOORD1;
                float3 starTexPos  : TEXCOORD2;
            };

            // Vertex shader
            Varyings Vertex (Attributes v)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(v.positionOS);
                output.positionWS = TransformObjectToWorld(v.positionOS);

                // Outputs
                output.lightSourceTexPos = mul(v.positionOS, (float3x3)_LightSourceDirectionMatrix);
                output.lightSourceTexPos = output.lightSourceTexPos / _LightSourceTextureSize + 0.5;
                
                output.starTexPos = mul((float3x3)_StarFieldRotationMatrix, v.positionOS);

                return output;
            }

            // Fragment shader
            float4 Fragment (Varyings input) : SV_Target
            {
                // Directions
                half3 viewDir = normalize(input.positionWS);

                half3 lightDirection = _MainLightPosition.xyz;
                half lightCosTheta = dot(viewDir, lightDirection);
                half rise = saturate(lightDirection.y * RISE);
                
                // Optical depth
                half saturatedViewDirY = saturate(viewDir.y);
                half z = saturatedViewDirY + 0.00094f * pow(1.63860f - acos(saturatedViewDirY), -1.253f);

                // Extinction
                half3 fex = exp(-(RAYLEIGH * _Kr / z  + 1000.0f * MIE / z));
                half3 revertFex = 1 - fex;
                half starExtinction = saturate(viewDir.y * 1000.0f) * fex.b;
                half moonExtinction = saturate(viewDir.y * 2.5f);
                half3 rayleightColor = PI316xRAYLEIGH * _RayleighColor;
                half3 mieColor = PI14xMIE * _MieColor;

                half rayPhase = 2.0 + 0.5 * pow(lightCosTheta, 2.0);
                half miePhase = MIEG.x / pow(abs(MIEG.y - MIEG.z * lightCosTheta), 1.5);
                half3 brTheta = rayPhase * rayleightColor;
                half3 bmTheta = miePhase * mieColor * rise;
                half3 brmTheta = (brTheta + bmTheta) / MIE_ADD_RAYLEIGH;
                half3 esun = _IsDay > 0.5 ? lerp(fex, revertFex, clamp(lightDirection.y, 0.0, 0.5)) : revertFex;
                half3 scatter = brmTheta * esun * _Scattering * revertFex;
                scatter *= rise;

                // Sky
                brmTheta = brTheta / MIE_ADD_RAYLEIGH;
                half3 skyLuminance = brmTheta * _LuminanceColor * _Luminance * revertFex;

                // Dynamic Clouds
                half2 cloudPos = normalize(half3(input.positionWS.x, input.positionWS.y / _CloudAltitude, input.positionWS.z)).xz;
                
                // 备用代码, 不要删除
                // 下方的云层UV计算方式可以使云层贴图在天空盒中以水平的方式铺开
                // half2 cloudPos = half2(viewDir.x, viewDir.z) / viewDir.y * _CloudAltitude;

                half2 cloudUV = cloudPos * 0.25 - 0.005 + _CloudSpeed * _Time.x;
                half4 tex1 = tex2D(_CloudTexture, cloudUV);
                cloudUV = cloudPos * 0.35 - 0.0065 + _CloudSpeed * _Time.x;
                half4 tex2 = tex2D(_CloudTexture, cloudUV);
                half noise1 = pow(abs(tex1.g + tex2.g), 0.1);
                half noise2 = pow(abs(tex2.b * tex1.r), 0.25);
                half cloudDensity = pow(noise1 * noise2, _CloudDensity);
                half cloudAlpha = saturate(cloudDensity);
                half mixCloud = saturate(smoothstep(_CloudEdge1, _CloudEdge2, viewDir.y) * cloudDensity);
                half3 cloud1 = lerp(_CloudColor1, 0, noise1);
                half3 cloud2 = lerp(_CloudColor1, _CloudColor2, noise2) * 2.5;
                half3 cloud = lerp(cloud1, cloud2, noise1 * noise2);

                half oneSide = step(input.lightSourceTexPos.z, 0);
                half4 lightSourceTex = tex2D(_LightSourceTexture, input.lightSourceTexPos.xy);
                lightSourceTex.rgb *= oneSide * _LightSourceTextureColor * _LightSourceTextureIntensity;
                lightSourceTex.rgb = pow(abs(lightSourceTex.rgb), lerp(1, 2, _IsDay));
                lightSourceTex.rgb *= lerp(moonExtinction, fex.b, _IsDay);

                half starsMask = 1.0 - lightSourceTex.a * oneSide;

                // Starfield
                half3 starTex = texCUBE(_StarFieldTexture, input.starTexPos).rgb;
                starTex *= starsMask * starExtinction * _StarFieldIntensity * _StarFieldColor;

                // Output
                half3 outputColor = scatter + skyLuminance + (lightSourceTex.rgb + starTex) * (1 - cloudAlpha);

                outputColor = lerp(outputColor, cloud, mixCloud);

                // Tonemapping
                outputColor = saturate(1.0 - exp(_Exposure * outputColor));
                return half4(outputColor, 1.0);
            }
            
            ENDHLSL
        }
    }
}