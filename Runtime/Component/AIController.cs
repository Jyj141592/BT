using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BehaviourTree;
using System;

namespace BT{
public class AIController : MonoBehaviour
{
    public RootNode rootNode;
    [field: SerializeField]
    public float frequency{
        get; private set;
    } = 0.1f;
    private BTNode.NodeState state = BTNode.NodeState.Running;
    private bool isDeath = false;
    private bool isPaused = false;
    void Start()
    {
        BlackBoard blackBoard = new BlackBoard();
        blackBoard.Init(gameObject);
        rootNode = rootNode.Clone();
        rootNode.Init(blackBoard);
        Run().Forget();
        GetComponent<HealthSystem>().onDeath+=() =>{
            isDeath = true;
            rootNode.Abort();
        };
    }

    private async UniTaskVoid Run(){
        while(!isDeath){
            if(isPaused) await UniTask.WaitUntil(()=> !isPaused);
            if(state != BTNode.NodeState.Running) {
                rootNode.Abort();
                state = BTNode.NodeState.Running;
            }
            state = rootNode.Run();
            await UniTask.Delay(TimeSpan.FromSeconds(frequency));
        }
    }

    public void Pause(float duration){
        isPaused = true;
        rootNode.Abort();
        Resume(duration).Forget();
    }
    private async UniTaskVoid Resume(float duration){
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
        isPaused = false;
    }
}
}