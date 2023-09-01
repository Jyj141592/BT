using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT{
public abstract class DecoratorNode : BTNode
{
    public BTNode child;

    public override void Abort(){
        base.Abort();
        child.Abort();
    }
}
}
