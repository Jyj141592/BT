using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT{
    [CreateNodeMenu]
    public class Sequence : CompositeNode
    {
        public override NodeState OnUpdate()
        {
            NodeState result = children[current].Run();
            if(result == NodeState.Success){
                if(++current >= ChildCount) return NodeState.Success;
                else return NodeState.Running;
            }
            return result;
        }
    }
}
