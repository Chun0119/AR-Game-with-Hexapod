﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HF_Static;

public class Fight : MonoBehaviour {
    public Enemy enemy = new Enemy();
    public StaticData.ENEMY_TYPE type;
    public AudioClip clip;

    private FightingJoystick joystick;
    private Animator animator;
    private Player player;
    private VirtualPlayer playerObj;
    private int enemyMultiplier, playerMultiplier;

    private void Start() {
        joystick = GameManager.Instance.fightingJoystick;

        animator = GetComponent<Animator>();
        animator.SetBool(type.ToString(), true);
        player = GameManager.Instance.player;
        ResetEnemyMultiplier();
        ResetPlayerMultiplier();
    }

    private void Update() {
        if (playerObj)
        {
            if (animator.GetBool("isDead")) {
                animator.SetBool("isDead", false);
            }

            animator.SetBool("loop", false);
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
            {
                if (animator.GetBool("isFighting"))
                {
                    animator.SetBool("loop", true);
                }
                else if (animator.GetBool("isReady"))
                {
                    animator.SetBool("isFighting", true);
                    StartCoroutine(Fighting());
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && !playerObj && GameManager.Instance.progress != StaticData.GAME_PROGRESS.FIGHTING) {
            playerObj = collision.gameObject.GetComponent<VirtualPlayer>();
            animator.SetBool("isReady", true);
            GameManager.Instance.ChangeProgress(StaticData.GAME_PROGRESS.FIGHTING);
            joystick.SetNoOfInstruction(this);
        }
    }

    private IEnumerator Fighting() {
        while (enemy.GetHp() > 0 && player.GetHp() > 0) {
            AudioManager.Instance.PlaySoundEffect(clip);
            player.BeingAttack(enemy.GetAtk(), enemyMultiplier);
            enemy.BeingAttack(player.GetAtk(), playerMultiplier);
            yield return new WaitForSeconds(1f);
        }

        if (enemy.GetHp() <= 0) {
            joystick.Celebrate();
            animator.SetBool("isDead", true);
            player.DefeatEnemy(enemy.GetMoney(), enemy.GetExp());
            int indexOfCheckpoint = InitializeMap.Instance.map.DefeatEnemy(transform.GetSiblingIndex());
            if (indexOfCheckpoint >= 0) {
                InitializeMap.Instance.checkpoints.transform.GetChild(indexOfCheckpoint).gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(StaticData.FIGHT_OPERATION_TIME);

            CancelInvoke();
            playerObj.EndFight();
            GameManager.Instance.ChangeProgress(StaticData.GAME_PROGRESS.MOVING);
            gameObject.SetActive(false);
        } else {
            CancelInvoke();
            playerObj.EndFight();
            GameManager.Instance.ChangeProgress(StaticData.GAME_PROGRESS.LOSING);
        }
    }

    public void MissDefendOperation() {
        enemyMultiplier = 2;
        //enemy skill effect
        Invoke("ResetEnemyMultiplier", 3f);
    }

    public void PlayerShoot() {
        playerMultiplier = 2;
        StartCoroutine(playerObj.Shoot());
        Invoke("ResetPlayerMultiplier", 3f);
    }

    public void PlayerActivateSkill() {
        playerMultiplier = 2;
        StartCoroutine(playerObj.ActivateSkill());
        Invoke("ResetPlayerMultiplier", 3f);
    }

    public void PlayerDefend() {
        enemyMultiplier = 0;
        StartCoroutine(playerObj.Defend());
        Invoke("ResetEnemyMultiplier", 3f);
    }

    private void ResetEnemyMultiplier() {
        enemyMultiplier = 1;
    }

    private void ResetPlayerMultiplier() {
        playerMultiplier = 1;
    }
}
