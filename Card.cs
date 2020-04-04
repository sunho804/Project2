/***
 * Author: Sunho Jung, Sue Koh
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization; // WCF Data Contract types


namespace CardLibrary
{
    //   [Serializable]
    [DataContract]
    public class Card
    {
        //Define possible values for suit and rank
        public enum SuitID { Clubs, Diamonds, Hearts, Spades };
        public enum RankID { Ace, King, Queen, Jack, Ten, Nine, Eight, Seven, Six, Five, Four, Three, Two };

        // Public methods & properties
        [DataMember]
        public SuitID Suit { get; private set; }
        [DataMember]
        public RankID Rank { get; private set; }

        public override string ToString()
        {
            return Rank.ToString() + " of " + Suit.ToString();
        }

        // Constructor

        internal Card(SuitID s, RankID r)
        {
            Suit = s;
            Rank = r;
        }
    }
}
