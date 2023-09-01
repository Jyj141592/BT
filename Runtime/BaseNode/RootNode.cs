using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT{
    [CreateAssetMenu(menuName = "BehaviourTree")]
    public class RootNode : BTNode
    {
        public List<BTNode> nodes = new List<BTNode>();
        public BTNode child;

        public override NodeState OnUpdate()
        {
            return child.Run();
        }
        
        public override void Abort(){
            base.Abort();
            child.Abort();
        }
    }
}
