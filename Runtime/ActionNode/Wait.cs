using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT{
    [CreateNodeMenu]
    public class Wait : ActionNode
    {
        [DisplayOnNode]
        [SerializeField]
        private float duration = 0.0f;
        private float startTime = 0.0f;
        public override NodeState OnUpdate()
        {
            if(Time.time >= startTime + duration) return NodeState.Success;
            return NodeState.Running;
        }
        public override void OnStart(){
            base.OnStart();
            startTime = Time.time;
        }
    }
}
