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
    private BTGraphView graphView;
    private TwoPaneSplitView twoPaneSplitView;
    private InspectorView inspectorView = null;
    public int instanceID;
    private string key = "PrevTree";

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
            string path = AssetDatabase.GetAssetPath(node);
            EditorPrefs.SetString(wnd.key, path);
            
            return true;
        }
        else return false;
    }
    private void CreateGUI(){
        twoPaneSplitView = new TwoPaneSplitView(1, 200, TwoPaneSplitViewOrientation.Horizontal);
        twoPaneSplitView.StretchToParentSize();
        rootVisualElement.Add(twoPaneSplitView);
              
        inspectorView = new InspectorView();
        graphView = new BTGraphView(GetWindow<BTWindow>(), inspectorView);
        twoPaneSplitView.Add(graphView);
        
        twoPaneSplitView.Add(inspectorView);
    }
    public void LoadGraphView(RootNode node){

        graphView?.LoadGraphView(node);
    }

    private void OnEnable() {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    private void OnDisable() {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }
    private void OnPlayModeStateChanged(PlayModeStateChange chg){
        switch(chg){
            case PlayModeStateChange.EnteredEditMode:
                string path = EditorPrefs.GetString(key);
                RootNode target = AssetDatabase.LoadAssetAtPath<RootNode>(path);
                if(target != null) LoadGraphView(target);
                break;
        }
    }
    public void OnSelectionChange() {
        if(Application.isPlaying){
            if(Selection.activeGameObject){
                AIController controller = Selection.activeGameObject.GetComponent<AIController>();
                if(controller) {
                    graphView?.LoadGraphView(controller.rootNode);
                }
            }
        }
        if(EditorUtility.InstanceIDToObject(instanceID) == null) graphView.ClearGraphView();
        
    }
    private void OnInspectorUpdate() {
       if(Application.isPlaying) graphView?.UpdateStates();
    }
}
}
