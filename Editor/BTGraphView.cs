using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT.Editor{
public class BTGraphView : GraphView
{
    public BTNodeView rootNode;
    public RootNode root;
    public Dictionary<string, BTNodeView> nodeViews;
    private BTWindow window;

    public BTGraphView(BTWindow wnd){
        window = wnd;
        nodeViews = new Dictionary<string, BTNodeView>();
        CreateGridBackground();
        AddStyleSheet("Assets/BT/Editor/USS/GridBackgroundStyle.uss");
        AddManipulators();
        //rootNode = CreateNodeView(typeof(RootNode), new Vector2(0, 0));

        SetElementsDeletion();
    }

    private void CreateGridBackground(){
        GridBackground gridBackground = new GridBackground();
        gridBackground.StretchToParentSize();
        Insert(0, gridBackground);
    }
    private void AddStyleSheet(string path){
        StyleSheet styleSheet = (StyleSheet) EditorGUIUtility.Load(path);
        styleSheets.Add(styleSheet);
    }
    private void AddManipulators(){
        this.AddManipulator(new ContentDragger());
        SetupZoom(ContentZoomer.DefaultMinScale,ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        //this.AddManipulator(CreateNodeContextMenu());
    }
    public BTNodeView CreateNodeView(Type nodeType, Vector2 position){
        BTNodeView nodeView = new BTNodeView();
        //var node = (nodeType.) Activator.CreateInstance(nodeType);
        var node = (BTNode) ScriptableObject.CreateInstance(nodeType);
        node.guid = GUID.Generate().ToString();
        nodeView.SetPosition(new Rect(position, Vector2.zero));
        nodeView.Init(node);
        nodeView.Draw();
        AddElement(nodeView);
        nodeViews.Add(node.guid, nodeView);
        root.nodes.Add(node);
        AssetDatabase.AddObjectToAsset(node, rootNode.node);
        AssetDatabase.SaveAssets();
        return nodeView;
    }
    
    private BTNodeView LoadNodeView(BTNode node){
        BTNodeView nodeView = new BTNodeView();
        nodeView.Init(node);

        nodeView.Draw();
        AddElement(nodeView);
        nodeViews.Add(node.guid, nodeView);
        return nodeView;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
        //base.BuildContextualMenu(evt);
        Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
        //evt.menu.AppendAction("CompositeNode/", callback => {Debug.Log("oong!");});
        // CreateMenu for Composite Nodes
        var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
        foreach(Type type in types){
            var attribute = type.GetCustomAttribute<CreateNodeMenuAttribute>();
            if(attribute == null) continue;
            string path = attribute.path;
            if(path == null) path = BTEditorUtility.NameSpaceToClassName(type.Name);
            evt.menu.AppendAction($"CompositeNode/{path}", callback => CreateNodeView(type, nodePosition));
        }
        // CreateMenu for Decorator Nodes
        types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
        foreach(Type type in types){
            var attribute = type.GetCustomAttribute<CreateNodeMenuAttribute>();
            if(attribute == null) continue;
            string path = attribute.path;
            if(path == null) path = BTEditorUtility.NameSpaceToClassName(type.Name);
            evt.menu.AppendAction($"DecoratorNode/{path}", callback => CreateNodeView(type, nodePosition));
        }
        // CreateMenu for Action Nodes
        types = TypeCache.GetTypesDerivedFrom<ActionNode>();
        foreach(Type type in types){
            var attribute = type.GetCustomAttribute<CreateNodeMenuAttribute>();
            if(attribute == null) continue;
            string path = attribute.path;
            if(path == null) path = BTEditorUtility.NameSpaceToClassName(type.Name);
            evt.menu.AppendAction($"ActionNode/{path}", callback => CreateNodeView(type, nodePosition));
        }
    }
    private void SetElementsDeletion(){
        deleteSelection = (operationName, askUser) => {
            List<GraphElement> deleteElements = new List<GraphElement>();
            foreach(GraphElement element in selection){
                if(element is BTNodeView nodeView){
                    if(nodeView.node is RootNode) continue;
                    else {
                        AssetDatabase.RemoveObjectFromAsset(nodeView.node);
                        AssetDatabase.SaveAssets();
                        root.nodes.Remove(nodeView.node);
                    }
                } 
                deleteElements.Add(element);

            }
            DeleteElements(deleteElements);
        };
    }

    public void LoadGraphView(RootNode root){
        if(this.root != root){
            ClearGraphView();
            if(root.guid == null) root.guid = GUID.Generate().ToString();
            rootNode = LoadNodeView(root);
            this.root = root;
            foreach(BTNode node in root.nodes){
                LoadNodeView(node);
            }
        }
        Vector2 pos = new Vector2(-rootNode.GetPosition().x + window.position.width / 2, -rootNode.GetPosition().y);
        UpdateViewTransform(pos, Vector3.one);
        //UpdateViewTransform(rootNode.transform.position, Vector3.one);
    }

    public void ClearGraphView(){
        graphElements.ForEach(graphElement => RemoveElement(graphElement));
        nodeViews.Clear();
        root = null;
        rootNode = null;
    }
    
    
}
}