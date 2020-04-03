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

        //in board, show card count 

        public MainWindow()
        {
            InitializeComponent();

            ChannelFactory<IShoe> channel = new ChannelFactory<IShoe>("ShoeEndPoint");
            shoe = channel.CreateChannel();


        }

        private void nameSetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (nameTxtBox.Text != "")
            {
                try
                {
                    //msgBrd.PostMessage(prefix + nameTxtBox.Text);
                    nameTxtBox.Clear();
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
                //draw 5 cards to each player
                for (var i = 0; i < 5; i++)
                {
                    string card = shoe.Draw();

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
                string card = shoe.Draw();
                cardListBox.Items.Insert(cardListBox.Items.Count, card);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
