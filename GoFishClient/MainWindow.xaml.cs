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

        private string prefix = "";

        //in board, show card count 

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                DuplexChannelFactory<IShoe> channel = new DuplexChannelFactory<IShoe>(this, "ShoeEndPoint");
                shoe = channel.CreateChannel();

                shoe.RegisterForCallbacks();

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
                    //msgBrd.PostMessage(prefix + nameTxtBox.Text);
                    name = nameTxtBox.Text;
                    nameTxtBox.Clear();
                    playersListBox.Items.Insert(0, name);
                    //TODO: UPDATE ALL USER'S WINDOW

                    //listMessages.ItemsSource = msgBrd.GetAllMessages();
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
                string welcome = (name + " has joined GO FISH");
                //draw 5 cards to each player
                for (var i = 0; i < 5; i++)
                {
                    Card card = shoe.Draw();

                    cardListBox.Items.Insert(0, card);
                    //shoe.NumCards.ToString();
                }
                boardListBox.Items.Insert(0, welcome);
                
                //TODO: CHECK FOR CARD MATCHES(BOOKS)
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
                //card
                //TODO: CHECK FOR CARD MATCHES(BOOKS)
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        // Implement ICallback contract
        private delegate void ClientUpdateDelegate(CallBackInfo info);

        public void UpdateGui(CallBackInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                // Update the GUI
                Console.WriteLine("updateGUI");
            }
            else
            {
                // Only the main (dispatcher) thread can change the GUI
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate(UpdateGui), info);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           // if (shoe != null && callbacksEnabled)
              //  Unsubscribe from the callbacks to prevent a runtime error in the service
               // shoe.ToggleCallbacks();
        }

        public void SendAllMessages(string[] messages)
        {
            throw new NotImplementedException();
        }
    }
}
