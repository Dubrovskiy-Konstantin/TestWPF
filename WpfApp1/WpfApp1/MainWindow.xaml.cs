using System;
using System.IO;
using Microsoft.Win32;
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
using WpfApp1.Logic;
using Excel = Microsoft.Office.Interop.Excel;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=TestWPF;Integrated Security=False";
        private DirectoryInfo _directory = null;
        private FileInfo[] _generatedFiles = null;
        private FileInfo _combinedFile = null;
        private int _counter = 0;
        private int _numberOfFiles = 0;
        private int _numberOfLines = 0;

        private List<Bill> _Bills { get; set; } = new List<Bill>();

        public MainWindow()
        {
            InitializeComponent();
            excelDataGrid.Items.Clear();
            GetListOfFilesTask2();
        }

        private void generateFilesTask1Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(numberOfFilesTexBox.Text, out _numberOfFiles))
                {
                    MessageBox.Show($"{numberOfFilesTexBox.Text} - не число");
                    return;
                }

                if (!int.TryParse(numberOfLinesTexBox.Text, out _numberOfLines))
                {
                    MessageBox.Show($"{numberOfLinesTexBox.Text} - не число");
                    return;
                }

                Generator generator = new Generator();
                _directory = new DirectoryInfo(directoryPathTexBox.Text);
                _generatedFiles = generator.GenereFiles(_directory, _numberOfFiles, _numberOfLines);
                MessageBox.Show("Успешно сгенерировано");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void joinFilesTask1Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Combinator combinator = new Combinator();
                string deletePattern = deletePatternTextBox.Text;
                _combinedFile = combinator.Combine(_generatedFiles, deletePattern, out int deletedLines);
                deletedLinesLabel.Content = deletedLines.ToString();
                MessageBox.Show("Успешно объединено");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void importFileToDBTask1Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DBImporter importer = new DBImporter(_connectionString);
                string result = string.Empty;
                importer.ImportFromFile(_combinedFile, (string str) =>
                {
                    _counter++;
                    result = str;
                    // Показывать не всё, чтобы бысртее работало
                    if (_counter % 200 == 0)
                    {
                        importProgressLabel.Content = result;
                    }
                });
                MessageBox.Show("Успешно импортировано");
                importProgressLabel.Content = $"Успешно {result}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void addNewFileTask2Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.ShowDialog();
                FileInfo file = new FileInfo(fileDialog.FileName);
                ExcelImporter excelImporter = new ExcelImporter(_connectionString);
                excelImporter.ImportFile(file);
                MessageBox.Show("Успешно импортировано");
                GetListOfFilesTask2();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void getFileTask2Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filename = excelFilesComboBox.SelectedItem?.ToString();
                excelDataGrid.Items.Clear();
                ExcelExporter excelImporter = new ExcelExporter(_connectionString);
                _Bills = excelImporter.GetBills(filename);
                foreach(Bill it in _Bills)
                {
                    excelDataGrid.Items.Add(it);
                }

                MessageBox.Show("Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GetListOfFilesTask2()
        {
            excelFilesComboBox.Items.Clear();
            foreach (string it in new ExcelExporter(_connectionString).GetFileNames())
            {
                excelFilesComboBox.Items.Add(it);
            }
        }
    }
}
