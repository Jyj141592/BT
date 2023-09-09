using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT{
public abstract class DecoratorNode : BTNode
{
    [HideInInspector]
    public BTNode child;
    public override void Init(BlackBoard blackBoard){
        base.Init(blackBoard);
        child.Init(blackBoard);

    }
    public override void Abort(){
        base.Abort();
        child.Abort();
    }
    public override BTNode Clone(RootNode root){
            DecoratorNode decoratorNode = base.Clone(root) as DecoratorNode;
            decoratorNode.child = child.Clone(root);
            return decoratorNode;
        }
}
}
