﻿using System;
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
    }

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IShoe
    {
        [OperationContract(IsOneWay = true)]
        void Shuffle();
        [OperationContract]
        Card Draw();
        int NumCards { [OperationContract] get; }

        [OperationContract] bool ToggleCallbacks();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Shoe : IDisposable, IShoe
    {
        //private attributes
        private List<Card> cards = null;
        private int cardIdx;
        private StreamWriter log = null;

        private HashSet<ICallback> callbacks = new HashSet<ICallback>();

        public Shoe()
        {
            log = new StreamWriter("shoe.log");
            logEvent("Creating the Shoe");

            cards = new List<Card>();
            repopulate();
        }

        public void Shuffle()
        {
            logEvent("Shuffling the Shoe");

            Random rng = new Random();
            cards = cards.OrderBy(card => rng.Next()).ToList();
            cardIdx = 0;

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
                // Returns the number of cards in the shoe that haven't aready 
                // been dealt via Draw()
                return cards.Count - cardIdx;
            }
        }

        // Helper methods

        private void repopulate()
        {
            logEvent($"Repopulating the Shoe with 1 deck");

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

        private void logEvent(string msg)
        {
            log.WriteLine(msg);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    log.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Shoe()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public bool ToggleCallbacks()
        {
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            if (callbacks.Contains(cb))
            {
                callbacks.Remove(cb);
                return false;
            }
            else
            {
                callbacks.Add(cb);
                return true;
            }
        }
        private void updateAllClients(bool emptyHand)
        {
            CallBackInfo info = new CallBackInfo(NumCards, emptyHand);

            foreach (ICallback cb in callbacks)
                cb.UpdateGui(info);
        }

        #endregion

    }
}
