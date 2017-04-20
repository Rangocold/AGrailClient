﻿using UnityEngine;
using Framework;
using Framework.UI;
using DG.Tweening;
using Framework.Message;
using UnityEngine.UI;
using System.Collections.Generic;

namespace AGrail
{
    public class BattleUIQT : WindowsBase
    {
        [SerializeField]
        private Transform root;
        [SerializeField]
        private Transform battleRoot;
        [SerializeField]
        private Text turn;
        [SerializeField]
        private Text[] morales = new Text[2];        
        [SerializeField]
        private Transform[] energy = new Transform[2];
        [SerializeField]
        private Transform[] grail = new Transform[2];
        [SerializeField]
        private List<Transform> playerAnchor = new List<Transform>();
        [SerializeField]
        private Transform ShowCardArea;
        [SerializeField]
        private GameObject playerStatusPrefab;

        private Dictionary<int, PlayerStatusQT> players = new Dictionary<int, PlayerStatusQT>();

        public override WindowType Type
        {
            get
            {
                return WindowType.BattleQT;
            }
        }

        public override void Awake()
        {
            //依据房间人数先去掉不存在anchor
            if(Lobby.Instance.SelectRoom.max_player != 6)
            {
                playerAnchor.RemoveAt(2);
                playerAnchor.RemoveAt(3);
            }
            //测试基本上都是先生成UI才会收到GameInfo事件
            //但不确定是否有可能反过来
            //最好是能够在Awake中先依据BattleData的数据初始化一遍
            GameManager.AddUpdateAction(onESCClick);
            Dialog.Instance.Reset();
            players.Clear();

            MessageSystem<MessageType>.Regist(MessageType.MoraleChange, this);
            MessageSystem<MessageType>.Regist(MessageType.GemChange, this);
            MessageSystem<MessageType>.Regist(MessageType.CrystalChange, this);
            MessageSystem<MessageType>.Regist(MessageType.GrailChange, this);
            MessageSystem<MessageType>.Regist(MessageType.PlayerIsReady, this);
            MessageSystem<MessageType>.Regist(MessageType.PlayerNickName, this);
            MessageSystem<MessageType>.Regist(MessageType.PlayerRoleChange, this);
            MessageSystem<MessageType>.Regist(MessageType.PlayerTeamChange, this);
            MessageSystem<MessageType>.Regist(MessageType.PlayerHandChange, this);
            MessageSystem<MessageType>.Regist(MessageType.PlayerHealChange, this);
            MessageSystem<MessageType>.Regist(MessageType.PlayerTokenChange, this);
            MessageSystem<MessageType>.Regist(MessageType.PlayerKneltChange, this);
            MessageSystem<MessageType>.Regist(MessageType.PlayerEnergeChange, this);
            MessageSystem<MessageType>.Regist(MessageType.PlayerBasicAndExCardChange, this);
            MessageSystem<MessageType>.Regist(MessageType.LogChange, this);
            MessageSystem<MessageType>.Regist(MessageType.CARDMSG, this);
            MessageSystem<MessageType>.Regist(MessageType.SKILLMSG, this);
            MessageSystem<MessageType>.Regist(MessageType.TURNBEGIN, this);

            root.localPosition = new Vector3(1280, 0, 0);
            root.DOLocalMoveX(0, 1.0f);
            base.Awake();
        }

        public override void OnDestroy()
        {
            players.Clear();
            MessageSystem<MessageType>.UnRegist(MessageType.MoraleChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.GemChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.CrystalChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.GrailChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.PlayerIsReady, this);
            MessageSystem<MessageType>.UnRegist(MessageType.PlayerNickName, this);
            MessageSystem<MessageType>.UnRegist(MessageType.PlayerRoleChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.PlayerTeamChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.PlayerHandChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.PlayerHealChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.PlayerTokenChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.PlayerKneltChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.PlayerEnergeChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.PlayerBasicAndExCardChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.LogChange, this);
            MessageSystem<MessageType>.UnRegist(MessageType.CARDMSG, this);
            MessageSystem<MessageType>.UnRegist(MessageType.SKILLMSG, this);
            MessageSystem<MessageType>.UnRegist(MessageType.TURNBEGIN, this);

            GameManager.RemoveUpdateAciont(onESCClick);
            base.OnDestroy();
        }

        public override void OnEventTrigger(MessageType eventType, params object[] parameters)
        {
            switch (eventType)
            {
                case MessageType.MoraleChange:
                    morales[(int)parameters[0]].text = parameters[1].ToString();
                    break;
                case MessageType.GemChange:
                    gemChange((Team)parameters[0], (int)parameters[1]);
                    break;
                case MessageType.CrystalChange:
                    crystalChange((Team)parameters[0], (int)parameters[1]);
                    break;
                case MessageType.GrailChange:
                    grailChange((Team)parameters[0], (uint)parameters[1]);
                    break;
                case MessageType.PlayerIsReady:
                    checkPlayer((int)parameters[0]);
                    //players[(int)parameters[0]].IsReady = (bool)parameters[1];
                    break;
                case MessageType.PlayerTeamChange:
                    checkPlayer((int)parameters[0]);
                    break;
            }
        }

        private void onESCClick()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Lobby.Instance.LeaveRoom();
                GameManager.UIInstance.PopWindow(WinMsg.Show);
            }
        }

        private void gemChange(Team team, int diffGem)
        {

        }

        private void crystalChange(Team team, int diffCrystal)
        {

        }

        private void grailChange(Team team, uint grail)
        {

        }

        private void checkPlayer(int playerIdx)
        {
            if (!players.ContainsKey(playerIdx))
            {
                var go = Instantiate(playerStatusPrefab);
                go.transform.SetParent(battleRoot);
                players.Add(playerIdx, go.GetComponent<PlayerStatusQT>());
                //依据id确定transform                
                var id = BattleData.Instance.PlayerInfos[playerIdx].id;
                var anchor = playerAnchor[(((int)id % 9) - BattleData.Instance.PlayerID) % playerAnchor.Count];
                go.transform.position = anchor.position;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;                
            }
        }
    }
}