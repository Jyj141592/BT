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
    private InspectorView InspectorView;
    private string key = "PrevTree";
    public BTGraphView(BTWindow wnd, InspectorView inspectorView){
        window = wnd;
        this.InspectorView = inspectorView;
        nodeViews = new Dictionary<string, BTNodeView>();
        CreateGridBackground();
        AddStyleSheet("Assets/BT/Editor/USS/GridBackgroundStyle.uss");
        AddManipulators();
        SetElementsDeletion();
        SetOnGraphViewChanged();

        Undo.undoRedoPerformed += () => {
            LoadGraphView(root , false);
            AssetDatabase.SaveAssets();
        };
        if(Application.isPlaying){
            if(Selection.activeGameObject){
                AIController controller = Selection.activeGameObject.GetComponent<AIController>();
                if(controller) {
                    
                    LoadGraphView(controller.rootNode);
                    return;
                }
            }
        }
        string path = EditorPrefs.GetString(key);
        RootNode target = AssetDatabase.LoadAssetAtPath<RootNode>(path);
        if(target != null) LoadGraphView(target);
        
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
        var rectangle = new RectangleSelector();
        rectangle.target = this;
        this.AddManipulator(rectangle);
    }
    public BTNodeView CreateNodeView(Type nodeType, Vector2 position){
        BTNodeView nodeView = new BTNodeView();
        var node = (BTNode) ScriptableObject.CreateInstance(nodeType);
        node.guid = GUID.Generate().ToString();
        nodeView.Init(node, InspectorView);
        nodeView.Draw();
        nodeView.SetPosition(new Rect(position, Vector2.zero));
        AddElement(nodeView);
        nodeViews.Add(node.guid, nodeView);
        Undo.RecordObject(root, "Create Node");
        root.nodes.Add(node);
        if(!Application.isPlaying){
            AssetDatabase.AddObjectToAsset(node, rootNode.node);
            Undo.RegisterCreatedObjectUndo(node,"Create Node");
            AssetDatabase.SaveAssets();
        }
        return nodeView;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter){
        List<Port> compatiblePorts = new List<Port>();
        ports.ForEach(port =>{
            if(startPort.node == port.node) return;
            if(startPort == port) return;
            if(startPort.direction == port.direction) return;
            compatiblePorts.Add(port);
        });
        return compatiblePorts;
    }
    
    private BTNodeView LoadNodeView(BTNode node){
        BTNodeView nodeView = new BTNodeView();
        nodeView.Init(node, InspectorView);
        nodeView.Draw();
        nodeView.SetPosition(new Rect(node.position,Vector2.zero));
        AddElement(nodeView);
        nodeViews.Add(node.guid, nodeView);
        return nodeView;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
        //base.BuildContextualMenu(evt);
        Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
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
                        Undo.RecordObject(root, "Delete Node");
                        root.nodes.Remove(nodeView.node);
                        DisconnectAll(nodeView);
                        if(!Application.isPlaying){
                            Undo.DestroyObjectImmediate(nodeView.node);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
                
                deleteElements.Add(element);

            }
            DeleteElements(deleteElements);
        };
    }

    private void SetOnGraphViewChanged(){
        graphViewChanged = (changes) => {
            if(changes.edgesToCreate != null){
                foreach(Edge edge in changes.edgesToCreate){
                    BTNodeView parent = edge.output.node as BTNodeView;
                    BTNodeView child = edge.input.node as BTNodeView;
                    ConnectNodes(parent.node, child.node);
                    foreach(KeyValuePair<string, BTNodeView> pair in nodeViews){
                        pair.Value.SortChildren();
                    }
                }
            }
            if(changes.elementsToRemove != null){
                foreach(var element in changes.elementsToRemove){
                    if(element is Edge edge){
                        BTNodeView parent = edge.output.node as BTNodeView;
                        BTNodeView child = edge.input.node as BTNodeView;
                        RemoveConnection(parent.node, child.node);
                    }
                }
            }
            if(changes.movedElements != null){
                foreach(KeyValuePair<string, BTNodeView> pair in nodeViews){
                    pair.Value.SortChildren();
                }
            }
            return changes;
        };
    }

    public void LoadGraphView(RootNode root , bool focus = true){
        
        //if(this.root != root){
            ClearGraphView();
            if(root.guid == null) {
                root.guid = GUID.Generate().ToString();
                root.position = Vector2.zero;
            }
            rootNode = LoadNodeView(root);
            this.root = root;
            foreach(BTNode node in root.nodes){
                LoadNodeView(node);
            }
            if(root.child != null){
                Edge edge = rootNode.outputPort.ConnectTo(nodeViews[root.child.guid].inputPort);
                AddElement(edge);
                //CreateEdge(rootNode, nodeViews[root.child.guid]);
            }
            foreach(BTNode node in root.nodes){
                var children = GetChildrenNode(node);
                foreach(BTNode child in children){
                    Edge edge = nodeViews[node.guid].outputPort.ConnectTo(nodeViews[child.guid].inputPort);
                    AddElement(edge);
                    //CreateEdge(nodeViews[node.guid],nodeViews[child.guid]);
                }
            }
        //}
        if(focus){
            Vector2 pos = new Vector2(-rootNode.GetPosition().x + window.position.width / 2, -rootNode.GetPosition().y);
            UpdateViewTransform(pos, Vector3.one);
        }
    }

    public void ClearGraphView(){
        graphElements.ForEach(graphElement => RemoveElement(graphElement));
        nodeViews.Clear();
        root = null;
        rootNode = null;
    }

    private void CreateEdge(BTNodeView parent, BTNodeView child){
        Edge edge = parent.outputPort.ConnectTo(child.inputPort);
        AddElement(edge);
        ConnectNodes(parent.node, child.node);
    }

    private void ConnectNodes(BTNode parent, BTNode child){
        if(parent is CompositeNode compositeNode){
            if(compositeNode.children.Contains(child)) return;
            Undo.RecordObject(compositeNode, "Connect Node");
            compositeNode.children.Add(child);
        }
        else if(parent is RootNode rootNode){
            Undo.RecordObject(rootNode, "Connect Node");
            rootNode.child = child;
        }
        else if(parent is DecoratorNode decoratorNode){
            Undo.RecordObject(decoratorNode, "Connect Node");
            decoratorNode.child = child;
        }
        //new SerializedObject(parent).ApplyModifiedProperties();
    }

    private void RemoveConnection(BTNode parent, BTNode child){
        if(parent is CompositeNode compositeNode){
            if(!compositeNode.children.Contains(child)) return;
            Undo.RecordObject(compositeNode, "Remove Connection");
            compositeNode.children.Remove(child);
        }
        else if(parent is RootNode rootNode){
            Undo.RecordObject(rootNode, "Remove Connection");
            rootNode.child = null;
        }
        else if(parent is DecoratorNode decoratorNode){
            Undo.RecordObject(decoratorNode, "Remove Connection");
            decoratorNode.child = null;
        }
        //new SerializedObject(parent).ApplyModifiedProperties();
    }
    
    private List<BTNode> GetChildrenNode(BTNode node){
        if(node is CompositeNode compositeNode){
            return compositeNode.children;
        }
        else if(node is DecoratorNode decoratorNode){
            var ret = new List<BTNode>();
            ret.Add(decoratorNode.child);
            return ret;
        }
        else if(node is RootNode rootNode){
            var ret = new List<BTNode>();
            ret.Add(rootNode.child);
            return ret;
        }
        return new List<BTNode>();
    }

    private void DisconnectAll(BTNodeView nodeView){
        if(nodeView.inputPort != null && nodeView.inputPort.connected){
            foreach(Edge edge in nodeView.inputPort.connections){
                BTNodeView parent = edge.output.node as BTNodeView;
                BTNodeView child = edge.input.node as BTNodeView;
                RemoveConnection(parent.node, child.node);
            }
            DeleteElements(nodeView.inputPort.connections);
        }
        if(nodeView.outputPort != null && nodeView.outputPort.connected){
            foreach(Edge edge in nodeView.outputPort.connections){
                BTNodeView parent = edge.output.node as BTNodeView;
                BTNodeView child = edge.input.node as BTNodeView;
                RemoveConnection(parent.node, child.node);
            }
            DeleteElements(nodeView.outputPort.connections);
        }
    }

    public void UpdateStates(){
        foreach(KeyValuePair<string, BTNodeView> pair in nodeViews){
            pair.Value.Update();
        }
    }
}
}