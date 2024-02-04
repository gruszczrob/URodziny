using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace URodziny
{

    public partial class MainWindow : Window
    {
        //Creating DataSet witch will contain data from excel as data set
        DataSet dataa;

        public MainWindow()
        {
            InitializeComponent();

        }

        //Add functionality on click to Close Button
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            //Close App
            System.Windows.Application.Current.Shutdown();
        }

        //Add functionality on click to Minimalize Button
        private void Minimalize_Click(object sender, RoutedEventArgs e)
        {
            //Minimalize app
            wind.WindowState = WindowState.Minimized;
        }

        //Add functionality on click to Browse Button
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            //Create new dialog window for user to search excel file with *.xlsx and *.xls extencions
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Excel document";
            dialog.DefaultExt = ".xlsx";
            dialog.Filter = "Excel documents (.xlsx)|*.xlsx;*.xls";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                fileNameText.Text = filename.ToString();
                using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        //DataSet that was created at the beginning of program now has value
                        dataa = reader.AsDataSet();
                    }
                }
            }
        }



        //Add functionality on click to Chcek Button 
        private void Check_btn_Click(object sender, RoutedEventArgs e)
        {
            //Getting data from fields witch user filled
            var startDate = StartDate.Text;
            var endDate = EndDate.Text;
            var numerArkusza = NumerArkusza.Text;
            var czyNaglowki = naglowki.IsChecked;
            var numerKolumny = NumerKol.Text.ToString();
            var fileName = fileNameText.Text;
            var NumerOfColumns = 0;
            var NumbersOfTables = 0;

            //Checking if user filled all fields
            if (StartDate.SelectedDate == null | EndDate.SelectedDate == null | NumerKol.Text == "" | fileNameText.Text == "..." | NumerArkusza.Text == "")
            {
                ErrorMessage_text.Text = "Fill all gaps";
            }
            else
            {
                //Checking if user gave us correct sheet number 
                var numerArkuszaInt = 0;
                try { numerArkuszaInt = Int32.Parse(numerArkusza); } catch { }
                foreach (var arkusz in dataa.Tables)
                {
                    NumbersOfTables++;
                }
                if (numerArkuszaInt > NumbersOfTables | numerArkuszaInt < 0)
                {
                    ErrorMessage_text.Text = "That table does not exist";
                }
                else
                {

                    //Checking if user gave us correct column number
                    foreach (DataColumn column in dataa.Tables[numerArkuszaInt - 1].Columns)
                    {
                        NumerOfColumns++;
                    }
                    var numerKolumnyInt = 1;
                    try { numerKolumnyInt = Int32.Parse(numerKolumny); } catch { }
                    if (numerKolumnyInt <= 0 | numerKolumnyInt > NumerOfColumns)
                    {
                        ErrorMessage_text.Text = "That column does not exist";
                    }
                    else
                    {
                        //Checking if date of birth is during the trip
                        ErrorMessage_text.Text = "";
                        using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                        {
                            using (var reader = ExcelReaderFactory.CreateReader(stream))
                            {
                                var data = reader.AsDataSet();
                                DateTime endDateTime = DateTime.Parse(endDate);
                                DateTime startDateTime = DateTime.Parse(startDate);
                                List<Persons> arrayOfBirtyday = new List<Persons>();
                                var z = endDateTime.Year - startDateTime.Year;
                                var Year = startDateTime.Year + z;
                                while (z >= 0)
                                {
                                    //If table contains column names we are skipping first row
                                    bool i;
                                    if ((bool)czyNaglowki)
                                    {
                                        i = true;
                                    }
                                    else
                                    {
                                        i = false;
                                    }

                                    foreach (DataRow row in data.Tables[numerArkuszaInt - 1].Rows)
                                    {
                                        if (!i)
                                        {
                                            string dataZPliku = row[numerKolumnyInt - 1].ToString();
                                            try
                                            {
                                                DateTime dataUrodzenia = DateTime.Parse(dataZPliku);
                                                DateTime DataUrodze = new DateTime(Year, dataUrodzenia.Month, dataUrodzenia.Day);

                                                if (DataUrodze.Ticks <= endDateTime.Ticks & DataUrodze.Ticks >= startDateTime.Ticks)
                                                {

                                                    //If tour participant has birthday on the trip we are creating new Object of Persons typ with his data. And we are adding this new object to list 
                                                    arrayOfBirtyday.Add(new Persons(row[0].ToString(), row[1].ToString(), dataUrodzenia, startDateTime, Year));
                                                }
                                            }
                                            catch
                                            {
                                                ErrorMessage_text.Text = "Given Column does not contain dates or has headlines";
                                                break;
                                            }
                                        }
                                        i = false;
                                    }
                                    Year--;
                                    z--;
                                }
                                //Sorting of this list
                                urodziny_text.Text = "";
                                List<Persons> SortedList = arrayOfBirtyday.OrderBy(o => o.diference.TotalDays).ToList();
                                if (SortedList.Count == 0)
                                {
                                    urodziny_text.Text += "Brak urodzin na tym wyjeździe";
                                }
                                else
                                {
                                    //Print this list
                                    foreach (Persons person in SortedList)
                                    {
                                        urodziny_text.Text += "\n" + person.print();
                                    }
                                }

                            }
                        }
                    }
                }

            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
