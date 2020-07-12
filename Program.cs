/*

                        Using Stack() implement war card game. 
                        players: 2- 8
                        deck: 36/52/54 cards, french
                        card ranks: A K Q J 10 9 8 7 6 | 5 4 3 2 | Joker    
                        deck divided by all players
                        players don't look own cards

                        round cycle: everybody place cards on table, winner with greater card gets all and put them to second stack. 
                        If two or more players have same card level then thay draw next card. Simple as it is.

                        game ends when only one player has all cards in hand
                        implement autoplay
                        FINAL VER must output result only

*/

using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;

namespace release
{
    //      === NEED FOR SHUFFLE ===
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
    //      === NEED FOR SHUFFLE ===
    //              === CLASSES ===
    public class Card
    {
        public Card(int cardVal, int suit)
        {
            this.cardVal = cardVal;
            this.suit = suit;
        }
        int cardVal; int suit; bool ifThirSix = false;

        public int CardVal { get => cardVal; set => cardVal = value; }
        public int Suit { get => suit; set => suit = value; }
        public bool IfThirSix { get => ifThirSix; set => ifThirSix = value; }

        //      --------------- operators
        public static bool operator >(Card first_card, Card second_card)
        {
            return (first_card.cardVal > second_card.cardVal);
        }
        public static bool operator <(Card first_card, Card second_card)
        {
            return (first_card.cardVal < second_card.cardVal);
        }
        public static bool operator ==(Card first_card, Card second_card)
        {
            return (first_card.cardVal == second_card.cardVal);
        }
        public static bool operator !=(Card first_card, Card second_card)
        {
            return (first_card.cardVal != second_card.cardVal);
        }
        //     ------------------   Overriding EQUALS and HASH
        public override bool Equals(object o)
        {
            if (o == null)
                return false;

            var second = o as Card;

            return second != null && cardVal == second.cardVal;
        }

        public override int GetHashCode()
        {
            return cardVal;
        }   
    }
    //      ------------------
    public class Player
    {
        public Player(Stack<Card> activeDeck, Stack<Card> reserveDeck, int name)
        {
            this.activeDeck = activeDeck;
            this.reserveDeck = reserveDeck;
            this.name = name;
        }
        Stack<Card> activeDeck;
        int name;
        private Stack<Card> reserveDeck;

        internal Stack<Card> ActiveDeck { get => activeDeck; set => activeDeck = value; }
        public int Name { get => name; set => name = value; }

        //      functions
        public Card ShowMe()
        {
            if (!Convert.ToBoolean(activeDeck.Count))
            {
                int rdeck = this.reserveDeck.Count;
                for (int card = 0; card < rdeck; card++)
                {
                    this.activeDeck.Push(this.reserveDeck.Peek());
                    this.reserveDeck.Pop();
                }
            }
            return this.activeDeck.Peek();
        }

        public Card drawCard()
        {
            if (!Convert.ToBoolean(activeDeck.Count))
            {
                int rdeck = this.reserveDeck.Count;
                for (int card = 0; card < rdeck; card++)
                {
                    this.activeDeck.Push(this.reserveDeck.Peek());
                    this.reserveDeck.Pop();
                }
            }
            Card tmpCard = this.activeDeck.Peek();
            this.activeDeck.Pop();
            return tmpCard;
        }
        public int cardsLeft()
        {
            int count = this.activeDeck.Count + this.reserveDeck.Count;
            return count;
        }
        public void takeCards(Stack<Card> utilCards)
        {
            int count = utilCards.Count;
            for (int utc = 0; utc < count; utc++)
            {
                this.reserveDeck.Push(utilCards.Peek());
                utilCards.Pop();
            }
        }
        //
        public static bool operator >(Player oneP, Player twoP)
        {
            return (oneP.activeDeck.Peek() > twoP.activeDeck.Peek());
        }
        public static bool operator <(Player oneP, Player twoP)
        {
            return (oneP.activeDeck.Peek() < twoP.activeDeck.Peek());
        }

    }
    //              === CLASSES ===
    class Program
    {
        static void Main(string[] args)
        {


            //          === CONST ===
            const string BEGIN = "\nArtyom Steklyanov project War Card Game\n";
            const string START = "\n1 - play\t2 - settings\texit - exit\n";
            const string END = "\nArtyom Steklyanov project War Card Game\n\nPress any key to exit...\n";
            const string ERROR = "\nWrong answer\n";
            const string SETTING_1 = "\n1 - Edit players number\t2 - Edit deck size\t3 - Switch log print\tBACK - to go to main screen\n";
            const string SETTING_2 = "\n2-6 player select\n";
            const string SETTING_3 = "\n1 - 36       2 - 52          3 - 54\n";
            const string CURRENT_SETTING = "\nCurrent setting is:\t{0} players\t{1} number of cards\n";
            const string LOG_ON = "\tLogs are ON\n";
            const string LOG_OFF = "\tLogs are OFF\n";
            const string GAME_1 = "Game has started\n";
            const string PRINT_WON = "\n\t\t\t\tRound {0}\tWinner: {1}\n";
            const string GAME_OVER = "\n\tGame over.\tWinner is player {0}\n\tRound needed: {1}\n\tTry again?\ty/[N]\t";
            const string GAME_OVER2 = "\n\tGame over.\n\tRound needed: {0}\n\tTry again?\ty/[N]\t";
            const string PL_DREW = "\nPlayer {0} drew card {1}  {2}\n";
            const string WAR = "\nWe have WAR\n";
            const string INFINITE_GAME = "\nInfinite game, rounds past: {0}.\tContinue? y/[N] \n";
            const string INF_PL = "Player {0} now has {1} cards in hand\n";
            
            //          === CONST ===
            //          === FUNCIONS ===
            string showCardVal(Card card)
            {
                if (card.CardVal < 11) { return Convert.ToString(card.CardVal); }
                else 
                { 
                    switch (card.CardVal)
                    {
                        case 11: return "Jack";
                        case 12: return "Queen";
                        case 13: return "King";
                        case 14: return "Ace";
                        case 15: return "Joker";
                        default: return "ERROR";
                    }
                }
            }

            string showCardSuit(Card card)
            {
                switch (card.Suit)
                {
                    case 0: return "Hearts";
                    case 1: return "Diamonds";
                    case 2: return "Spades";
                    case 3: return "Clubs";
                    default: return "ERROR";
                }
            }

            Stack<Card> create_deck(int deck_size)
            {
                Stack<Card> startDeck = new Stack<Card> { };
                int maxCard;
                int startCard;
                if (deck_size == 54) { maxCard = 15; }
                else { maxCard = 14; }
                if (deck_size == 36) { startCard = 6; }
                else { startCard = 2; }

                for (int suit = 0; suit < 4; suit++)
                {
                    for (int cardVal = startCard; cardVal <= maxCard; cardVal++)
                    {
                        Card tmpCard = new Card(cardVal, suit);
                        startDeck.Push(tmpCard);
                    }
                }
                if (deck_size == 54) { startDeck.Pop(); startDeck.Pop(); }
                return startDeck;
            }
            //------------------------------------------------
            Stack<Card> deck_shuffle(Stack<Card> startDeck)
            {
                Stack<Card> shuffledDeck = new Stack<Card> { };
                shuffledDeck = startDeck;
                int maxCards = shuffledDeck.Count;

                List<Card> deckCopy = new List<Card> { };
                for (int cardNum = 0; cardNum < maxCards; cardNum++)
                {
                    deckCopy.Add(shuffledDeck.Peek());
                    shuffledDeck.Pop();
                }

                deckCopy.Shuffle();
                for (int cardNum = 0; cardNum < maxCards; cardNum++)
                {
                    shuffledDeck.Push(deckCopy[cardNum]);
                }

                return shuffledDeck;
            }
            //------------------------------------------------


            void play(List<int> setting)
            {
                //      var
                int playersCount = setting[0];
                int deck_size = setting[1];
                bool log_switch = Convert.ToBoolean(setting[2]);
                string ans;
                bool gameHandler = true;
                bool gameOver;
                List<Player> players = new List<Player> { };
                Stack<Card> tableDeck = new Stack<Card> { };
                List<int> warList = new List<int> { };
                int roundNum = 0;
                int roundMax = 10000;
                bool isTrueEnd;
                bool firstLaunch;
                int warCounter;
                int correctDeck =  deck_size;
                //
                //      init game
                while (gameHandler)
                {
                    gameOver = false;
                    isTrueEnd = true;
                    Stack<Card> mainDeck = create_deck(deck_size);      //      deck create and shuffle
                    mainDeck = deck_shuffle(mainDeck);

                    //correct deck_size
                    if ((deck_size % playersCount) > 0)
                    {
                        correctDeck -= (deck_size % playersCount);   // needed for correct division deck_size/playersCount
                    }
                    //
                    //  Players create
                    for (int plNum = 0; plNum < playersCount; plNum++)
                    {
                        Stack<Card> tmpDeck = new Stack<Card> { };
                        Stack<Card> resDeck = new Stack<Card> { };     

                        do
                        {
                            tmpDeck.Push(mainDeck.Peek());
                            mainDeck.Pop();
                        }
                        while (mainDeck.Count > ((correctDeck / playersCount) * (playersCount - 1 - plNum)));

                        players.Add(new Player(tmpDeck, resDeck, plNum));
                    }
                    //
                    Console.Write(GAME_1);

                    // Round start
                    do
                    {
                        playersCount = players.Count;
                        roundNum++;
                        int winner = 0;
                        // check winner
                        for (int plNum = 1; plNum < playersCount; plNum++)
                        {
                            Card oneCard = players[winner].ShowMe();
                            Card twoCard = players[plNum].ShowMe();
                            if (plNum == 1)
                            {
                                if (log_switch)
                                    Console.Write(PL_DREW, players[winner].Name, showCardVal(oneCard), showCardSuit(oneCard));
                            }
                            if (log_switch)
                                Console.Write(PL_DREW, players[plNum].Name, showCardVal(twoCard), showCardSuit(twoCard));
                            // we compare winning card with others, if another card is higher than winner - we switch winner
                            if (oneCard.CardVal == twoCard.CardVal)
                            {
                                warList.Add(plNum);                          // if card values are the same then add player to war
                            }
                            if (players[winner] < players[plNum])
                            {
                                if (Convert.ToBoolean(warList.Count)) { warList.Clear(); }    // clear list if new winner
                                winner = plNum;
                            }
                        }
                        // remove one layer of cards
                        for (int plNum = 0; plNum < playersCount; plNum++)
                        {
                            tableDeck.Push(players[plNum].drawCard());
                        }
                        // if war

                        firstLaunch = true;
                        List<int> tmpD = new List<int> { };

                        warCounter = 0;
                        while (warList.Count != 0)
                        {
                            warCounter++;
#if DEBUG
                            if (warCounter == 2)
                            {
                                Console.Write("Something wrong?");
                            }
#endif
                            if (log_switch)
                                Console.Write(WAR);
                            if (firstLaunch)
                            {
                                warList.Add(winner);          //          ADD first lucky winner
                            }
                            int pl_left = warList.Count;
                            winner = 0;

                            // check if everybody has at least 2 card left, if no - deleting
                            for (int plNum = 0; plNum < warList.Count; plNum++)
                            {
                                if (players[warList[plNum]].cardsLeft() <= 1)
                                {
                                    players.RemoveAt(warList[plNum]);
                                    warList.RemoveAt(plNum);
                                    pl_left--;
                                }
                            }
                            if (players.Count == 1) { break; }
                            //---------- remove top cards
                            if (!firstLaunch)
                            {
                                for (int plNum = 0; plNum < pl_left; plNum++)
                                {
                                    tableDeck.Push(players[warList[plNum]].drawCard());
                                }
                            }
                            //----------check cards again
                            for (int plNum = 1; plNum < pl_left; plNum++)
                            {
                                Card oneCard = players[warList[winner]].ShowMe();
                                Card twoCard = players[warList[plNum]].ShowMe();
                                if (plNum == 1)
                                {
                                    if (log_switch)
                                        Console.Write(PL_DREW, players[warList[winner]].Name, showCardVal(oneCard), showCardSuit(oneCard));
                                }
                                if (log_switch)
                                    Console.Write(PL_DREW, players[warList[plNum]].Name, showCardVal(twoCard), showCardSuit(twoCard));
                                if (oneCard.CardVal == twoCard.CardVal)
                                {
                                    tmpD.Add(warList[plNum]);                          // add player to next war
                                    if (plNum == pl_left - 1)
                                    {
                                        tmpD.Add(warList[winner]);
                                        warList = tmpD;
                                    }
                                }
                                if (players[warList[winner]] < players[warList[plNum]])
                                {
                                    if (Convert.ToBoolean(tmpD.Count)) { tmpD.Clear(); }
                                    winner = plNum;
                                    if (plNum == pl_left - 1)
                                    {
                                        winner = warList[winner];
                                        warList = tmpD;
                                    }
                                }
                                if (warList.Count != 0 && plNum == pl_left - 1 && players[warList[winner]] > players[warList[plNum]])
                                {
                                    winner = warList[winner];
                                    warList.Clear();
                                }
                            }
                            firstLaunch = false;
                            tmpD.Clear();
                        }

                        // ending instructions
                        if (log_switch)
                        Console.Write(PRINT_WON, roundNum, players[winner].Name);

                        do
                        {
                            Stack<Card> tmpDeck = tableDeck;
                            players[winner].takeCards(tmpDeck);
                        } while (false);
                        //      check if smbd out of cards
                        for (int plNum = 0; plNum < players.Count; plNum++)
                        {
                            if (players[plNum].cardsLeft() == 0)
                            {
                                players.RemoveAt(plNum);
                                plNum--;
                            }
                        }
                        // CHECK IF GAME IS TOO LONG
                        if (roundNum > roundMax)
                        {
                            Console.Write(INFINITE_GAME, roundNum);
                            for (int plNum = 0; plNum < players.Count; plNum++)
                            {
                                Console.Write(INF_PL, players[plNum].Name, players[plNum].cardsLeft());
                            }
                            ans = Console.ReadLine().ToLower();
                            if (ans == "y") { roundMax += 10000; continue; }
                            else { isTrueEnd = false; gameOver = true; }
                            continue;
                        }
                        //
                        if (playersCount == 1)
                        { gameOver = true; }
                        // Round end
                    } while (!gameOver);
                    if (gameOver)
                    {
                        if (isTrueEnd) { Console.Write(GAME_OVER, players[0].Name, roundNum); }
                        else { Console.Write(GAME_OVER2, roundNum); }
                        roundNum = 0;
                        ans = Console.ReadLine().ToLower();
                        if (ans == "y") { playersCount = setting[0]; players.Clear(); continue; }
                        else { gameHandler = false; }
                    }
                }
                //
            }
            //-------------------------------------------------------------------------
            List<int> setGame(List<int> set)
            {
                List<int> curSet = new List<int> { set[0], set[1], set[2] };       //  1 - players     2 - deck
                string ans = "";
                bool handle_2 = true;
                int tmpInt = 0;

                while (handle_2)
                {
                    Console.Write(SETTING_1);
                    Console.Write(CURRENT_SETTING, curSet[0], curSet[1]);
                    ans = Console.ReadLine().ToLower();

                    switch (ans)                             //      SwitchEngine(tm)
                    {
                        case "1":           //      players
                            Console.Write(SETTING_2);
                            ans = Console.ReadLine();

                            while (true)
                            {
                                try
                                {
                                    if (ans == "lup") { handle_2 = false; break; }        // check if exit
                                    tmpInt = Convert.ToInt32(ans);                      // check if num
                                    break;
                                }
                                catch (Exception e)
                                {
                                    Console.Write(ERROR);
                                    ans = Console.ReadLine();
                                    continue;
                                }
                            }
                            if (tmpInt < 2 || tmpInt > 8)
                            {
                                Console.Write(ERROR);
                                break;
                            }
                            else { curSet[0] = tmpInt; }
                            break;
                        case "2":           //      deck
                            Console.Write(SETTING_3);
                            ans = Console.ReadLine();               //      1 - 36       2 - 52          3 - 54
                            switch (ans)
                            {
                                case "1":
                                    curSet[1] = 36;
                                    break;
                                case "2":
                                    curSet[1] = 52;
                                    break;
                                case "3":
                                    curSet[1] = 54;
                                    break;
                                default:
                                    Console.Write(ERROR);
                                    break;
                            }
                            break;
                        case "3":       //      log switch
                            if (Convert.ToBoolean(curSet[2])) {
                                curSet[2] = 0;
                                Console.Write(LOG_OFF); 
                            } 
                            else { 
                                curSet[2] = 1;
                                Console.Write(LOG_ON); 
                            }
                            break;
                        case "back":
                            handle_2 = false;
                            break;

                        default:
                            Console.Write(ERROR);
                            break;
                    }
                }

                return curSet;
            }
            //-------------------------------------------------------------------------
            void begin()
            {
                Console.Write(BEGIN);
            }
            //-------------------------------------------------------------------------
            void start()
            {
                //  VAR
                bool handle_1 = true;
                string ans = "";
                List<int> setting = new List<int> { 6, 54, 1 };        //  players, card deck, log
                //
                while (handle_1)
                {
                    Console.Write(START);
                    Console.Write(CURRENT_SETTING, setting[0], setting[1]);
                    if (Convert.ToBoolean(setting[2])) { Console.Write(LOG_ON); } else { Console.Write(LOG_OFF); }
                    ans = Console.ReadLine();
                    switch (ans)                    //      SwitchEngine(tm)
                    {
                        case "1":
                            play(setting);
                            break;
                        case "2":
                            setting = setGame(setting);
                            break;
                        case "exit":
                            handle_1 = false;
                            break;
                        default:
                            Console.WriteLine(ERROR);
                            break;
                    }
                }

            }
            //-------------------------------------------------------------------------
            void end()
            {
                Console.Write(END);
                Console.Read();
            }
            //          === FUNCTIONS ===
            //          === MAIN INSTRUCTIONS ===
            begin();
            start();
            end();
            //          === MAIN INSTRUCTIONS ===
        }
    }
}
