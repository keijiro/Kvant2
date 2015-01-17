using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Kvant {

[CustomEditor(typeof(Streamline))]
public class StreamlineEditor : Editor
{
    SerializedProperty propRange;
    SerializedProperty propVelocity;
    SerializedProperty propRandom;
    SerializedProperty propTail;

    SerializedProperty propNoiseVelocity;
    SerializedProperty propNoiseDensity;

    SerializedProperty propColor;
    SerializedProperty propDebug;

    void OnEnable()
    {
        propRange    = serializedObject.FindProperty("_range");
        propVelocity = serializedObject.FindProperty("_velocity");
        propRandom   = serializedObject.FindProperty("_random");
        propTail     = serializedObject.FindProperty("_tail");

        propNoiseVelocity = serializedObject.FindProperty("_noiseVelocity");
        propNoiseDensity  = serializedObject.FindProperty("_noiseDensity");

        propColor = serializedObject.FindProperty("_color");
        propDebug = serializedObject.FindProperty("_debug");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(propRange);
        EditorGUILayout.PropertyField(propVelocity);
        EditorGUILayout.Slider(propRandom, 0.0f, 1.0f);
        EditorGUILayout.PropertyField(propTail);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(propNoiseVelocity);
        EditorGUILayout.PropertyField(propNoiseDensity);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(propColor);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(propDebug);

        serializedObject.ApplyModifiedProperties();
    }
}

} // namespace Kvant
