using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT{
[CreateNodeMenu]
public class Selector : CompositeNode
{
    public override NodeState OnUpdate()
    {
        NodeState result = children[current].Run();
        if(result == NodeState.Failure)
        {
            if(++current >= ChildCount) return NodeState.Failure;
            else return NodeState.Running;
        }
        return result;
    }
}
}