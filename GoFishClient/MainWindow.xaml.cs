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
        private IShoe shoe = null;
        private int cardCount = 0;
        private bool gameOver = false;
        private bool turnOver = false;
        private Dictionary<string, int> cardMatches = null;
        private int numPlayers = 0;
        private Dictionary<string, List<Card>> cardsOfClients = new Dictionary<string, List<Card>>();

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
                    nameSetBtn.IsEnabled = nameTxtBox.IsEnabled = boardListBox.IsEnabled = true;

                    connectToMessageBoard();
                    shoe.AddPlayer(nameTxtBox.Text);
                    shoe.PostMessage(nameTxtBox.Text + " has joined.");
                    loadComboBoxes();
                    playBtn.IsEnabled = true;
                    cardCount = shoe.NumCards - 5;
                    playersAskComboBox.Items.Remove(nameTxtBox.Text);
                    createCardMatchesDictionary();
                    numPlayersCombobox.SelectedIndex = shoe.NumPlayers;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
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

                    //cardListBox.Items.Insert(0, card);
                    //Console.WriteLine(card);
                    cardMatches[card.Rank.ToString()]++;
                    //shoe.NumCards.ToString();
                    shoe.AddCardToPlayer(nameTxtBox.Text, card);
                }
                
                findBooks();
                playBtn.IsEnabled = false;
                shoe.PostMessage($"There is now {cardCount} cards left in the pile.");

                if (playersListBox.Items.Count == Int32.Parse(numPlayersCombobox.SelectedItem.ToString()))
                {
                    shoe.PostMessage("Start game!");
                }
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
                var something = cardsOfClients;
                //draw 1 card
                Card card = shoe.Draw();
                //cardListBox.Items.Insert(cardListBox.Items.Count, card);
                shoe.AddCardToPlayer(nameTxtBox.Text, card);
                cardMatches[card.Rank.ToString()]++;
                findBooks();
                shoe.PostMessage($"Drawing a card. There is now {cardCount} cards left in the pile.");
                if (shoe.NumCards == 0)
                    gameOver = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void askBtn_Click(object sender, RoutedEventArgs e)
        {
            shoe.PostMessage(playersAskComboBox.SelectedItem + ", do you have any " + cardsAskComboBox.SelectedItem + "?");
            
            if(cardsOfClients.ContainsKey(playersAskComboBox.SelectedItem.ToString()))
            {
                List<Card> cardsOfOtherPlayer = cardsOfClients[playersAskComboBox.SelectedItem.ToString()];
                List<Card> cardsThatWillBePassed = new List<Card>();

                foreach(Card c in cardsOfOtherPlayer)
                {
                    if(c.Rank.ToString() == cardsAskComboBox.SelectedItem.ToString())
                    {
                        cardsThatWillBePassed.Add(c);
                    }
                }

                foreach(Card c in cardsThatWillBePassed)
                {
                    shoe.RemoveCardFromPlayer(playersAskComboBox.SelectedItem.ToString(), c);
                    shoe.AddCardToPlayer(nameTxtBox.Text, c);
                }
            }

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

        private void createCardMatchesDictionary()
        {
            cardMatches = new Dictionary<string, int>();
            var ranks = Enum.GetValues(typeof(Card.RankID)).Cast<Card.RankID>();
            foreach(var r in ranks)
            {
                cardMatches.Add(r.ToString(), 0);
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

        private delegate void ClientCardUpdateDelegate(Dictionary<string, List<Card>> cards);

        public void UpdateCards(Dictionary<string, List<Card>> cards)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    cardListBox.ItemsSource = cards[nameTxtBox.Text];
                    cardsOfClients = cards;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new ClientCardUpdateDelegate(UpdateCards), cards);
        }

        private delegate void ClientUpdateDelegate(CallBackInfo info);

        public void UpdateGui(CallBackInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                // Update the GUI
                cardCount = info.NumCards;
                numPlayers = info.NumPlayers;
                
                if (info.EmptyHand)
                {
                    cardListBox.Items.Clear();
                }
            }
            else
            {
                // Not the dispatcher thread that's running this method!
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate(UpdateGui), info);
            }
        }

        public void findBooks()
        {
            List<Card> cardsdelete = new List<Card>();
            foreach (KeyValuePair<string, int> entry in cardMatches)
            {
                if (entry.Value == 4)
                {
                    bookListBox.Items.Add(entry.Key);
                    for (int i = 0; i < cardListBox.Items.Count; i++)
                    {
                        Card c = cardListBox.Items[i] as Card;
                        if (c.Rank.ToString() == entry.Key)
                        {
                            cardsdelete.Add(c);
                        }
                    }
                    foreach (Card c in cardsdelete)
                        shoe.RemoveCardFromPlayer(nameTxtBox.Text, c);
                    shoe.PostMessage($"Four {entry.Key}s have been found!");
                }
            }
            for(int i = 0; i < bookListBox.Items.Count; i++)
            {
                cardMatches[bookListBox.Items[i].ToString()] = 0;
            }
           
        }

        public void loadComboBoxes()
        {
            cardsAskComboBox.ItemsSource = Enum.GetValues(typeof(Card.RankID)).Cast<Card.RankID>();
            for(int i = 2; i < 6; i++)
                numPlayersCombobox.Items.Add(i);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            shoe.PostMessage(nameTxtBox.Text + " has left.");
            if (shoe != null)
                shoe.Leave(nameTxtBox.Text);
            shoe.RemovePlayer(nameTxtBox.Text);
        }

        private void numPlayersCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if(shoe != null)
                {
                    shoe.NumPlayers = (int)numPlayersCombobox.SelectedIndex;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
