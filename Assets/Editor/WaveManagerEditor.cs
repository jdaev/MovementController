using UnityEngine;
using UnityEditor;
using UnityEditorInternal; 

[CustomEditor(typeof(WaveManager))]
public class WaveManagerEditor : Editor
{
    SerializedProperty wave;

    ReorderableList list;

    private void OnEnable()
    {
        wave = serializedObject.FindProperty("wave");

        list = new ReorderableList(serializedObject, wave, true, true, true, true);

        list.drawElementCallback = DrawListItems;
        list.drawHeaderCallback = DrawHeader;

    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {        
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index); 

        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), 
            element.FindPropertyRelative("mobs"),
            GUIContent.none
        ); 

        EditorGUI.LabelField(new Rect(rect.x + 120, rect.y, 100, EditorGUIUtility.singleLineHeight), "Level");

        EditorGUI.PropertyField(
            new Rect(rect.x + 160, rect.y, 20, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("level"),
            GUIContent.none
        ); 


        EditorGUI.LabelField(new Rect(rect.x + 200, rect.y, 100, EditorGUIUtility.singleLineHeight), "Quantity");

        EditorGUI.PropertyField(
            new Rect(rect.x + 250, rect.y, 20, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("quantity"),
            GUIContent.none
        );        

    }

    void DrawHeader(Rect rect)
    {
        string name = "Wave";
        EditorGUI.LabelField(rect, name);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}