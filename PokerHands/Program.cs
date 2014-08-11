using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PokerHands
{
    #region Enums

    public enum CardSuit
    {
        NULL = 1,
        CLUBS,
        DIAMONDS,
        HEARTS,
        SPADES
    }

    public enum Player
    {
        PLAYER1,
        PLAYER2
    }

    public enum CardValue
    {
        NULL = 1,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        TEN,
        JACK,
        QUEEN,
        KING,
        ACE
    }

    #endregion

    public class Program
    {
        private static List<string> AllGames = new List<string>();
        private static int Player1Wins = 0;
        private static int Player2Wins = 0;
        private static List<Card> Player1CurHand = new List<Card>();
        private static List<Card> Player2CurHand = new List<Card>();

        static void Main(string[] args)
        {
            OpenFile();
            AnalyzeAllHands();

            Console.WriteLine("Player 1 has {0} wins.", Player1Wins);
            Console.WriteLine("Player 2 has {0} wins.", Player2Wins);
            Console.WriteLine("Total of {0} games played", AllGames.Count);
            Console.ReadLine();
        }

        private static void OpenFile()
        {
            StreamReader sr = new StreamReader("poker.txt");

            while (!sr.EndOfStream)
            {
                AllGames.Add(sr.ReadLine().Trim());
            }

            sr.Close();
        }

        private static void AnalyzeAllHands()
        {
            foreach (string hand in AllGames)
            {
                GenerateHands(hand);
                AnalyzePlayersHands();
            }
        }

        #region Support Methods

        private static bool CheckForAllSameSuit(List<Card> hand)
        {
            return (hand[0].suit == hand[1].suit) && (hand[0].suit == hand[2].suit) &&
                    (hand[0].suit == hand[3].suit) && (hand[0].suit == hand[4].suit);
        }

        private static bool DetermineWinner(Card p1, Card p2)
        {
            if (p1.value != CardValue.NULL && p2.value == CardValue.NULL)
            {
                Player1Wins++;
                return true;
            }
            else if (p1.value == CardValue.NULL && p2.value != CardValue.NULL)
            {
                Player2Wins++;
                return true;
            }
            else if (p1.value != CardValue.NULL && p2.value != CardValue.NULL)
            {
                if (p1.value > p2.value)
                    Player1Wins++;
                else
                    Player2Wins++;
                return true;
            }

            return false;
        }

        private static void AnalyzePlayersHands()
        {
            if (RoyalFlushControl())
                return;

            if (StraightFlushControl())
                return;

            if (FourOfAKindControl())
                return;

            if (FullHouseControl())
                return;

            if (FlushControl())
                return;

            if (StraightControl())
                return;

            if (ThreeOfAKindControl())
                return;

            if (TwoPairsControl())
                return;

            if (OnePairControl())
                return;

            HighestRemainingCard();
        }

        private static void GenerateHands(string hand)
        {
            Player1CurHand = new List<Card>();
            Player2CurHand = new List<Card>();
            Card card;
            char[] splitCard = null;

            // Split the string 
            // First five are player 1
            // Second five are player 2
            string[] players = hand.Split(new string[] { " " }, StringSplitOptions.None);

            for (int i = 0; i < players.Length; i++)
            {
                splitCard = players[i].ToCharArray();
                card.value = DetermineValue(splitCard[0]);
                card.suit = DetermineSuit(splitCard[1]);

                if (i < 5)
                {
                    Player1CurHand.Add(card);
                }
                else
                {
                    Player2CurHand.Add(card);
                }
            }

            // Sort by card value order
            Player1CurHand.Sort((a, b) => a.value.CompareTo(b.value));
            Player2CurHand.Sort((a, b) => a.value.CompareTo(b.value));
        }

        private static CardValue DetermineValue(char value)
        {
            switch (value)
            {
                case '2':
                    return CardValue.TWO;
                case '3':
                    return CardValue.THREE;
                case '4':
                    return CardValue.FOUR;
                case '5':
                    return CardValue.FIVE;
                case '6':
                    return CardValue.SIX;
                case '7':
                    return CardValue.SEVEN;
                case '8':
                    return CardValue.EIGHT;
                case '9':
                    return CardValue.NINE;
                case 'T':
                    return CardValue.TEN;
                case 'J':
                    return CardValue.JACK;
                case 'Q':
                    return CardValue.QUEEN;
                case 'K':
                    return CardValue.KING;
                case 'A':
                    return CardValue.ACE;
                default:
                    return CardValue.NULL;
            }
        }

        private static CardSuit DetermineSuit(char suit)
        {
            switch (suit)
            {
                case 'C':
                    return CardSuit.CLUBS;
                case 'D':
                    return CardSuit.DIAMONDS;
                case 'H':
                    return CardSuit.HEARTS;
                case 'S':
                    return CardSuit.SPADES;
                default:
                    return CardSuit.NULL;
            }
        }

        #endregion

        #region Royal Flush

        private static bool HasRoyalFlush(List<Card> hand)
        {
            if (CheckForAllSameSuit(hand))
            {
                // First a ten and last an ace?
                return (hand[0].value == CardValue.TEN) && (hand[4].value == CardValue.ACE);
            }

            return false;
        }

        /// <summary>
        /// Determines if there are any winners for a royal flush.
        /// </summary>
        /// <returns>True if a winner was found, false otherwise</returns>
        private static bool RoyalFlushControl()
        {
            // Does either have a royal flush?
            // If none, continue to Stright flush.
            // If one, winner,
            // If two, tie.
            if (HasRoyalFlush(Player1CurHand) && !HasRoyalFlush(Player2CurHand))
            {
                Player1Wins++;
                return true;
            }
            else if (!HasRoyalFlush(Player1CurHand) && HasRoyalFlush(Player2CurHand))
            {
                Player2Wins++;
                return true;
            }
            else if (HasRoyalFlush(Player1CurHand) && HasRoyalFlush(Player2CurHand))
            {
                Player1Wins++;
                Player2Wins++;
                return true;
            }
            return false;
        }

        #endregion

        #region Straight Flush

        private static void HasStraightFlush(List<Card> hand, out Card top)
        {
            bool sameVal = false;

            if (CheckForAllSameSuit(hand))
            {
                sameVal = ((((int)hand[0].value) + 1 == (int)hand[1].value) &&
                            (((int)hand[0].value) + 2 == (int)hand[2].value) &&
                            (((int)hand[0].value) + 3 == (int)hand[3].value) &&
                            (((int)hand[0].value) + 4 == (int)hand[4].value));

                if (sameVal)
                {
                    top.suit = hand[4].suit;
                    top.value = hand[4].value;
                }
            }

            top.suit = CardSuit.NULL;
            top.value = CardValue.NULL;
        }

        private static bool StraightFlushControl()
        {
            Card p1;
            Card p2;

            // Does either have a stright flush?
            // If none, continue to four of a kind.
            // If one, winner.
            // If two, check for highest card value.

            HasStraightFlush(Player1CurHand, out p1);
            HasStraightFlush(Player2CurHand, out p2);

            return DetermineWinner(p1, p2);
        }

        #endregion

        #region Four of a Kind

        private static void HasFourOfAKind(List<Card> hand, out Card card)
        {
            Card temp = new Card();
            int diff = 0;

            temp.suit = hand[0].suit;
            temp.value = hand[0].value;

            for (int i = 1; i < hand.Count; i++)
            {
                if (hand[i].value != temp.value)
                {
                    ++diff;
                    if (diff == 2)
                    {
                        card.value = CardValue.NULL;
                        card.suit = CardSuit.NULL;
                        return;
                    }
                }
            }

            card.suit = temp.suit;
            card.value = temp.value;
        }

        private static bool FourOfAKindControl()
        {
            Card p1;
            Card p2;

            HasFourOfAKind(Player1CurHand, out p1);
            HasFourOfAKind(Player2CurHand, out p2);

            // Does either have a four of a kind?
            // If none, continue to full house.
            // If one, winner.
            // If two, check same cards value. higher wins
            return DetermineWinner(p1, p2);
        }

        #endregion

        #region Full House

        private static void HasFullHouse(List<Card> hand, out Card three)
        {
            int c1 = 1;
            int c2 = 0;

            Card t1;
            Card t2;

            t1.value = hand[0].value;
            t2.value = CardValue.NULL;

            t1.suit = hand[0].suit;
            t2.suit = CardSuit.NULL;

            for (int i = 1; i < hand.Count; i++)
            {
                if (hand[i].value == t1.value)
                    c1++;
                else if (hand[i].value == t2.value)
                    c2++;
                else if (t2.value == CardValue.NULL)
                {
                    t2.value = hand[i].value;
                    t2.suit = hand[i].suit;
                    c2++;
                }
            }

            if (c1 == 3 && c2 == 2)
            {
                three.value = t1.value;
                three.suit = t1.suit;
            }
            else if (c1 == 2 && c2 == 3)
            {
                three.value = t2.value;
                three.suit = t2.suit;
            }
            else
            {
                three.value = CardValue.NULL;
                three.suit = CardSuit.NULL;
            }
        }

        private static bool FullHouseControl()
        {
            Card p1;
            Card p2;

            // Does either have full house?
            // If none, continue to flush.
            // If one, winner.
            // If two, 3 higher cards wins

            HasFullHouse(Player1CurHand, out p1);
            HasFullHouse(Player2CurHand, out p2);

            return DetermineWinner(p1, p2);
        }

        #endregion

        #region Flush

        private static bool HasFlush(List<Card> hand)
        {
            if (CheckForAllSameSuit(hand))
                return true;

            return false;
        }

        private static void DetermineFlushWinner()
        {
            for (int i = Player1CurHand.Count-1; i >= 0; i--)
            {
                if (Player1CurHand[i].value > Player2CurHand[i].value)
                {
                    Player1Wins++;
                    return;
                }
                else if (Player1CurHand[i].value < Player2CurHand[i].value)
                {
                    Player2Wins++;
                    return;
                }
            }

            Player1Wins++;
            Player2Wins++;
        }

        private static bool FlushControl()
        {
            // Does either have flush?
            // If none, continue to stright
            // If one, winner.
            // If two, highest value wins.

            bool p1 = HasFlush(Player1CurHand);
            bool p2 = HasFlush(Player2CurHand);

            if (p1 && !p2)
            {
                Player1Wins++;
                return true;
            }
            else if (!p1 && p2)
            {
                Player2Wins++;
                return true;
            }
            else if (p1 && p2)
            {
                DetermineFlushWinner();
                return true;
            }
            return false;
        }

        #endregion

        #region Straight

        private static void HasStraight(List<Card> hand, out Card top)
        {
            bool sameVal = false;

            sameVal = ((((int)hand[0].value) + 1 == (int)hand[1].value) &&
                            (((int)hand[0].value) + 2 == (int)hand[2].value) &&
                            (((int)hand[0].value) + 3 == (int)hand[3].value) &&
                            (((int)hand[0].value) + 4 == (int)hand[4].value));

            if (sameVal)
            {
                top.suit = hand[4].suit;
                top.value = hand[4].value;
                return;
            }

            top.suit = CardSuit.NULL;
            top.value = CardValue.NULL;
        }

        private static bool StraightControl()
        {
            Card p1;
            Card p2;

            // Does either have stright?
            // If none, continue to three of a kind.
            // If one, winner.
            // if two, highest value wins.

            HasStraight(Player1CurHand, out p1);
            HasStraight(Player2CurHand, out p2);

            return DetermineWinner(p1, p2);
        }

        #endregion

        #region Three Of A Kind

        private static bool HasThreeOfAKind(List<Card> hand, out Card trip)
        {
            // The three are going to be consecutive
            if (hand[0].value == hand[1].value && hand[0].value == hand[2].value)
            {
                trip.suit = CardSuit.NULL;
                trip.value = hand[0].value;
                return true;
            }
            else if (hand[1].value == hand[2].value && hand[1].value == hand[3].value)
            {
                trip.suit = CardSuit.NULL;
                trip.value = hand[1].value;
                return true;
            }
            else if (hand[2].value == hand[3].value && hand[2].value == hand[4].value)
            {
                trip.suit = CardSuit.NULL;
                trip.value = hand[2].value;
                return true;
            }

            trip.value = CardValue.NULL;
            trip.suit = CardSuit.NULL;
            return false;
        }

        private static bool ThreeOfAKindControl()
        {
            Card trip1;
            Card trip2;

            // Does either have three of a kind?
            // If none, continue to two pairs.
            // If one, winner.
            // If two, highest value cards wins.
            bool p1 = HasThreeOfAKind(Player1CurHand, out trip1);
            bool p2 = HasThreeOfAKind(Player2CurHand, out trip2);

            if (p1 && !p2)
            {
                Player1Wins++;
                return true;
            }
            else if (!p1 && p2)
            {
                Player2Wins++;
                return true;
            }
            else if (p1 && p2)
            {
                DetermineWinner(trip1, trip2);
                return true;
            }
            return false;
        }

        #endregion

        #region Two Pairs

        private static bool HasTwoPairs(List<Card> hand, out Card top1, out Card top2)
        {
            int i = 0;
            Card pair1 = new Card();
            Card pair2 = new Card();

            pair1.suit = pair2.suit = CardSuit.NULL;
            pair1.value = pair2.value = CardValue.NULL;

            while (i < hand.Count)
            {
                if ((i < hand.Count - 1) && (hand[i].value == hand[i + 1].value))
                {
                    if (pair1.value == CardValue.NULL)
                    {
                        pair1.value = hand[i].value;
                    }
                    else
                    {
                        pair2.value = hand[i].value;
                    }
                    i += 2;
                }
                else
                    i++;
            }

            if (pair1.value != CardValue.NULL && pair2.value != CardValue.NULL)
            {
                top1.value = pair1.value;
                top2.value = pair2.value;
                top1.suit = top2.suit = CardSuit.NULL;
                return true;
            }
            else
            {
                top1.value = top2.value = CardValue.NULL;
                top1.suit = top2.suit = CardSuit.NULL;
                return false;
            }
        }

        private static void DetermineTwoPairWinner(Card p1a, Card p1b, Card p2a, Card p2b)
        {
            Card c1 = new Card();
            Card c2 = new Card();

            List<Card> result1 = new List<Card>() { p1a, p1b };
            List<Card> result2 = new List<Card>() { p2a, p2b };

            result1.Sort((x, y) => x.value.CompareTo(y.value));
            result2.Sort((x, y) => x.value.CompareTo(y.value));

            if ((result1[1].value > result2[1].value) ||
                ((result1[1].value == result2[1].value) && (result1[0].value > result2[0].value)))
            {
                Player1Wins++;
            }
            else if ((result1[1].value < result2[1].value) ||
                ((result1[1].value == result2[1].value) && (result1[0].value < result2[0].value)))
            {
                Player2Wins++;
            }
            else
            {
                for (int i = 0; i < Player1CurHand.Count; i++)
                {
                    if (Player1CurHand[i].value != p1a.value && Player1CurHand[i].value != p1b.value)
                    {
                        c1.suit = Player1CurHand[i].suit;
                        c1.value = Player1CurHand[i].value;
                    }
                    if (Player2CurHand[i].value != p2a.value && Player2CurHand[i].value != p2b.value)
                    {
                        c2.suit = Player2CurHand[i].suit;
                        c2.value = Player2CurHand[i].value;
                    }
                }

                DetermineWinner(c1, c2);
            }
        }

        private static bool TwoPairsControl()
        {
            Card p1a;
            Card p1b;
            Card p2a;
            Card p2b;

            // Does either have two pair?
            // If none, continue to two pairs.
            // If one, winner.
            // If two, higher pairs wins.
            // If pairs match, check remaining card value.  Highest wins.
            bool p1 = HasTwoPairs(Player1CurHand, out p1a, out p1b);
            bool p2 = HasTwoPairs(Player2CurHand, out p2a, out p2b);

            if (p1 && !p2)
            {
                Player1Wins++;
                return true;
            }
            else if (!p1 && p2)
            {
                Player2Wins++;
                return true;
            }
            else if (p1 && p2)
            {
                DetermineTwoPairWinner(p1a, p1b, p2a, p2b);
                return true;
            }
            return false;
        }

        #endregion

        #region One Pair

        private static void HasOnePair(List<Card> hand, out Card pair)
        {
            int i = 0;
            Card pairTemp = new Card();

            // Does either have one pair?
            // If none, continue to high card.
            // If one, winner.
            // If two, higher pair wins.
            // If pair matches, check remaining card values.  Highest wins.

            pairTemp.suit = CardSuit.NULL;
            pairTemp.value = CardValue.NULL;

            while (i < hand.Count)
            {
                if ((i < hand.Count - 1) && (hand[i].value == hand[i + 1].value))
                {
                    pairTemp.value = hand[i].value;
                    break;
                }
                else
                    i++;
            }

            if (pairTemp.value != CardValue.NULL)
            {
                pair.value = pairTemp.value;
                pair.suit = pairTemp.suit;
            }
            else
            {
                pair.value = CardValue.NULL;
                pair.suit = CardSuit.NULL;
            }
        }

        private static void DetermineOnePairWinner(Card p1, Card p2)
        {
            if (p1.value > p2.value)
            {
                Player1Wins++;
            }
            else if (p1.value < p2.value)
            {
                Player2Wins++;
            }
            else
            {
                List<Card> result1 = new List<Card>();
                List<Card> result2 = new List<Card>();

                for (int i = 0; i < Player1CurHand.Count; i++)
                {
                    if (Player1CurHand[i].value != p1.value)
                        result1.Add(Player1CurHand[i]);

                    if (Player2CurHand[i].value != p2.value)
                        result2.Add(Player2CurHand[i]);
                }

                // Sort remaining cards
                result1.Sort((x, y) => x.value.CompareTo(y.value));
                result2.Sort((x, y) => x.value.CompareTo(y.value));

                if ((result1[2].value > result2[2].value) ||
                    ((result1[2].value == result2[2].value) && (result1[1].value > result2[1].value)) ||
                    ((result1[2].value == result2[2].value) && (result1[1].value == result2[1].value) && (result1[0].value > result2[0].value)))
                {
                    Player1Wins++;
                }
                else if ((result1[2].value < result2[2].value) ||
                    ((result1[2].value == result2[2].value) && (result1[1].value < result2[1].value)) ||
                    ((result1[2].value == result2[2].value) && (result1[1].value == result2[1].value) && (result1[0].value < result2[0].value)))
                {
                    Player2Wins++;
                }
                else
                {
                    Player1Wins++;
                    Player2Wins++;
                }
            }
        }

        private static bool OnePairControl()
        {
            Card p1;
            Card p2;

            HasOnePair(Player1CurHand, out p1);
            HasOnePair(Player2CurHand, out p2);

            if (p1.value != CardValue.NULL && p2.value == CardValue.NULL)
            {
                Player1Wins++;
                return true;
            }
            else if (p1.value == CardValue.NULL && p2.value != CardValue.NULL)
            {
                Player2Wins++;
                return true;
            }
            else if (p1.value != CardValue.NULL && p2.value != CardValue.NULL)
            {
                DetermineOnePairWinner(p1, p2);
                return true;
            }
            return false;
        }

        #endregion

        #region Highest Remaining Card

        private static void HighestRemainingCard()
        {
            for (int i = Player1CurHand.Count-1; i >= 0; i--)
            {
                if (Player1CurHand[i].value > Player2CurHand[i].value)
                {
                    Player1Wins++;
                    return;
                }
                else if (Player1CurHand[i].value < Player2CurHand[i].value)
                {
                    Player2Wins++;
                    return;
                }
            }

            Player1Wins++;
            Player2Wins++;
        }

        #endregion

        
    }

    public struct Card
    {
        public CardSuit suit;
        public CardValue value;
    }
}
