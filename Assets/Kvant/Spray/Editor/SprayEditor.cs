using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Kvant {

[CustomEditor(typeof(Spray))]
public class SprayEditor : Editor
{
    SerializedProperty propShapes;
    SerializedProperty propMaxParticles;

    SerializedProperty propEmitterPosition;
    SerializedProperty propEmitterSize;

    SerializedProperty propMinLife;
    SerializedProperty propMaxLife;

    SerializedProperty propMinScale;
    SerializedProperty propMaxScale;

    SerializedProperty propDirection;
    SerializedProperty propSpread;

    SerializedProperty propMinSpeed;
    SerializedProperty propMaxSpeed;

    SerializedProperty propMinRotation;
    SerializedProperty propMaxRotation;

    SerializedProperty propNoiseVelocity;
    SerializedProperty propNoiseDensity;

    SerializedProperty propColor;
    SerializedProperty propRandomSeed;
    SerializedProperty propDebug;

    void OnEnable()
    {
        propShapes          = serializedObject.FindProperty("_shapes");
        propMaxParticles    = serializedObject.FindProperty("_maxParticles");

        propEmitterPosition = serializedObject.FindProperty("_emitterPosition");
        propEmitterSize     = serializedObject.FindProperty("_emitterSize");

        propMinLife         = serializedObject.FindProperty("_minLife");
        propMaxLife         = serializedObject.FindProperty("_maxLife");

        propMinScale        = serializedObject.FindProperty("_minScale");
        propMaxScale        = serializedObject.FindProperty("_maxScale");

        propDirection       = serializedObject.FindProperty("_direction");
        propSpread          = serializedObject.FindProperty("_spread");

        propMinSpeed        = serializedObject.FindProperty("_minSpeed");
        propMaxSpeed        = serializedObject.FindProperty("_maxSpeed");

        propMinRotation     = serializedObject.FindProperty("_minRotation");
        propMaxRotation     = serializedObject.FindProperty("_maxRotation");

        propNoiseVelocity   = serializedObject.FindProperty("_noiseVelocity");
        propNoiseDensity    = serializedObject.FindProperty("_noiseDensity");

        propColor           = serializedObject.FindProperty("_color");
        propRandomSeed      = serializedObject.FindProperty("_randomSeed");
        propDebug           = serializedObject.FindProperty("_debug");
    }

    void MinMaxSlider(SerializedProperty propMin, SerializedProperty propMax, float minLimit, float maxLimit)
    {
        var min = propMin.floatValue;
        var max = propMax.floatValue;

        EditorGUI.BeginChangeCheck();

        var label = new GUIContent(min.ToString("0.0") + " - " + max.ToString("0.0"));
        EditorGUILayout.MinMaxSlider(label, ref min, ref max, minLimit, maxLimit);

        if (EditorGUI.EndChangeCheck()) {
            propMin.floatValue = min;
            propMax.floatValue = max;
        }
    }

    public override void OnInspectorGUI()
    {
        var emptyLabel = new GUIContent();

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(propMaxParticles);
        EditorGUILayout.PropertyField(propShapes, true);
        if (EditorGUI.EndChangeCheck()) 
            (target as Spray).NotifyConfigChanged();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Emitter Position / Size");
        EditorGUILayout.PropertyField(propEmitterPosition, emptyLabel);
        EditorGUILayout.PropertyField(propEmitterSize, emptyLabel);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Life");
        MinMaxSlider(propMinLife, propMaxLife, 0.1f, 10.0f);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Scale");
        MinMaxSlider(propMinScale, propMaxScale, 0.01f, 10.0f);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Direction / Spread");
        EditorGUILayout.PropertyField(propDirection, emptyLabel);
        EditorGUILayout.Slider(propSpread, 0.0f, 1.0f, emptyLabel);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Speed / Angular Speed");
        MinMaxSlider(propMinSpeed, propMaxSpeed, 0.0f, 100.0f);
        MinMaxSlider(propMinRotation, propMaxRotation, 0.0f, 1000.0f);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Noise Density / Velocity");
        EditorGUILayout.Slider(propNoiseDensity, 0.01f, 1.0f, emptyLabel);
        EditorGUILayout.Slider(propNoiseVelocity, 0.0f, 50.0f, emptyLabel);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(propColor);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(propRandomSeed);
        EditorGUILayout.PropertyField(propDebug);

        serializedObject.ApplyModifiedProperties();
    }
}

} // namespace Kvant
