using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DiceMachineEditor : Editor
{

    [CustomEditor(typeof(DiceMachine))]
    public class CubeGenerateButton : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DiceMachine diceMachine = (DiceMachine)target;
            if (GUILayout.Button("Capture"))
            {
                diceMachine.CaptureCurrent();
            }
            if (GUILayout.Button("Save"))
            {
                diceMachine.SaveData();
            }
            if (GUILayout.Button("----Reset----"))
            {
                diceMachine.ResetData();
            }
        }
    }
}
