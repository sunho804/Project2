using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceModel;

using CardLibrary;

namespace GoFishClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        private string name = "";
        private IShoe shoe = null;
        private int cardCount = 0;
        private bool callbacksEnabled = false;
        Tuple<string, Tuple<string, string>> askinfo = null;

        //in board, show card count 

        public MainWindow()
        {
            InitializeComponent();
            playBtn.IsEnabled = false;
        }

        private void nameSetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (nameTxtBox.Text != "")
            {
                try
                {
                    // shoe.PostMessage(nameTxtBox.Text);
                    //nameTxtBox.Clear();
                    //listMessages.ItemsSource = msgBrd.GetAllMessages();
                    nameSetBtn.IsEnabled = nameTxtBox.IsEnabled = boardListBox.IsEnabled = true;

                    connectToMessageBoard();
                    shoe.AddPlayer(nameTxtBox.Text);
                    shoe.PostMessage(nameTxtBox.Text + " has joined.");
                    loadCardRanksComboBox();
                    playBtn.IsEnabled = true;
                    cardCount = shoe.NumCards - 5;
                    playersAskComboBox.Items.Remove(nameTxtBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            shoe.PostMessage(nameTxtBox.Text + " has left.");
            playersListBox.Items.Remove(nameTxtBox.Text);
            if (shoe != null)
                shoe.Leave(nameTxtBox.Text);

            this.Close();
        }

        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //draw 5 cards to each player
                for (var i = 0; i < 5; i++)
                {
                    Card card = shoe.Draw();

                    cardListBox.Items.Insert(0, card);
                    Console.WriteLine(card);
                    //shoe.NumCards.ToString();
                }
                
                findBooks();
                playBtn.IsEnabled = false;
                shoe.PostMessage($"There is now {cardCount} cards left in the pile.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void drawBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //draw 1 card
                Card card = shoe.Draw();
                cardListBox.Items.Insert(cardListBox.Items.Count, card);
                findBooks();
                shoe.PostMessage($"There is now {cardCount} cards left in the pile.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void askBtn_Click(object sender, RoutedEventArgs e)
        {
            shoe.PostMessage(playersAskComboBox.SelectedItem + ", do you have any " + cardsAskComboBox.SelectedItem + "?");

            shoe.StoreCard(nameTxtBox.Text, playersAskComboBox.SelectedItem.ToString(), cardsAskComboBox.SelectedItem.ToString());
            //foreach(Card c in cardListBox.Items)
            //{
            //    if(c.Rank.ToString() == cardsAskComboBox.SelectedItem.ToString())
            //    {
            //        Card selected = c;
            //        cardListBox.Items.Remove(c);
            //    }
            //}
        }


        private void connectToMessageBoard()
        {
            try
            {
                // Configure the ABCs of using the MessageBoard service
                DuplexChannelFactory<IShoe> channel = new DuplexChannelFactory<IShoe>(this, "ShoeEndPoint");

                // Activate a MessageBoard object
                shoe = channel.CreateChannel();

                if (shoe.Join(nameTxtBox.Text))
                {
                    // Alias accepted by the service so update GUI
                    boardListBox.ItemsSource = shoe.GetAllMessages();
                    playersListBox.ItemsSource = shoe.GetAllPlayers();
                    nameTxtBox.IsEnabled = nameSetBtn.IsEnabled = false;
                }
                else
                {
                    // Alias rejected by the service so nullify service proxies
                    shoe = null;
                    MessageBox.Show("ERROR: Alias in use. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private delegate void GuiUpdateDelegate(string[] messages);

        public void SendAllMessages(string[] messages)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    boardListBox.ItemsSource = messages;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new GuiUpdateDelegate(SendAllMessages), new object[] { messages });
        }

        public void AddPlayers(string[] names)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    playersListBox.ItemsSource = names;
                    playersAskComboBox.ItemsSource = names;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new GuiUpdateDelegate(AddPlayers), new object[] { names });
        }

        private delegate void ClientUpdateDelegate(CallBackInfo info);

        public void UpdateGui(CallBackInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                // Update the GUI
                cardCount = info.NumCards;
                //sliderDecks.Value = info.NumDecks;
                //txtDeckCount.Text = (info.NumDecks == 1 ? "1 Deck" : info.NumDecks + " Decks");
                if (info.EmptyHand)
                {
                    cardListBox.Items.Clear();
                    //txtHandCount.Text = "0";
                }
            }
            else
            {
                // Not the dispatcher thread that's running this method!
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate(UpdateGui), info);
            }
        }

        public void AskPlayer(CallBackInfo info)
        {

        }

        public void findBooks()
        {
            //List<Card> cards = new List<Card>();
            //foreach (Card c1 in cardListBox.Items) { cards.Add(c1); }
            //List<Card> matches = new List<Card>();
            List<string> matches = new List<string>();
            List<Card> potentialMatches = new List<Card>();

            int count = 0;
            
            for(int i = 0; i < cardListBox.Items.Count; i++)
            {
                Card.RankID r = (cardListBox.Items[i] as Card).Rank;
                for(int j = i + 1; j < cardListBox.Items.Count; j++)
                {
                    Card.RankID nextR = (cardListBox.Items[j] as Card).Rank;
                    if (r == nextR)
                    {
                        count++;
                        Card c = cardListBox.Items[j] as Card;
                        potentialMatches.Add(c);
                    }
                    if (count == 4)
                    {
                        matches.Add(r.ToString());
                        foreach (Card c in potentialMatches)
                            cardListBox.Items.Remove(c);
                    }
                }
            }
            //for(int i = 0; i < cards.Count; i++)
            //{
            //    for (int y = 1; y < cards.Count; y++)
            //    {
            //        if(cards[i].Rank == cards[y].Rank)
            //        {
            //            if(i != y)
            //            {
            //                string book = (cards[i] + " and " + cards[y]);
            //                bookListBox.Items.Insert(0, book);
            //                Card match1 = cards[i];
            //                Card match2 = cards[y];
            //                cards.Remove(match1);
            //                cards.Remove(match2);
            //            }
            //        }
            //    }
            //}
            //cardListBox.Items.Clear();
            //foreach (Card c in cards)
            //{
            //    cardListBox.Items.Insert(0, c);
            //}
        }

        public void loadCardRanksComboBox()
        {
            cardsAskComboBox.ItemsSource = Enum.GetValues(typeof(Card.RankID)).Cast<Card.RankID>();
        }


    }
}
