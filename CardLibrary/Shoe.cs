using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace CardLibrary
{
    [ServiceContract]
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateGui(CallBackInfo info);

        [OperationContract(IsOneWay = true)]
        void SendAllMessages(string[] messages);

        [OperationContract(IsOneWay = true)]
        void AddPlayers(string[] names);

        [OperationContract(IsOneWay = true)]
        void UpdateCards(Dictionary<string, List<Card>> cards);

        //[OperationContract(IsOneWay = true)]
        //void AskPlayer(CallBackInfo info);

        //[OperationContract(IsOneWay = true)]
        //void sendCard(CallBackInfo info);
    }

    //[ServiceContract]
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IShoe
    {
        [OperationContract(IsOneWay = true)]
        void Shuffle();
        [OperationContract]
        Card Draw();
        int NumCards { [OperationContract] get; }
        int NumPlayers { [OperationContract] get; [OperationContract] set; }
        [OperationContract]
        bool Join(string name);
        [OperationContract(IsOneWay = true)]
        void Leave(string name);
        [OperationContract(IsOneWay = true)]
        void PostMessage(string msg);
        [OperationContract(IsOneWay = true)]
        void AddPlayer(string name);
        [OperationContract(IsOneWay = true)]
        void RemovePlayer(string name);
        [OperationContract(IsOneWay = true)]
        void AddCardToPlayer(string name, Card c);
        [OperationContract(IsOneWay = true)]
        void RemoveCardFromPlayer(string name, Card c);
        [OperationContract]
        string[] GetAllMessages();
        [OperationContract]
        string[] GetAllPlayers();

        //[OperationContract] bool ToggleCallbacks();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Shoe : IShoe
    {
        //private attributes
        private List<Card> cards = null;
        private int cardIdx = 0;
        private static uint objCount = 0;
        private uint objNum;
        private Dictionary<string, ICallback> callbacks = new Dictionary<string, ICallback>();
        private List<string> messages = new List<string>();
        private List<string> players = new List<string>();
        private int numPlayers = 0;
        private Dictionary<string, List<Card>> cardsOfPlayers = new Dictionary<string, List<Card>>();

        public Shoe()
        {
            objNum = ++objCount;
            cards = new List<Card>();
            repopulate();
        }

        public void Shuffle()
        {
            Random rng = new Random();
            cards = cards.OrderBy(card => rng.Next()).ToList();
            cardIdx = 0;

            // Initiate callbacks
            updateAllClients(true);
        }

        public Card Draw()
        {
            if (cardIdx >= cards.Count())
                throw new IndexOutOfRangeException("The shoe is empty.");

            //logEvent($"Dealing: {cards[cardIdx].ToString()}");
            Card card = cards[cardIdx++];

            // Initiate callbacks
            updateAllClients(false);

            return card;
        }


        public int NumCards
        {
            get
            {
                return cards.Count - cardIdx;
            }
        }

        public int NumPlayers
        {
            get
            {
                return numPlayers;
            }
            set
            {
                if (numPlayers != value)
                    numPlayers = value;
            }
        }

        // Helper methods

        private void repopulate()
        {
            Console.WriteLine($"Shoe object #{objNum}");

            // Clear out the "old" cards
            cards.Clear();

            // Add new "new" cards
            foreach (Card.SuitID s in Enum.GetValues(typeof(Card.SuitID)))
            {
                foreach (Card.RankID r in Enum.GetValues(typeof(Card.RankID)))
                {
                    cards.Add(new Card(s, r));
                }
            }

            // Randomize the collection
            Shuffle();
        }

        public bool Join(string name)
        {
            if (callbacks.ContainsKey(name.ToUpper()))
                return false;
            else
            {
                // Retrieve client's callback proxy
                ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

                // Save alias and callback proxy
                callbacks.Add(name.ToUpper(), cb);

                return true;
            }
        }


        public void Leave(string name)
        {
            if (callbacks.ContainsKey(name.ToUpper()))
            {
                callbacks.Remove(name.ToUpper());
            }
            if (players.Contains(name.ToUpper()))
                players.Remove(name.ToUpper());
        }

        public void AddPlayer(string name)
        {
            players.Insert(players.Count, name);
            updatePlayers();
        }

        public void RemovePlayer(string name)
        {
            players.Remove(name);
            updatePlayers();
        }

        public void AddCardToPlayer(string name, Card c)
        {
            if (!cardsOfPlayers.ContainsKey(name))
            {
                cardsOfPlayers.Add(name, new List<Card>());
                cardsOfPlayers[name].Add(c);
            }
            else
                cardsOfPlayers[name].Add(c);
            updateCards();
        }

        public void RemoveCardFromPlayer(string name, Card c)
        {
            Card cardDeleted = null;
            foreach (Card card in cardsOfPlayers[name])
            {
                if (card.ToString() == c.ToString())
                    cardDeleted = card;
            }
            cardsOfPlayers[name].Remove(cardDeleted);
            List<Card> cards = new List<Card>();
            foreach (var i in cardsOfPlayers[name])
            {
                if (i != c)
                {
                    cards.Add(i);
                }
            }
            cardsOfPlayers.Remove(name);
            cardsOfPlayers.Add(name, new List<Card>());
            foreach (Card card in cards)
            {
                cardsOfPlayers[name].Add(card);
            }

            updateCards();
        }

        public void PostMessage(string message)
        {
            messages.Insert(0, message);
            updateAllUsers();
        }

        public string[] GetAllMessages()
        {
            return messages.ToArray<string>();
        }

        public string[] GetAllPlayers()
        {
            return players.ToArray<string>();
        }

        // Helper methods

        private void updateAllUsers()
        {
            String[] msgs = messages.ToArray<string>();
            foreach (ICallback cb in callbacks.Values)
                cb.SendAllMessages(msgs);
        }

        private void updateAllClients(bool emptyHand)
        {
            CallBackInfo info = new CallBackInfo(cards.Count - cardIdx - 1, emptyHand, numPlayers);

            foreach (var cb in callbacks)
                if (cb.Value != null)
                    cb.Value.UpdateGui(info);
        }

        private void updatePlayers()
        {
            String[] players = this.players.ToArray<string>();
            foreach (ICallback cb in callbacks.Values)
                cb.AddPlayers(players);
        }

        private void updateCards()
        {
            Dictionary<string, List<Card>> cards = this.cardsOfPlayers;
            foreach (ICallback cb in callbacks.Values)
                cb.UpdateCards(cards);
        }

    }
}
