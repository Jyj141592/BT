using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT{
    [CreateNodeMenu]
    public class WriteLog : ActionNode
    {
        [DisplayOnNode]
        private string message;
        public override NodeState OnUpdate()
        {
            Debug.Log(message);
            return NodeState.Success;
        }
    }
}
