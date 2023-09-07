using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT{
    [CreateAssetMenu(menuName = "BehaviourTree")]
    public class RootNode : BTNode
    {
        [HideInInspector]
        public List<BTNode> nodes = new List<BTNode>();
        [HideInInspector]
        public BTNode child;

        public override NodeState OnUpdate()
        {
            return child.Run();
        }
        
        public override void Abort(){
            base.Abort();
            child.Abort();
        }
        public BTNode Clone(){
            RootNode root = Instantiate(this);
            root.nodes = new List<BTNode>();
            root.child = child.Clone(root);
            return root;
        }
    }
}
