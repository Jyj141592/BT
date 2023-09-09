using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace BT{
public abstract class BTNode : ScriptableObject
{
    [HideInInspector]
    public enum NodeState{
        Running, Success, Failure
    }
    public bool started{
        get; private set;
    } = false;
    public NodeState state {
        get; private set;
    } = NodeState.Running;
    private BlackBoard blackBoard;
    [HideInInspector]
    public string guid = null;
    [HideInInspector]
    public Vector2 position;
    public virtual void Init(BlackBoard blackBoard){
        this.blackBoard = blackBoard;
        started = false;
        state = NodeState.Running;
    }
    public NodeState Run(){
        if(!started){
            OnStart();
        }
        state = OnUpdate();
        if(state != NodeState.Running){
            OnExit();
        }
        return state;
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
    public virtual BTNode Clone(RootNode root){
        BTNode node = Instantiate(this);
        root.nodes.Add(node);
        return node;
    }
}
}
