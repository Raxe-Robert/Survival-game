﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBaseFSM : StateMachineBehaviour {

    public GameObject NPC;
    public GameObject Opponent;
    public UnityEngine.AI.NavMeshAgent agent;
    public Vector3 Waypoint;
    public float accuracy = 1.0f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        NPC = animator.gameObject;
        Opponent = PlayerNetwork.PlayerObject;
        agent = NPC.GetComponent<UnityEngine.AI.NavMeshAgent>();
     }    
}
