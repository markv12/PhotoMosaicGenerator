#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MosaicManager))]
public class MosaicManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MosaicManager mosaicManager = (MosaicManager)target;

        if (GUILayout.Button("Run"))
        {
            mosaicManager.Run();
        }
    }
}
#endif