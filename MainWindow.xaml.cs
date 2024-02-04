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
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataSet dataa;
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Minimalize_Click(object sender, RoutedEventArgs e)
        {
            wind.WindowState = WindowState.Minimized;
        }


        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Excel document";
            dialog.DefaultExt = ".xlsx";
            dialog.Filter = "Text documents (.xlsx)|*.xlsx;*.xls";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                fileNameText.Text = filename.ToString();
                using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        dataa = reader.AsDataSet();
                    }
                }
            }
        }




        private void Check_btn_Click(object sender, RoutedEventArgs e)
        {
            var startDate = StartDate.Text;
            var endDate = EndDate.Text;
            var numerArkusza = NumerArkusza.Text;
            var czyNaglowki = naglowki.IsChecked;
            var numerKolumny = NumerKol.Text.ToString();
            var fileName = fileNameText.Text;
            var NumerOfColumns = 0;
            var NumbersOfTables = 0;
            if (StartDate.SelectedDate == null | EndDate.SelectedDate == null | NumerKol.Text == "" | fileNameText.Text=="..." | NumerArkusza.Text==""){
                ErrorMessage_text.Text = "Fill all gaps";
            }
            else
            {
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
                    foreach (DataColumn column in dataa.Tables[numerArkuszaInt-1].Columns)
                    {
                        NumerOfColumns++;
                    }
                    var numerKolumnyInt = 1;
                    try { numerKolumnyInt = Int32.Parse(numerKolumny); } catch { }
                    if (numerKolumnyInt<=0 | numerKolumnyInt > NumerOfColumns)
                    {
                        ErrorMessage_text.Text = "That column does not exist";
                    }
                    else
                    {
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
                                    bool i;
                                    if ((bool)czyNaglowki)
                                    {
                                        i = true;
                                    }
                                    else
                                    {
                                        i = false;
                                    }

                                    foreach (DataRow row in data.Tables[numerArkuszaInt-1].Rows)
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
                                urodziny_text.Text = "";
                                List<Persons> SortedList = arrayOfBirtyday.OrderBy(o => o.diference.TotalDays).ToList();
                                if(SortedList.Count == 0)
                                {
                                    urodziny_text.Text += "Brak urodzin na tym wyjeździe";
                                }
                                else
                                {
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
