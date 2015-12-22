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

using Microsoft.Win32;
using System.IO;
using System.Globalization;

namespace RaboNAB
{
    public enum Flow { Inflow, Outflow };

    public class Transaction
    {       
        public string OwnerAccount { get; set; }
        public string Currency { get; set; }
        public DateTime ClearDate { get; set; }
        public Flow FlowType { get; set; }
        public decimal Amount { get; set; }
        public string CounterAccount { get; set; }
        public string CounterAccountName { get; set; }
        public DateTime Date { get; set; }
        public string BookingCode { get; set; }
        public string Filler { get; set; }
        public string Description { get; set; }
        public string EndToEndId { get; set; }
        public string CounterAccountId { get; set; }
        public string MandateId { get; set; }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true)
                return;

            List<Transaction> transactions = new List<Transaction>();
            var lines = File.ReadAllLines(openFileDialog.FileName);
            var csv = (from line in lines
              select line.Split(',')).ToList();
            
            StreamWriter file = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\YNAB_" + DateTime.Now.Ticks + ".csv");
            file.WriteLine("Date,Payee,Category,Memo,Outflow,Inflow");


            var pbStatusInterval = Math.Ceiling(100.0 / csv.Count);

            foreach(var csvLine in csv) {
                for(int i = 0; i < csvLine.Length; i ++) {
                    csvLine[i] = csvLine[i].Replace("\"", "");
                }

                var transaction = new Transaction();

                transaction.OwnerAccount       = csvLine[0];
                transaction.Currency           = csvLine[1];
                transaction.ClearDate          = DateTime.ParseExact(csvLine[2], "yyyyMMdd", CultureInfo.InvariantCulture);

                if (csvLine[3] == "D")
                    transaction.FlowType       = Flow.Outflow;
                else if (csvLine[3] == "C")
                    transaction.FlowType       = Flow.Inflow;
                else throw new Exception();

                transaction.Amount             = Decimal.Parse(csvLine[4], CultureInfo.InvariantCulture);
                transaction.CounterAccount     = csvLine[5];
                transaction.CounterAccountName = csvLine[6];
                transaction.Date               = DateTime.ParseExact(csvLine[7], "yyyyMMdd", CultureInfo.InvariantCulture);
                transaction.BookingCode        = csvLine[8];
                transaction.Filler             = csvLine[9];
                transaction.Description        = csvLine[10] + csvLine[11] + csvLine[12] + csvLine[13] + csvLine[14] + csvLine[15];
                transaction.EndToEndId         = csvLine[16];
                transaction.CounterAccountId   = csvLine[17];
                transaction.MandateId          = csvLine[18];

                transactions.Add(transaction);

                var flow = "";
                var amount = transaction.Amount.ToString("0.00", CultureInfo.InvariantCulture);
                if (transaction.FlowType == Flow.Inflow)
                    flow = "," + amount;
                else if (transaction.FlowType == Flow.Outflow)
                    flow = amount + ",";
                else throw new Exception();

                if (transaction.CounterAccountName == "")
                {
                    transaction.CounterAccountName = transaction.Description;
                    transaction.Description = "";
                }

                file.WriteLine(String.Format("{0:dd/MM/yyyy},{1},{2},{3},{4}", transaction.Date, transaction.CounterAccountName, "", transaction.Description, flow));

                pbStatus.Value += pbStatusInterval;
            }

            file.Close();
            
        }
    }
}
