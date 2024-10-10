using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BackgroundMusicManager))]
public class BackgroundMusicManagerEditor : Editor
{
    private LandType newLandType;
    private AudioClip newAudioClip;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BackgroundMusicManager manager = (BackgroundMusicManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add New Music Entry", EditorStyles.boldLabel);

        newLandType = (LandType)EditorGUILayout.EnumPopup("Land Type", newLandType);
        newAudioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", newAudioClip, typeof(AudioClip), false);

        if (GUILayout.Button("Add Entry"))
        {
            if (newAudioClip != null)
            {
                //manager.AddMusicEntry(newLandType, newAudioClip);
                newLandType = default(LandType);
                newAudioClip = null;
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign an audio clip before adding an entry.", "OK");
            }
        }
    }
}