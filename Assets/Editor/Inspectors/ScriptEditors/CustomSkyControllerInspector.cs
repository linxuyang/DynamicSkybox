using UnityEngine;

namespace UnityEditor
{
    [CustomEditor(typeof(CustomSkyController))]
    public class CustomSkyControllerInspector : Editor
    {
        private SerializedProperty krProperty;
        private SerializedProperty rayleighColorProperty;
        private SerializedProperty mieColorProperty;
        private SerializedProperty scatteringProperty;
        private SerializedProperty luminanceColorProperty;
        private SerializedProperty luminanceProperty;
        private SerializedProperty isDayProperty;
        private SerializedProperty mainLightProperty;
        private SerializedProperty lightSourceTextureProperty;
        private SerializedProperty lightSourceTextureSizeProperty;
        private SerializedProperty lightSourceTextureIntensityProperty;
        private SerializedProperty lightSourceTextureColorProperty;
        private SerializedProperty starFieldTextureProperty;
        private SerializedProperty starFieldRotationProperty;
        private SerializedProperty starFieldIntensityProperty;
        private SerializedProperty starFieldColorProperty;
        private SerializedProperty cloudTextureProperty;
        private SerializedProperty cloudDensityProperty;
        private SerializedProperty cloudAltitudeProperty;
        private SerializedProperty cloudSpeedProperty;
        private SerializedProperty cloudColor1Property;
        private SerializedProperty cloudColor2Property;
        private SerializedProperty cloudEdgeProperty;
        private SerializedProperty cloudEdgeRangeProperty;
        private SerializedProperty exposureProperty;

        private SavedBool luminanceScatterFoldout;
        private SavedBool lightSourceFoldout;
        private SavedBool starFieldFoldout;
        private SavedBool cloudFoldout;

        private readonly GUIContent krGUIContent = new GUIContent("大气厚度", "影响光源在大气中开始散射的高度");
        private readonly GUIContent scatteringGUIContent = new GUIContent("光源散射强度", "影响光源在大气中散射反射的程度");
        private readonly GUIContent rayleighColorGUIContent = new GUIContent("大气颜色", "影响光源在大气漫反射和散射的颜色");
        private readonly GUIContent mieColorGUIContent = new GUIContent("直接散射", "影响光源直接散射产生的光晕的颜色");
        private readonly GUIContent luminanceColorGUIContent = new GUIContent("环境色调", "整体环境色调");
        private readonly GUIContent luminanceGUIContent = new GUIContent("环境亮度", "整体环境亮度");
        private readonly GUIContent isDayGUIContent = new GUIContent("昼夜", "切换白天或夜晚会影响Shader内部的某些计算公式");
        private readonly GUIContent mainLightGUIContent = new GUIContent("场景主灯(方向光)", "天空盒会根据场景主灯的方向渲染光源天体以及光源周围的高亮区域");
        private readonly GUIContent lightSourceTextureGUIContent = new GUIContent("光源贴图", "天空中光源的贴图, 需要透明度(光源图案内部不透明, 外部全透明)");
        private readonly GUIContent lightSourceTextureSizeGUIContent = new GUIContent("光源大小", "调整光源贴图在天空中的大小");
        private readonly GUIContent lightSourceTextureIntensityGUIContent = new GUIContent("光源亮度", "调整光源贴图在天空中的亮度");
        private readonly GUIContent lightSourceTextureColorGUIContent = new GUIContent("光源颜色", "调整光源贴图的颜色");
        private readonly GUIContent starFieldTextureGUIContent = new GUIContent("星空贴图(Cube)", "星空的立方体贴图");
        private readonly GUIContent starFieldRotationGUIContent = new GUIContent("旋转星空", "可以使星空贴图在天空旋转到合适的位置");
        private readonly GUIContent starFieldIntensityGUIContent = new GUIContent("星空亮度", "调整星空贴图的颜色");
        private readonly GUIContent starFieldColorGUIContent = new GUIContent("星空颜色", "调整星空贴图的颜色");
        private readonly GUIContent cloudTextureGUIContent = new GUIContent("云层贴图", "云层噪声贴图(RGB三通道噪声)");
        private readonly GUIContent cloudDensityGUIContent = new GUIContent("云层密度", "调整云的整体密度");
        private readonly GUIContent cloudAltituderGUIContent = new GUIContent("云层高度", "调整云层在天空中的高度");
        private readonly GUIContent cloudSpeedGUIContent = new GUIContent("移动速度", "调整云在水平面上的移动方向和速度");
        private readonly GUIContent cloudColor1GUIContent = new GUIContent("云层1颜色", "调整第一层云的颜色");
        private readonly GUIContent cloudColor2GUIContent = new GUIContent("云层2颜色", "调整第二层云的颜色");
        private readonly GUIContent cloudEdgeGUIContent = new GUIContent("云层淡出高度", "调整云层完全淡出的高度");
        private readonly GUIContent cloudEdgeRangeGUIContent = new GUIContent("云层淡出距离", "调整云层逐渐淡出的距离");
        private readonly GUIContent exposureGUIContent = new GUIContent("曝光率", "调节整个天空盒的曝光率");

        private readonly string[] isDayOptions = {"白天", "夜晚"};

        private void OnEnable()
        {
            krProperty = serializedObject.FindProperty("kr");
            rayleighColorProperty = serializedObject.FindProperty("rayleighColor");
            mieColorProperty = serializedObject.FindProperty("mieColor");
            scatteringProperty = serializedObject.FindProperty("scattering");
            luminanceColorProperty = serializedObject.FindProperty("luminanceColor");
            luminanceProperty = serializedObject.FindProperty("luminance");
            isDayProperty = serializedObject.FindProperty("isDay");
            mainLightProperty = serializedObject.FindProperty("mainLight");
            lightSourceTextureProperty = serializedObject.FindProperty("lightSourceTexture");
            lightSourceTextureSizeProperty = serializedObject.FindProperty("lightSourceTextureSize");
            lightSourceTextureIntensityProperty = serializedObject.FindProperty("lightSourceTextureIntensity");
            lightSourceTextureColorProperty = serializedObject.FindProperty("lightSourceTextureColor");
            starFieldTextureProperty = serializedObject.FindProperty("starFieldTexture");
            starFieldRotationProperty = serializedObject.FindProperty("starFieldRotation");
            starFieldIntensityProperty = serializedObject.FindProperty("starFieldIntensity");
            starFieldColorProperty = serializedObject.FindProperty("starFieldColor");
            cloudTextureProperty = serializedObject.FindProperty("cloudTexture");
            cloudDensityProperty = serializedObject.FindProperty("cloudDensity");
            cloudAltitudeProperty = serializedObject.FindProperty("cloudAltitude");
            cloudSpeedProperty = serializedObject.FindProperty("cloudSpeed");
            cloudColor1Property = serializedObject.FindProperty("cloudColor1");
            cloudColor2Property = serializedObject.FindProperty("cloudColor2");
            cloudEdgeProperty = serializedObject.FindProperty("cloudEdge");
            cloudEdgeRangeProperty = serializedObject.FindProperty("cloudEdgeRange");
            exposureProperty = serializedObject.FindProperty("exposure");
            
            luminanceScatterFoldout = new SavedBool("CustomSkyController.LuminanceScatterFolder", false);
            lightSourceFoldout = new SavedBool("CustomSkyController.LightSourceFoldout", false);
            starFieldFoldout = new SavedBool("CustomSkyController.StarFieldFoldout", false);
            cloudFoldout = new SavedBool("CustomSkyController.CloudFoldout", false);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            luminanceScatterFoldout.value =
                EditorGUILayout.BeginFoldoutHeaderGroup(luminanceScatterFoldout.value, "Luminance Scatter");
            if (luminanceScatterFoldout.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(krProperty, krGUIContent);
                EditorGUILayout.PropertyField(scatteringProperty, scatteringGUIContent);
                EditorGUILayout.PropertyField(rayleighColorProperty, rayleighColorGUIContent);
                EditorGUILayout.PropertyField(mieColorProperty, mieColorGUIContent);
                EditorGUILayout.PropertyField(luminanceColorProperty, luminanceColorGUIContent);
                EditorGUILayout.PropertyField(luminanceProperty, luminanceGUIContent);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            lightSourceFoldout.value =
                EditorGUILayout.BeginFoldoutHeaderGroup(lightSourceFoldout.value, "Light Source");
            if (lightSourceFoldout.value)
            {
                EditorGUI.indentLevel++;
                int popupValue = isDayProperty.boolValue ? 0 : 1;
                popupValue = EditorGUILayout.Popup(isDayGUIContent, popupValue, isDayOptions);
                isDayProperty.boolValue = popupValue == 0;
                EditorGUILayout.PropertyField(mainLightProperty, mainLightGUIContent);
                EditorGUILayout.PropertyField(lightSourceTextureProperty, lightSourceTextureGUIContent);
                EditorGUILayout.PropertyField(lightSourceTextureSizeProperty, lightSourceTextureSizeGUIContent);
                EditorGUILayout.PropertyField(lightSourceTextureIntensityProperty, lightSourceTextureIntensityGUIContent);
                EditorGUILayout.PropertyField(lightSourceTextureColorProperty, lightSourceTextureColorGUIContent);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            starFieldFoldout.value =
                EditorGUILayout.BeginFoldoutHeaderGroup(starFieldFoldout.value, "Star Field");
            if (starFieldFoldout.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(starFieldTextureProperty, starFieldTextureGUIContent);
                EditorGUILayout.PropertyField(starFieldRotationProperty, starFieldRotationGUIContent);
                EditorGUILayout.PropertyField(starFieldIntensityProperty, starFieldIntensityGUIContent);
                EditorGUILayout.PropertyField(starFieldColorProperty, starFieldColorGUIContent);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            cloudFoldout.value =
                EditorGUILayout.BeginFoldoutHeaderGroup(cloudFoldout.value, "Cloud");
            if (cloudFoldout.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(cloudTextureProperty, cloudTextureGUIContent);
                EditorGUILayout.PropertyField(cloudDensityProperty, cloudDensityGUIContent);
                EditorGUILayout.PropertyField(cloudAltitudeProperty, cloudAltituderGUIContent);
                EditorGUILayout.PropertyField(cloudSpeedProperty, cloudSpeedGUIContent);
                EditorGUILayout.PropertyField(cloudColor1Property, cloudColor1GUIContent);
                EditorGUILayout.PropertyField(cloudColor2Property, cloudColor2GUIContent);
                EditorGUILayout.PropertyField(cloudEdgeProperty, cloudEdgeGUIContent);
                EditorGUILayout.PropertyField(cloudEdgeRangeProperty, cloudEdgeRangeGUIContent);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.PropertyField(exposureProperty, exposureGUIContent);
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
