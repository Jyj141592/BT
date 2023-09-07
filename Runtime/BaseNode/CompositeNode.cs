using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT{
public abstract class CompositeNode : BTNode
{
    [HideInInspector]
    public List<BTNode> children = new List<BTNode>();
    protected int current = 0;
    public int ChildCount{
        get => children.Count;
    }

    public override void OnStart(){
        base.OnStart();
        current = 0;
    }
    public override void Abort(){
        base.Abort();
        for(int i = 0; i < ChildCount; i++){
            children[i].Abort();
        }
    }
    public override BTNode Clone(RootNode root){
            CompositeNode compositeNode = base.Clone(root) as CompositeNode;
            compositeNode.children.ConvertAll(node => node.Clone(root));
            return compositeNode;
        }
}
}
