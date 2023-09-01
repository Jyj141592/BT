using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace BT{
public abstract class BTNode : ScriptableObject
{
    public enum NodeState{
        Running, Success, Failure
    }
    private bool started = false;
    private NodeState state = NodeState.Running;
    private BlackBoard blackBoard;
    public string guid {
        get; set;
    } = null;
    public virtual void Init(BlackBoard blackBoard){
        this.blackBoard = blackBoard;
    }
    public NodeState Run(){
        if(!started){
            OnStart();
        }
        NodeState result = OnUpdate();
        if(result != NodeState.Running){
            OnExit();
        }
        return state = result;
    }
    public virtual void OnStart(){
        started = true;
    }
    public abstract NodeState OnUpdate();
    public virtual void OnExit(){
        started = false;
    }
    public virtual void Abort(){
        if(started) OnExit();
        state = NodeState.Running;
    }
}
}
