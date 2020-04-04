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
    public partial class MainWindow : Window
    {
        private string name = "";
        private IShoe shoe = null;
        private int cardCount = 0;
        private bool callbacksEnabled = false;

        //in board, show card count 

        public MainWindow()
        {
            InitializeComponent();

            ChannelFactory<IShoe> channel = new ChannelFactory<IShoe>("ShoeEndPoint");
            shoe = channel.CreateChannel();


            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
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
                    //shoe.NumCards.ToString();
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
                //draw 1 card
                Card card = shoe.Draw();
                cardListBox.Items.Insert(cardListBox.Items.Count, card);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void connectToMessageBoard()
        {
            try
            {
                // Configure the ABCs of using the MessageBoard service
                DuplexChannelFactory<IShoe> channel = new DuplexChannelFactory<IShoe>(this, "MessagingService");

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
                //txtShoeCount.Text = info.NumCards.ToString();
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
    }
}
