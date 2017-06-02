﻿using System.Collections.Generic;
using network;
using Framework.Message;

namespace AGrail
{
    public class MaoXian : RoleBase
    {
        public override RoleID RoleID
        {
            get
            {
                return RoleID.MaoXian;
            }
        }

        public override string RoleName
        {
            get
            {
                return "冒险家";
            }
        }

        public override Card.CardProperty RoleProperty
        {
            get
            {
                return Card.CardProperty.幻;
            }
        }

        public MaoXian()
        {
            for (uint i = 1201; i <= 1206; i++)
                Skills.Add(i, Skill.GetSkill(i));
        }

        public override bool CanSelect(uint uiState, Card card, bool isCovered)
        {
            switch (uiState)
            {
                case 1201:
                    return BattleData.Instance.Agent.SelectCards.Count == 0 ||
                        card.Element == Card.GetCard(BattleData.Instance.Agent.SelectCards[0]).Element;                    
            }
            return base.CanSelect(uiState, card, isCovered);
        }

        public override bool CanSelect(uint uiState, SinglePlayerInfo player)
        {
            switch (uiState)
            {
                case 1201:
                    return player.team != BattleData.Instance.MainPlayer.team;
                case 13:                    
                    var r = RoleFactory.Create(player.role_id);
                    return player.team == BattleData.Instance.MainPlayer.team && r.MaxEnergyCount > player.gem + player.crystal;
            }
            return base.CanSelect(uiState, player);
        }

        public override bool CanSelect(uint uiState, Skill skill)
        {
            switch (uiState)
            {
                case 10:
                case 11:
                case 1201:
                case 1203:
                case 1204:
                    if(skill.SkillID == 1204)
                        return BattleData.Instance.MainPlayer.crystal + BattleData.Instance.MainPlayer.gem > 0 
                            && additionalState == 0 && BattleData.Instance.Crystal[(int)BattleData.Instance.MainPlayer.team] > 0;
                    if (skill.SkillID == 1203)
                        return BattleData.Instance.MainPlayer.crystal + BattleData.Instance.MainPlayer.gem > 0 && additionalState == 0 
                            && BattleData.Instance.Gem[(int)BattleData.GetOtherTeam((Team)BattleData.Instance.MainPlayer.team)] > 0;
                    return skill.SkillID == 1201;
            }
            return base.CanSelect(uiState, skill);
        }

        public override uint MaxSelectCard(uint uiState)
        {
            switch (uiState)
            {
                case 1201:
                    return 3;                    
            }
            return base.MaxSelectCard(uiState);
        }

        public override uint MaxSelectPlayer(uint uiState)
        {
            switch (uiState)
            {
                case 13:
                case 1201:
                    return 1;
            }
            return base.MaxSelectPlayer(uiState);
        }

        public override bool CheckExtract(uint uiState)
        {
            if (uiState == 10)
            {
                foreach(var v in BattleData.Instance.PlayerInfos)
                {
                    var r = RoleFactory.Create(v.role_id);
                    if (v.team == BattleData.Instance.MainPlayer.team && r.MaxEnergyCount > v.gem + v.crystal)
                        return true;
                }                
            }                
            return false;
        }

        public override bool CheckOK(uint uiState, List<uint> cardIDs, List<uint> playerIDs, uint? skillID)
        {
            switch (uiState)
            {
                case 13:
                    return playerIDs.Count == 1;
                case 1201:
                    return playerIDs.Count == 1 && cardIDs.Count >= 2;
                case 1203:
                case 1204:
                    return true;
            }
            return base.CheckOK(uiState, cardIDs, playerIDs, skillID);
        }

        public override bool CheckCancel(uint uiState, List<uint> cardIDs, List<uint> playerIDs, uint? skillID)
        {
            switch (uiState)
            {
                case 1201:                    
                case 1203:
                case 1204:
                    return true;
            }
            return base.CheckCancel(uiState, cardIDs, playerIDs, skillID);
        }

        public override void AdditionAction()
        {
            if (BattleData.Instance.Agent.SelectArgs.Count == 1)
            {
                switch (BattleData.Instance.Agent.SelectArgs[0])
                {
                    case 1203:
                    case 1204:
                        additionalState = 1200;
                        break;
                }
            }
            base.AdditionAction();
        }

        public override void UIStateChange(uint state, UIStateMsg msg, params object[] paras)
        {
            List<List<uint>> selectList;
            switch (state)
            {
                case 10:
                    additionalState = 0;
                    break;
                case 12:
                    if (BattleData.Instance.Gem[BattleData.Instance.MainPlayer.team] + BattleData.Instance.Crystal[BattleData.Instance.MainPlayer.team] == 4)
                    {
                        BattleData.Instance.Agent.PlayerRole.Buy(1, 0);
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    }
                    else
                    {
                        BattleData.Instance.Agent.PlayerRole.Buy(2, 0);
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    }
                    return;
                case 13:
                    OKAction = () =>
                    {
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.CloseArgsUI);
                        sendActionMsg(BasicActionType.ACTION_SPECIAL_SKILL, BattleData.Instance.MainPlayer.id,
                           BattleData.Instance.Agent.SelectPlayers, null, 1202, BattleData.Instance.Agent.SelectArgs);
                    };
                    CancelAction = () =>
                    {
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.CloseArgsUI);
                        BattleData.Instance.Agent.FSM.BackState(UIStateMsg.ClickBtn);
                    };
                    if (msg == UIStateMsg.ClickPlayer)
                    {
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.CloseArgsUI);
                        if (BattleData.Instance.Agent.SelectPlayers.Count > 0)
                        {                            
                            var s = BattleData.Instance.GetPlayerInfo(BattleData.Instance.Agent.SelectPlayers[0]);
                            var tGem = BattleData.Instance.Gem[BattleData.Instance.MainPlayer.team];
                            var tCrystal = BattleData.Instance.Crystal[BattleData.Instance.MainPlayer.team];
                            var maxEnergyCnt = RoleFactory.Create(s.role_id).MaxEnergyCount;
                            selectList = new List<List<uint>>();
                            if (maxEnergyCnt - s.gem - s.crystal >= 2)
                            {
                                if (tGem >= 2)
                                    selectList.Add(new List<uint>() { 2, 0 });
                                if (tGem >= 1 && tCrystal >= 1)
                                    selectList.Add(new List<uint>() { 1, 1 });
                                if (tCrystal >= 2)
                                    selectList.Add(new List<uint>() { 0, 2 });
                            }
                            if (maxEnergyCnt - s.gem - s.crystal >= 1)
                            {
                                if (tGem >= 1)
                                    selectList.Add(new List<uint>() { 1, 0 });
                                if (tCrystal >= 1)
                                    selectList.Add(new List<uint>() { 0, 1 });
                            }
                            MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.ShowArgsUI, "Energy", selectList);
                            MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                                string.Format("{0}: 请选择要提炼的能量", Skills[1202].SkillName));
                        }
                    }
                    if (BattleData.Instance.Agent.SelectPlayers.Count <= 0)
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                            string.Format("{0}: 请选择能量要给予的对象", Skills[1202].SkillName));
                    return;
                case 1201:
                    OKAction = () =>
                    {
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.CloseArgsUI);
                        if (BattleData.Instance.Agent.SelectCards.Count == 3)
                            BattleData.Instance.Agent.SelectArgs = new List<uint>() { 39 };
                        sendActionMsg(BasicActionType.ACTION_ATTACK_SKILL, BattleData.Instance.MainPlayer.id,
                            BattleData.Instance.Agent.SelectPlayers, BattleData.Instance.Agent.SelectCards, state,
                            BattleData.Instance.Agent.SelectArgs);
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    CancelAction = () => 
                    {
                        MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.CloseArgsUI);
                        BattleData.Instance.Agent.FSM.BackState(UIStateMsg.Init);
                    };
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.CloseArgsUI);
                    selectList = new List<List<uint>>() { new List<uint>() { 45 }, new List<uint>() { 133 },
                        new List<uint>() { 87 }, new List<uint>() { 66 }, new List<uint>() { 110 }};
                    var mList = new List<string>() { "地","水","火","风","雷" };
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.ShowArgsUI, "欺诈属性", selectList, mList);
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                        string.Format("{0}: 请选择目标玩家, 同系卡牌以及属性。三张默认为暗灭", Skills[state].SkillName));
                    return;
                case 1203:
                case 1204:
                    OKAction = () =>
                    {
                        sendActionMsg(BasicActionType.ACTION_MAGIC_SKILL, BattleData.Instance.MainPlayer.id, null, null, state);                            
                        BattleData.Instance.Agent.FSM.ChangeState<StateIdle>(UIStateMsg.Init, true);
                    };
                    CancelAction = () => { BattleData.Instance.Agent.FSM.BackState(UIStateMsg.Init); };
                    MessageSystem<Framework.Message.MessageType>.Notify(Framework.Message.MessageType.SendHint,
                        string.Format("是否发动{0}", Skills[state].SkillName));
                    return;
            }
            base.UIStateChange(state, msg, paras);
        }
    }
}


