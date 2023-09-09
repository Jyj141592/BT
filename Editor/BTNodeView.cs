using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace BT.Editor{
public class BTNodeView : Node
{
    public BTNode node;
    private InspectorView inspectorView;
    public Port inputPort = null;
    public Port outputPort = null;
    private Color defaultColor = new Color(80f / 255f, 80f / 255f, 80f / 255f);
    private Color runningColor = Color.yellow;
    private Color successColor = Color.blue;
    private Color failureColor = Color.red;
    public void Init(BTNode node, InspectorView inspectorView){
        this.node = node;
        this.inspectorView = inspectorView;
    }
    public void Draw(){
        mainContainer.Remove(titleContainer);
        topContainer.Remove(inputContainer);
        topContainer.Remove(outputContainer);
        
        MainContainerStyle();
        
        if(node is not RootNode){
            inputPort = CreatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single);
            inputPort.style.marginLeft = 8;
            mainContainer.Insert(0, inputPort);
        }
        else {
            VisualElement element = new VisualElement();
            element.style.height = 20;
            mainContainer.Insert(0, element);
        }
        
        TextElement title = new TextElement(){
            text = BTEditorUtility.NameSpaceToClassName(node.GetType().ToString())
            //text = node.name
        };
        title.style.height = 20;
        title.style.alignSelf = Align.Center;
        //title.style.paddingTop = 5;
        title.style.fontSize = 15;
        mainContainer.Add(title);
        
        CreateNodeContext();

        if(node is not ActionNode){
            Port.Capacity capacity = Port.Capacity.Single;
            if(node is CompositeNode){
                capacity = Port.Capacity.Multi;
            }
            outputPort = CreatePort(Orientation.Vertical, Direction.Output, capacity);
            outputPort.style.marginRight = 8;
            mainContainer.Add(outputPort);
        }
        else{
            VisualElement element = new VisualElement();
            element.style.height = 20;
            mainContainer.Add(element);
        }
        //inputCon.StretchToParentWidth();
    }

    public override void SetPosition(Rect newPos){
        base.SetPosition(newPos);
        Undo.RecordObject(node, "Set Position");
        node.position = newPos.position;
        //EditorUtility.SetDirty(node);
        //AssetDatabase.SaveAssets();
    }

#region CreateNodeUI
        private void MainContainerStyle(){
            mainContainer.style.backgroundColor = defaultColor;
            mainContainer.style.minWidth = 90;
            mainContainer.style.maxWidth = 300;
        }
        private Port CreatePort(Orientation orientation, Direction direction, Port.Capacity capacity){
            Port port = InstantiatePort(orientation, direction, capacity, typeof(bool));
            port.portName = null;
            FlexDirection dir = direction == Direction.Input ? FlexDirection.Column : FlexDirection.ColumnReverse;
            port.style.alignSelf = Align.Center;
            return port;
        }
        private void CreateNodeContext(){
            SerializedObject obj = new SerializedObject(node);
            var fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach(var field in fields){
                var attribute = field.GetCustomAttribute<DisplayOnNodeAttribute>(true);
                if(attribute == null) continue;
                if(!field.IsPublic && field.GetCustomAttribute<SerializeField>(true) == null) continue;
                if(field.FieldType == typeof(int)){
                    IntegerField intField = new IntegerField(){
                        label = field.Name
                    };
                    intField.value = (int) field.GetValue(node);

                    SetFieldStyle(intField, field.Name);
                    
                    if(!Application.isPlaying)
                        intField.BindProperty(obj.FindProperty(field.Name));
                    else {intField.RegisterValueChangedCallback(callback => {
                        field.SetValue(node, callback.newValue);
                    });}
                    mainContainer.Add(intField);
                }
                else if(field.FieldType == typeof(float)){
                    FloatField floatField = new FloatField(){
                        label = field.Name
                    };
                    floatField.value = (float) field.GetValue(node);

                    SetFieldStyle(floatField, field.Name);
                    
                    if(!Application.isPlaying){
                        var prop = obj.FindProperty(field.Name);
                        floatField.BindProperty(prop);
                    }
                    else {
                        floatField.RegisterValueChangedCallback(callback => {
                        field.SetValue(node, callback.newValue);
                        //Debug.Log((float) field.GetValue(node));
                    });
                    }
                    mainContainer.Add(floatField);
                }
                else if(field.FieldType == typeof(bool)){
                    Toggle toggle = new Toggle(){
                        label = field.Name
                    };
                    toggle.value = (bool) field.GetValue(node);

                    SetFieldStyle(toggle, field.Name);

                    if(!Application.isPlaying)
                        toggle.BindProperty(obj.FindProperty(field.Name));
                    else {toggle.RegisterValueChangedCallback(callback => {
                        field.SetValue(node, callback.newValue);
                    });}
                    mainContainer.Add(toggle);
                }
                else if(field.FieldType == typeof(string)){
                    TextField textField = new TextField(){
                        label = field.Name
                    };
                    textField.multiline = true;
                    textField.value = (string) field.GetValue(node);

                    SetFieldStyle(textField, field.Name);

                    if(!Application.isPlaying)
                        textField.BindProperty(obj.FindProperty(field.Name));
                    else {textField.RegisterValueChangedCallback(callback => {
                        field.SetValue(node, callback.newValue);
                    });}
                    mainContainer.Add(textField);
                }
                else if(field.FieldType.IsEnum){
                    EnumField enumField = new EnumField((System.Enum) field.GetValue(node)){
                        label = field.Name
                    };

                    SetFieldStyle(enumField, field.Name);

                    if(!Application.isPlaying)
                        enumField.BindProperty(obj.FindProperty(field.Name));
                    else {enumField.RegisterValueChangedCallback(callback => {
                        field.SetValue(node, callback.newValue);
                    });}
                    mainContainer.Add(enumField);
                }
            }
        }
        private void SetFieldStyle<T>(BaseField<T> field, string name){
            Label label = new Label(name);
            label.style.paddingTop = 2;
            label.style.width = 55;
            field.Remove(field.labelElement);
            field.Insert(0, label);
        }
#endregion
    public override void OnSelected(){
        base.OnSelected();
        inspectorView.OnSelection(this);
    }
    public void Update(){
        if(node.state == BTNode.NodeState.Running){
            if(node.started) mainContainer.style.backgroundColor = runningColor;
            else mainContainer.style.backgroundColor = defaultColor;
        }
        else if(node.state == BTNode.NodeState.Success){
            mainContainer.style.backgroundColor = successColor;
        }
        else mainContainer.style.backgroundColor = failureColor;
    }
    public void SortChildren(){
        if(node is CompositeNode composite){
            composite.children.Sort((left, right) => left.position.x < right.position.x ? -1 : 1);
        }
    }
}
}
