using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace BT.Editor{
public class InspectorView : VisualElement
{
    UnityEditor.Editor editor;
    IMGUIContainer container;
    public InspectorView(){
        Add(new Label("Inspector"));
    }
    public void OnSelection(BTNodeView nodeView){
        if(childCount >= 2){
            RemoveAt(1);
        }
        UnityEngine.Object.DestroyImmediate(editor);

        editor = UnityEditor.Editor.CreateEditor(nodeView.node);
        container = new IMGUIContainer(() => editor.OnInspectorGUI());
        Add(container);
    }
}
}
