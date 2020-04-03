using CardLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CardServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost servHost = null;

            try
            {
                // Register the service Address
                servHost = new ServiceHost(typeof(Shoe));

                // Run the service
                servHost.Open();
                Console.WriteLine("Go Fish has started. Press any key to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Wait for a keystroke
                Console.ReadKey();
                if (servHost != null)
                    servHost.Close();
            }
        }
    }
}
