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
        [OperationContract(IsOneWay = true)] void UpdateGui(CallBackInfo info);
        [OperationContract(IsOneWay = true)]
        void SendAllMessages(string[] messages);
        [OperationContract(IsOneWay = true)]
        void AddPlayers(string[] names);
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
        [OperationContract]
        bool Join(string name);
        [OperationContract(IsOneWay = true)]
        void Leave(string name);
        [OperationContract(IsOneWay = true)]
        void PostMessage(string msg);
        [OperationContract(IsOneWay = true)]
        void AddPlayer(string name);
        [OperationContract]
        string[] GetAllMessages();
        [OperationContract]
        string[] GetAllPlayers();
        int NumCards { [OperationContract] get; }

        [OperationContract] bool ToggleCallbacks();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Shoe : IShoe
    {
        //private attributes
        private List<Card> cards = null;
        private int cardIdx;
        private static uint objCount = 0;
        private uint objNum;
        private Dictionary<string, ICallback> callbacks = new Dictionary<string, ICallback>();
        private List<string> messages = new List<string>();
        private List<string> players = new List<string>();

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
            CallBackInfo info = new CallBackInfo(cards.Count - cardIdx, emptyHand);

            foreach (var cb in callbacks)
                if (cb.Value != null)
                    cb.Value.UpdateGui(info);
        }

        private void updatePlayers()
        {
            //List<string> playernames = new List<string>();
            //foreach (KeyValuePair<string, ICallback> entry in callbacks)
            //{
            //    playernames.Add(entry.Key);
            //}
            String[] players = this.players.ToArray<string>();
            foreach (ICallback cb in callbacks.Values)
                cb.AddPlayers(players);
        }

    }
}
