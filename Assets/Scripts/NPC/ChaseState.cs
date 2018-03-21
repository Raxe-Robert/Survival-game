﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : NPCBaseFSM {

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PhotonNetwork.isMasterClient)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PhotonNetwork.isMasterClient)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            agent.SetDestination(opponent.transform.position);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PhotonNetwork.isMasterClient)
        {
            agent.SetDestination(npc.transform.position);
        }
    }
    
}
