using System;
using System.Collections.Generic;
using System.Text;

namespace Hearthrock
{

    /// <summary>
    /// this class manage the main logic of Hearthrock.
    /// including but not limited to: interact with Hearthstone, keep program active
    /// </summary>
    class HearthrockRobot
    {
       

        private static bool HeroSpellReady = true;

        public static void RockEnd()
        {
            HeroSpellReady = true;
        }

        public static bool IsEnimyDangerous(Entity enimy) {
            if (enimy.GetATK() > 4) {
                return true;
            }
            if (enimy.GetHealth() > 4)
            {
                if (enimy.GetATK() - enimy.GetHealth() > 4)
                {
                    return true;
                }
            }
            else if (enimy.GetHealth() > 2) // 3,4
            {
                if (enimy.GetATK() - enimy.GetHealth() > 3)
                {
                    return true;
                }
            }
            else // 1,2
            {
                if (enimy.GetATK() - enimy.GetHealth() > 2)
                {
                    return true;
                }
            }

            return false;
        }

        public static Card getRandomBattlecryCard()
        {

            Player player = GameState.Get().GetFriendlySidePlayer();
            Player player_enemy = GameState.Get().GetFirstOpponentPlayer(GameState.Get().GetFriendlySidePlayer());
            List<Card> minions = player.GetBattlefieldZone().GetCards();
            List<Card> minions_enemy = player_enemy.GetBattlefieldZone().GetCards();
            List<Card> randomCardList = new List<Card>();
            foreach (Card card in minions) {
                if (card.GetEntity().CanBeTargetedByBattlecries()) { 
                    randomCardList.Add(card);
                }
            }
            foreach (Card card in minions_enemy) {
                if (card.GetEntity().CanBeTargetedByBattlecries()) { 
                    randomCardList.Add(card);
                }
            }
            string message = "affect by battlecry: ";

            foreach (Card card in randomCardList) {
                message += card.GetEntity().GetName() + " ";
            }
            Console.WriteLine(DateTime.Now + ": " + message);

            Random rnd = new Random();
            int index = rnd.Next(0, randomCardList.Count);
            return randomCardList[0];
        }
        public static Card getFirstBattlefieldCard()
        {
            Player player = GameState.Get().GetFriendlySidePlayer();
            Player player_enemy = GameState.Get().GetFirstOpponentPlayer(GameState.Get().GetFriendlySidePlayer());
            List<Card> minions = player.GetBattlefieldZone().GetCards();
            List<Card> minions_enemy = player_enemy.GetBattlefieldZone().GetCards();
            List<Card> randomCardList = new List<Card>();
            foreach (Card card in minions)
            {
                randomCardList.Add(card);
            }
            foreach (Card card in minions_enemy)
            {
                randomCardList.Add(card);
            }
            if (randomCardList.Count > 0)
                return randomCardList[0];
            return null;
        }

        public static List<Card> getMyValidOptionTargetList()
        {
            List<Card> validCardList = new List<Card>();
            Player player = GameState.Get().GetFriendlySidePlayer();
            List<Card> minions = player.GetBattlefieldZone().GetCards();
            foreach (Card card in minions)
            {
                if (GameState.Get().IsValidOptionTarget(card.GetEntity()))
                    validCardList.Add(card);
            }
            if (GameState.Get().IsValidOptionTarget(player.GetHeroCard().GetEntity()))
                validCardList.Add(player.GetHeroCard());
            return validCardList;
        }

        public static List<Card> getOpponentPlayerValidOptionTargetList()
        {
            List<Card> validCardList = new List<Card>();
            Player player_enemy = GameState.Get().GetFirstOpponentPlayer(GameState.Get().GetFriendlySidePlayer());
            List<Card> minions_enemy = player_enemy.GetBattlefieldZone().GetCards();
            foreach (Card card in minions_enemy)
            {
                if (GameState.Get().IsValidOptionTarget(card.GetEntity()))
                    validCardList.Add(card);
            }
            if (GameState.Get().IsValidOptionTarget(player_enemy.GetHeroCard().GetEntity()))
                validCardList.Add(player_enemy.GetHeroCard());
            return validCardList;
        }



        public static RockAction RockIt()
        {
            RockAction action = new RockAction();
            Player player = GameState.Get().GetFriendlySidePlayer();
            Player player_enemy = GameState.Get().GetFirstOpponentPlayer(GameState.Get().GetFriendlySidePlayer());
            Card hero = player.GetHeroCard();
            Card hero_enemy = player_enemy.GetHeroCard();

            int resource = player.GetNumAvailableResources();
            
            List<Card> curSecretCardList = player.GetSecretZone().GetCards();



            List<Card> crads = player.GetHandZone().GetCards();
            foreach (Card card in crads)
            {
                HearthrockEngine.Log(card.GetEntity().GetName() + " :=: " + card.GetEntity().GetCardId()); 
            }


           
            List<Card> minions = player.GetBattlefieldZone().GetCards();
            List<Card> minions_enemy = player_enemy.GetBattlefieldZone().GetCards();
            Card heropower = player.GetHeroPowerCard();

            // find best match taunt attacker
            List<Card> minion_taunts_enemy = new List<Card>();
            List<Card> minion_dangerous_enemy = new List<Card>();
            List<Card> minion_notaunts = new List<Card>();
            List<Card> minion_taunts = new List<Card>();
            List<Card> minion_attacker = new List<Card>();



            int attack_count_enemy = 0;
            foreach (Card minion_enemy in minions_enemy)
            {
                if (!minion_enemy.GetEntity().CanAttack())
                {
                    continue;
                }
                attack_count_enemy += minion_enemy.GetEntity().GetATK();
                if (minion_enemy.GetEntity().HasWindfury())
                {
                    attack_count_enemy += minion_enemy.GetEntity().GetATK();
                }
            }
            attack_count_enemy += hero_enemy.GetEntity().GetATK();


            int my_army_attack_count = 0;

            foreach (Card card in minions)
            {
                if (isActive(card)) {
                    my_army_attack_count += card.GetEntity().GetATK();
                }
            }
            int player_enemy_defense = 0;
            foreach (Card card_oppo in minions_enemy)
            {
                if (card_oppo.GetEntity().CanBeAttacked() && !card_oppo.GetEntity().IsStealthed())
                {
                    if (card_oppo.GetEntity().HasTaunt())
                    {
                        player_enemy_defense += card_oppo.GetEntity().GetCurrentHealth(); 
                    }
         
                }
            }

            string killMessage = " enemy health: " + hero_enemy.GetEntity().GetCurrentHealth().ToString();
            killMessage += " enemy mionion defense: " + player_enemy_defense.ToString();
            killMessage += " my total attack : " + my_army_attack_count.ToString();

            
            bool canKill = (hero_enemy.GetEntity().GetCurrentHealth() + player_enemy_defense < my_army_attack_count);


            killMessage += " canKill: " + canKill.ToString();
            HearthrockEngine.Log(killMessage);


            foreach (Card card_oppo in minions_enemy)
            {
                if (card_oppo.GetEntity().CanBeAttacked() && !card_oppo.GetEntity().IsStealthed())
                {
                    if (card_oppo.GetEntity().HasTaunt())
                    {
                        minion_taunts_enemy.Add(card_oppo);
                    }
                    else if (IsEnimyDangerous(card_oppo.GetEntity()))
                    {
                        if (!canKill)
                            minion_dangerous_enemy.Add(card_oppo);
                    }
                }
            }

            foreach (Card card in minions)
            {
                if (card.GetEntity().CanAttack() && !card.GetEntity().IsExhausted() && !card.GetEntity().IsFrozen() && !card.GetEntity().IsAsleep() && card.GetEntity().GetATK() > 0)
                {
                    if (card.GetEntity().HasTaunt())
                    {
                        minion_taunts.Add(card);
                    }
                    else
                    {
                        minion_notaunts.Add(card);
                    }
                    minion_attacker.Add(card);
                }
            }


            minions_enemy.Sort(new CardPowerComparer());
            minion_taunts_enemy.Sort(new CardPowerComparer());
            minion_notaunts.Sort(new CardPowerComparer());
            minion_notaunts.Reverse();
            minion_taunts.Sort(new CardPowerComparer());
            minion_taunts.Reverse();
            minion_attacker.Sort(new CardPowerComparer());
            minion_attacker.Reverse();

            // PlayEmergencyCard 
            RockAction action_temp = PlayEmergencyCard(resource, crads, hero, hero_enemy, attack_count_enemy);
            if (action_temp != null)
            {
                action = action_temp;
                return action;
            }

            // if coin necessory
            Card coin_card = null;
            bool need_coin_card = false;
            foreach (Card card in crads)
            {
                if (card.GetEntity().GetCardId() == "GAME_005")
                {
                    coin_card = card;
                    continue;
                }
                if (resource == card.GetEntity().GetCost() - 1)
                {
                    need_coin_card = true;
                }
            }

            // use coin
            if (coin_card != null && need_coin_card)
            {
                action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                action.card1 = coin_card;
                return action;
            }

            // PlayEmergencyCard  again
            action_temp = PlayEmergencyCard(resource, crads, hero, hero_enemy, attack_count_enemy);
            if (action_temp != null)
            {
                action = action_temp;
                return action;
            }


            // use as much spell as possible
            foreach (Card card in crads)
            {
                // but not the coin
                if (card.GetEntity().GetCardId() == "GAME_005")
                {
                    continue;
                }
                if (resource < card.GetEntity().GetCost())
                {
                    continue;
                }



                //if (card.GetEntity().IsSpell() || card.GetEntity().IsWeapon())
                //{
                //    action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                //    action.card1 = card;
                //    return action;
                //}

                // play 动物伙伴

                if (card.GetEntity().GetCardId() == "NEW1_031")
                {
                    action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                    action.card1 = card;
                    return action;
                }
                if (card.GetEntity().GetCardId() == "OG_211") { 
                    action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                    action.card1 = card;
                    return action;
                }
                if (card.GetEntity().HasBattlecry())
                {
                    action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                    action.card1 = card;
                    return action;
                }
                //if (card.GetEntity().IsSpell()) {
                //    action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                //    action.card1 = card;
                //    return action;
                //}

                if (player.GetWeaponCard() == null)
                {
                    // 鹰角弓
                    if (card.GetEntity().GetCardId() == "EX1_536")
                    {
                        action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                        action.card1 = card;
                        return action;
                    }
                }
                // 关门放狗
                if (card.GetEntity().GetCardId() == "EX1_538")
                {
                    if (minions_enemy.Count > 0) {
                        action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                        action.card1 = card;
                        return action; 
                    }  
                }
                // 快速射击
                if (card.GetEntity().GetCardId() == "BRM_013")
                {
                    action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                    action.card1 = card;
                    return action;  
                }
                // 杀戮命令
                if (card.GetEntity().GetCardId() == "EX1_539")
                {
                    action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                    action.card1 = card;
                    return action;
                }

            }

            // find a minion which can use all resource
            foreach (Card card in crads)
            {
                if (resource < card.GetEntity().GetCost())
                {
                    continue;
                }
                if (card.GetEntity().IsMinion() && GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone().GetCards().Count < 6 && (card.GetEntity().GetCost() == resource || ((card.GetEntity().GetCost() == resource - 2) && HeroSpellReady)))
                {
                    action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                    action.card1 = card;
                    return action;
                } 
            }
            // find a minion which can use all resource
            foreach (Card card in crads)
            {
                if (resource < card.GetEntity().GetCost())
                {
                    continue;
                }

                if (card.GetEntity().IsMinion() && GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone().GetCards().Count < 6)
                {
                    action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                    action.card1 = card;
                    return action;
                }
                if (card.GetEntity().IsMinion() && GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone().GetCards().Count == 6)
                {
                    if (card.GetEntity().HasCharge() || card.GetEntity().HasTaunt())
                    {
                        action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                        action.card1 = card;
                        return action;
                    }
                }
            }
           // play 奥秘
            foreach (Card card in crads)
            {
                if (resource < card.GetEntity().GetCost())
                {
                    continue;
                }
     

                if (card.GetEntity().IsSecret())
                {
                    bool tmpCheck = false;
                    foreach (Card secretCard in player.GetSecretZone().GetCards())
                    {
                        if (secretCard.GetEntity().GetCardId().Equals(card.GetEntity().GetCardId()))
                        {
                            tmpCheck = true;
                            break;
                        }
                    }
                    if (!tmpCheck)
                    {
                        action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                        action.card1 = card;
                        return action;
                    }
                }
            }

            // begin attack
            { // deal with his taunts
                // PlayKill  notaunts > taunts
                action_temp = PlayKill(minion_taunts_enemy, minion_notaunts);
                if (action_temp != null)
                {
                    action = action_temp;
                    return action;
                }

                // PlayKill  taunts > taunts
                action_temp = PlayKill(minion_taunts_enemy, minion_taunts);
                if (action_temp != null)
                {
                    action = action_temp;
                    return action;
                }

                //deal damage with no taunt
                foreach (Card card_oppo in minion_taunts_enemy)
                {
                    foreach (Card card in minion_notaunts)
                    {
                        action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                        action.card1 = card;
                        action.card2 = card_oppo;
                        return action;
                    }
                }

                //deal damage with taunt
                foreach (Card card_oppo in minion_taunts_enemy)
                {
                    foreach (Card card in minion_taunts)
                    {
                        action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                        action.card1 = card;
                        action.card2 = card_oppo;
                        return action;
                    }
                }

            }

            { // deal with his dangerous
                // TODO: should check maybe i can kill him  
                // PlayKill  notaunts > dangerous
                action_temp = PlayKill(minion_dangerous_enemy, minion_notaunts);
                if (action_temp != null)
                {
                    action = action_temp;
                    return action;
                }


                //deal damage with no taunt
                foreach (Card card_oppo in minion_dangerous_enemy)
                {
                    foreach (Card card in minion_notaunts)
                    {
                        action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                        action.card1 = card;
                        action.card2 = card_oppo;
                        return action;
                    }
                }
            }

            { // deal with enemy depends on my health 
                // TODO: should check maybe i can kill him  
                // no taunt, but in danger
                if (minion_taunts_enemy.Count == 0 && (hero.GetEntity().GetHealth() - attack_count_enemy) < 10)
                {
                    foreach (Card card_oppo in minions_enemy)
                    {
                        // find dangerous card
                        if (card_oppo.GetEntity().GetATK() - card_oppo.GetEntity().GetHealth() > 3)
                        {
                            foreach (Card card in minion_attacker)
                            {
                                // if can kill, kill
                                if (card.GetEntity().GetATK() >= card_oppo.GetEntity().GetHealth())
                                {
                                    action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                                    action.card1 = card;
                                    action.card2 = card_oppo;
                                    return action;
                                }
                            }
                        }
                    }
                }

                // no taunt, but in great danger
                if (minion_taunts_enemy.Count == 0 && (hero.GetEntity().GetHealth() - attack_count_enemy) < 0)
                {
                    foreach (Card card_oppo in minions_enemy)
                    {
                        // find dangerous card
                        if (card_oppo.GetEntity().GetATK() - card_oppo.GetEntity().GetHealth() > 1)
                        {
                            foreach (Card card in minion_attacker)
                            {
                                if (card.GetEntity().GetATK() > card_oppo.GetEntity().GetATK())
                                {
                                    continue;
                                }
                                // if can kill, kill
                                if (card.GetEntity().GetATK() >= card_oppo.GetEntity().GetHealth())
                                {
                                    action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                                    action.card1 = card;
                                    action.card2 = card_oppo;
                                    return action;
                                }
                            }
                        }
                    }
                }
            } // END deal with enemy depends on my health 


            // attack his face!
            foreach (Card card in minions)
            {
                if (card.GetEntity().CanAttack() && !card.GetEntity().IsExhausted() && !card.GetEntity().IsFrozen() && !card.GetEntity().IsAsleep() && card.GetEntity().GetATK() > 0)
                {
                    foreach (Card card_oppo in minions_enemy)
                    {
                        // for bug, should noy run
                        if (card_oppo.GetEntity().HasTaunt())
                        {
                            action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                            action.card1 = card;
                            action.card2 = card_oppo;
                            return action;
                        }
                    }
                    action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                    action.card1 = card;
                    action.card2 = player_enemy.GetHeroCard();
                    return action;
                }


            }

            // deal with hero weapon and atk
            HearthrockEngine.Log("HasWeapon " + player.HasWeapon());
            if (player.HasWeapon())
            {
                HearthrockEngine.Log("HasWeapon CanAttack " + player.GetWeaponCard().GetEntity().CanAttack());
            }
            if (minion_taunts_enemy.Count == 0 && player.HasWeapon() && player.GetWeaponCard().GetEntity().CanAttack()
                && !player.GetWeaponCard().GetEntity().IsFrozen() && !player.GetWeaponCard().GetEntity().IsExhausted()
                && !player.GetWeaponCard().GetEntity().IsAsleep())
            {
                action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                action.card1 = player.GetWeaponCard();
                action.card2 = player_enemy.GetHeroCard();
                return action;
            }


            Entity me = player.GetHeroCard().GetEntity();
            if (minion_taunts_enemy.Count == 0 && me.CanAttack() && me.GetATK() > 0 && !me.IsFrozen() && !me.IsExhausted() && !me.IsAsleep())
            {
                action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                action.card1 = player.GetHeroCard();
                action.card2 = player_enemy.GetHeroCard();
                return action;
            }


            if (resource >= 2 && HeroSpellReady)
            {
                HeroSpellReady = false;
                TAG_CLASS hero_class = player.GetHeroCard().GetEntity().GetClass();
                switch (hero_class)
                {
                    case TAG_CLASS.WARLOCK:
                        if (crads.Count > 8)
                        {
                            return action;
                        }
                        if (hero.GetEntity().GetCurrentHealth() < 5)
                        {
                            return action;
                        }
                        else if (hero.GetEntity().GetCurrentHealth() < 12)
                        {


                            if (attack_count_enemy + 2 > hero.GetEntity().GetCurrentHealth())
                            {
                                return action;
                            }
                            else
                            {
                                action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                                action.card1 = heropower;
                                return action;
                            }
                        }
                        else
                        {
                            action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                            action.card1 = heropower;
                            return action;
                        }
                    case TAG_CLASS.HUNTER:
                    case TAG_CLASS.DRUID:
                    case TAG_CLASS.PALADIN:
                    case TAG_CLASS.ROGUE:
                    case TAG_CLASS.SHAMAN:
                    case TAG_CLASS.WARRIOR:
                        action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                        action.card1 = heropower;
                        return action;
                    case TAG_CLASS.PRIEST:
                        action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                        action.card1 = heropower;
                        action.card2 = player.GetHeroCard();
                        return action;
                    case TAG_CLASS.MAGE:
                        action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                        action.card1 = heropower;
                        action.card2 = player_enemy.GetHeroCard();
                        return action;
                    default:
                        break;
                }
            }
            return action;

        }


        private static RockAction PlayKill(List<Card> minion_target, List<Card> minion_attacker)
        {
            RockAction action = new RockAction();
            Card target_best = null;
            Card attacker_best = null;

            //find taunt kill attacker
            foreach (Card card_oppo in minion_target)
            {
                foreach (Card card in minion_attacker)
                {
                    if (card_oppo.GetEntity().GetCurrentHealth() <= card.GetEntity().GetATK())
                    {
                        if (target_best == null)
                        {
                            target_best = card_oppo;
                            attacker_best = card;
                        }
                        else
                        {
                            if (attacker_best.GetEntity().GetATK() > card.GetEntity().GetATK())
                            {
                                attacker_best = card;
                            }
                        }
                    }
                }

                if (target_best != null)
                {
                    action.type = HEARTHROCK_ACTIONTYPE.ATTACK;
                    action.card1 = attacker_best;
                    action.card2 = target_best;
                    return action;
                }
            }
            return null;
        }

        private static bool isActive(Card card)
        {
            Entity e = card.GetEntity();
            return (!e.IsAsleep()) && (e.CanAttack()) && (e.GetATK() > 0) && (!(e.IsFrozen())) && (!e.IsExhausted());
        }

        private static RockAction PlayEmergencyCard(int resource, List<Card> crads, Card hero, Card hero_enemy, int attack_count_enemy)
        {
            RockAction action = new RockAction();

            // best fit taunt
            foreach (Card card in crads)
            {
                if (resource < card.GetEntity().GetCost())
                {
                    continue;
                }
                if (card.GetEntity().IsMinion() && card.GetEntity().HasTaunt() && GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone().GetCards().Count < 7 && (card.GetEntity().GetCost() == resource || ((card.GetEntity().GetCost() == resource - 2) && HeroSpellReady)))
                {
                    action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                    action.card1 = card;
                    return action;
                }
            }

            // if hero has less health, need more emergency taunt
            if ((hero.GetEntity().GetHealth() - attack_count_enemy) < 10)
            {
                // not perfect fit taunt
                foreach (Card card in crads)
                {

                    if (resource < card.GetEntity().GetCost())
                    {
                        continue;
                    }
                    // waste a cost
                    if (card.GetEntity().IsMinion() && card.GetEntity().HasTaunt() && GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone().GetCards().Count < 7 && (card.GetEntity().GetCost() == resource - 1 || ((card.GetEntity().GetCost() == resource - 3) && HeroSpellReady)))
                    {
                        action.type = HEARTHROCK_ACTIONTYPE.PLAY;
                        action.card1 = card;
                        return action;
                    }
                }
            }

            //// if hero has more health, play as much charge as possible
            //if ((hero.GetEntity().GetHealth() - hero_enemy.GetEntity().GetHealth()) > 10)
            //{
            //    foreach (Card card in crads)
            //    {
            //        if (resource < card.GetEntity().GetCost())
            //        {
            //            continue;
            //        }

            //        // waste a cost
            //        if (card.GetEntity().IsMinion() && GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone().GetCards().Count < 7)
            //        {
            //            if (card.GetEntity().HasCharge())
            //            {
            //                action.type = HEARTHROCK_ACTIONTYPE.PLAY;
            //                action.card1 = card;
            //                return action;
            //            }
            //        }
            //    }
            //}
            return null;
        }

    }

}
