using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT.Editor{
public class BTNodeView : Node
{
    public BTNode node;
    public void Init(BTNode node){
        this.node = node;
    }
    public void Draw(){
        mainContainer.Remove(titleContainer);
        topContainer.Remove(inputContainer);
        topContainer.Remove(outputContainer);
        
        MainContainerStyle();
        
        if(node is not RootNode){
            Port inputPort = CreatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single);
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
            Port outputPort = CreatePort(Orientation.Vertical, Direction.Output, capacity);
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
#region CreateNodeUI
        private void MainContainerStyle(){
            mainContainer.style.backgroundColor = new Color(80f / 255f, 80f / 255f, 80f / 255f);
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
            var fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach(var field in fields){
                var attribute = field.GetCustomAttribute<DisplayOnNodeAttribute>(true);
                if(attribute == null) continue;
                if(field.FieldType == typeof(int)){
                    IntegerField intField = new IntegerField(){
                        label = field.Name
                    };
                    intField.value = (int) field.GetValue(node);

                    SetFieldStyle(intField, field.Name);

                    intField.RegisterValueChangedCallback(callback => {
                        field.SetValue(node, callback.newValue);
                    });
                    mainContainer.Add(intField);
                }
                else if(field.FieldType == typeof(float)){
                    FloatField floatField = new FloatField(){
                        label = field.Name
                    };
                    floatField.value = (float) field.GetValue(node);

                    SetFieldStyle(floatField, field.Name);

                    floatField.RegisterValueChangedCallback(callback => {
                        field.SetValue(node, callback.newValue);
                    });
                    mainContainer.Add(floatField);
                }
                else if(field.FieldType == typeof(bool)){
                    Toggle toggle = new Toggle(){
                        label = field.Name
                    };
                    toggle.value = (bool) field.GetValue(node);

                    SetFieldStyle(toggle, field.Name);

                    toggle.RegisterValueChangedCallback(callback => {
                        field.SetValue(node, callback.newValue);
                    });
                    mainContainer.Add(toggle);
                }
                else if(field.FieldType == typeof(string)){
                    TextField textField = new TextField(){
                        label = field.Name
                    };
                    textField.multiline = true;
                    textField.value = (string) field.GetValue(node);

                    SetFieldStyle(textField, field.Name);

                    textField.RegisterValueChangedCallback(callback => {
                        field.SetValue(node, callback.newValue);
                    });
                    mainContainer.Add(textField);
                }
                else if(field.FieldType.IsEnum){
                    EnumField enumField = new EnumField((System.Enum) field.GetValue(node)){
                        label = field.Name
                    };

                    SetFieldStyle(enumField, field.Name);

                    enumField.RegisterValueChangedCallback(callback => {
                        field.SetValue(node, callback.newValue);
                    });
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
        }

}
