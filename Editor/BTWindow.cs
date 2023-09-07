using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Unity.VisualScripting;

namespace BT.Editor{
public class BTWindow : EditorWindow
{
    //private TwoPaneSplitView twoPaneSplitView;
    private BTGraphView graphView = null;
    //private InspectorView inspectorView = null;
    public int instanceID;
    private string key = "PrevTree";

    //[MenuItem("Window/BT")]
    public static BTWindow Open(){
        BTWindow wnd = GetWindow<BTWindow>();
        wnd.titleContent = new GUIContent("BTEditor");
        return wnd;
    }
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line){
        Object target = EditorUtility.InstanceIDToObject(instanceID);
        if(target is RootNode node){
            BTWindow wnd = Open();
            wnd.instanceID = instanceID;
            wnd.LoadGraphView(node);
            EditorPrefs.SetInt(wnd.key, instanceID);
            return true;
        }
        else return false;
    }
    private void CreateGUI(){
        // twoPaneSplitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
        // twoPaneSplitView.StretchToParentSize();
        // rootVisualElement.Add(twoPaneSplitView);
        
        // inspectorView = new InspectorView();
        // twoPaneSplitView.Add(inspectorView);
        // //inspectorView.StretchToParentSize();
        
        // graphView = new BTGraphView();
        // twoPaneSplitView.Add(graphView);
        // //graphView.StretchToParentSize();

        CreateGraphView();

    }
    private void CreateGraphView(){
        BTWindow wnd = GetWindow<BTWindow>();
        graphView = new BTGraphView(wnd);
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    public void LoadGraphView(RootNode node){
        graphView.LoadGraphView(node);
    }

    private void OnSelectionChange() {
        //Debug.Log("deleted!");
        if(EditorUtility.InstanceIDToObject(instanceID) == null) graphView.ClearGraphView();      
    }
}
}
