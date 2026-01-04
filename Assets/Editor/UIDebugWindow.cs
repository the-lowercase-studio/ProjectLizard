using UnityEditor;
using UnityEngine;

public class UIDebugWindow : EditorWindow
{
    private bool _showCardUseArea;
    private GameObject _cardUseArea;

    [MenuItem("Tools/UI Debug")]
    private static void Open()
    {
        GetWindow<UIDebugWindow>("UI Debug");
    }

    private void OnGUI()
    {
        GUILayout.Label("UI Visibility", EditorStyles.boldLabel);

        _cardUseArea = EditorGUILayout.ObjectField("Card Use Area Object", _cardUseArea, typeof(GameObject), true) as GameObject;
        _showCardUseArea = EditorGUILayout.Toggle("Show Card Use Area", _showCardUseArea);

        if (GUILayout.Button("Apply"))
        {
            _cardUseArea.SetActive(_showCardUseArea);
        }
    }
}