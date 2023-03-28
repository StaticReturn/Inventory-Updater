using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using OfficeOpenXml;
using System.Threading;

namespace InventoryMaintenance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void ReadFilesDelegate(string[] files);
        public delegate void RFCallbackDelegate(IMList data);
        public delegate void MessageDelegate(String message);
        string[] files;
        String initials = "";
        IMList masterList = new IMList();
        List<String> errors = new List<String>();
        bool EditEnding = false;

        public MainWindow()
        {
            InitializeComponent();
            MyDataGrid.CurrentCellChanged += RefreshRecheck;
            MyDataGrid.CellEditEnding += Ending;
        }

        void Ending(object sender, EventArgs e)
        {
            EditEnding = true;
        }

        void ClearAllValues()
        {
            masterList = new IMList();
            errors = new List<string>();
            initials = "";
            Errors.Text = "";
        }


        private void Dropbox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void Dropbox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                files = (string[])e.Data.GetData(DataFormats.FileDrop);

                ClearAllValues();
                ReadFilesDelegate load = new ReadFilesDelegate(LoadData);
                load.BeginInvoke(files, null, null);
            }
        }

        void LoadData(string[] files)
        {
            IMList allData = new IMList();
            int count = 1;

            foreach (String file in files)
            {
                String path = System.IO.Path.GetDirectoryName(file);
                String filename = file.Substring(path.Length + 1, file.Length - (path.Length + 1));

                Errors.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new MessageDelegate(PrintMessage),
                    $"Reading {filename} ({count} of {files.Length})..."
                );

                try
                {
                    String worksheetType = "";
                    var package = new ExcelPackage(new FileInfo(file));
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[1];
                    int numberOfRows = workSheet.Dimension.End.Row;
                    int startRow = 13;


                    if (workSheet.Cells[5, 9].Value == null)
                        errors.Add("No initial for this worksheet.");
                    else
                        initials = workSheet.Cells[5, 9].Value.ToString();

                    if (workSheet.Cells[3, 7].Value == null)
                        errors.Add("No worksheet type selected.");
                    else
                        worksheetType = workSheet.Cells[3, 7].Value.ToString();     // "Normal New Items", "Update Worksheet", or "New Deli Items"

                    for (int row = startRow; row < numberOfRows; row++)
                    {
                        List<String> data = new List<String>();

                        if (row > 100)
                        {
                            errors.Add($"Read 100 rows, but this document has more.");
                            break;
                        }

                        for (int column = 1; column < 34; column++)
                            data.Add(workSheet.Cells[row, column].Value == null ? "" : workSheet.Cells[row, column].Value.ToString().Trim());

                        if (data[1] == "" && data[2] == "" && data[3] == "" && data[4] == "" && data[5] == "" && data[6] == "" && data[7] == "" && data[8] == "" && data[17] == "")
                            continue;

                        allData.Add(filename, row, worksheetType, data);
                    }
                }
                catch (Exception e)
                {
                    errors.Add($"Problem reading {filename}: {e.Message}");
                }

                count++;

                Errors.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new MessageDelegate(PrintMessage),
                    $" Done.\n"
                );
            }

            MyDataGrid.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new RFCallbackDelegate(LoadFiles),
                allData
            );
        }

        void PrintMessage(String message)
        {
            Errors.Text += message;
        }

        void LoadFiles(IMList data)
        {
            masterList = data;

            WriteGrid();
            PrintCount();
            CheckData();
        }

        void WriteGrid()
        {
            if (masterList.Count != 0)
            {
                MyDataGrid.ItemsSource = null;
                MyDataGrid.ItemsSource = masterList.items;
            }
        }

        private void RefreshRecheck(object sender, EventArgs e)
        {
            if (MyDataGrid.CurrentColumn != null)
            {
                switch (MyDataGrid.CurrentColumn.DisplayIndex)
                {
                    case 18:    // Deli Scale Price
                    case 27:    // Exception Pricing
                    case 30:    // Ideal Margin
                    case 31:    // Cost
                        if (EditEnding)
                        {
                            MyDataGrid.CommitEdit();
                            MyDataGrid.Items.Refresh();
                            EditEnding = false;
                        }
                        break;
                    default:
                        break;
                }
            }

            CheckData();
        }

        String WriteNewItems()
        {
            String name = DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + DateTime.Now.ToString("yy") + " new items 1.txt";
            String path = System.IO.Path.GetDirectoryName(files[0]) + "\\" + name;
            String data = masterList.InventoryMaintenence("New Item");

            if (masterList.NewItemCount == 0 && masterList.NewDeliItemCout == 0)
                return "";

            if (File.Exists(path))
                File.Delete(path);

            File.WriteAllText(path, data);
            return name;
        }

        String WriteUpdates()
        {
            String name = DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + DateTime.Now.ToString("yy") + " updates 1.txt";
            String path = System.IO.Path.GetDirectoryName(files[0]) + "\\" + name;
            String data = masterList.InventoryMaintenence("Update");

            if (masterList.UpdateCount == 0)
                return "";

            if (File.Exists(path))
                File.Delete(path);

            File.WriteAllText(path, data);
            return name;
        }

        void PrintErrors()
        {
            bool errorsFound = false;
            bool warningsFound = false;
            Errors.Text = "";

            foreach (var error in errors)
            {
                errorsFound = true;
                Errors.Text += error + "\n";
            }

            foreach (var error in masterList.GetErrors())
            {
                errorsFound = true;
                Errors.Text += error + "\n";
            }

            foreach (var warning in masterList.GetWarnings())
            {
                warningsFound = true;
                Errors.Text += warning + "\n";
            }

            if (errorsFound)
            {
                ErrorBox.Content = "✓";
                ErrorBox.Foreground = new SolidColorBrush(Colors.Red);
                WriteButton.IsEnabled = false;
            } else
            {
                ErrorBox.Content = "✗";
                ErrorBox.Foreground = new SolidColorBrush(Colors.Black);
                WriteButton.IsEnabled = true;
            }

            if (warningsFound)
            {
                WarningBox.Content = "✓";
                WarningBox.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                WarningBox.Content = "✗";
                WarningBox.Foreground = new SolidColorBrush(Colors.Black);
            }

            if (!errorsFound && !warningsFound)
                Errors.Text = "No errors found.";
        }

        void PrintCount()
        {
            String newItemCountFound = (masterList.NewItemCount == 1) ? "item" : "items";
            String updateCountFound = (masterList.NewItemCount == 1) ? "update" : "updates";
            Errors.Text = "";
            Errors.Text += $"Found {masterList.NewItemCount} new {newItemCountFound}.";
            Errors.Text += $"Found {masterList.UpdateCount} {updateCountFound}.";
        }

        void UpdateEBTAndHi5()
        {
            masterList.CheckHi5AndEBT();

            if (masterList.EBT != 0)
            {
                EBTBox.Content = "✓";
                EBTBox.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                EBTBox.Content = "✗";
                EBTBox.Foreground = new SolidColorBrush(Colors.Black);
            }

            if (masterList.Hi5 != 0)
            {
                Hi5Box.Content = "✓";
                Hi5Box.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                Hi5Box.Content = "✗";
                Hi5Box.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void CheckData()
        {
            UpdateEBTAndHi5();
            UpdateDeliCode();
            UpdateEDLPCode();
            PrintErrors();
        }

        private void Write_Button_Click(object sender, RoutedEventArgs e)
        {
            String file1 = WriteNewItems();
            String file2 = WriteUpdates();
            String file3 = WriteIngredients();

            Errors.Text = "";

            if (masterList.NewItemCount > 0 || masterList.NewDeliItemCout > 0)
                Errors.Text += $"Wrote \"{file1}\".\n";

            if (masterList.UpdateCount > 0)
                Errors.Text += $"Wrote \"{file2}\".\n";

            if (masterList.NewDeliItemCout > 0)
                Errors.Text += file3;
        }

        void UpdateEDLPCode()
        {
            masterList.CheckEDLP();

            if (masterList.EDLP)
            {
                HSBox.Content = "✓";
                HSBox.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                HSBox.Content = "✗";
                HSBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        void UpdateDeliCode()
        {
            masterList.CheckDeliLabels();

            if (masterList.DeliLabels)
            {
                DeliBox.Content = "✓";
                DeliBox.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                DeliBox.Content = "✗";
                DeliBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        String WriteIngredients()
        {
            Dictionary<String, String> ingredients = masterList.GetIngredients();
            String message = "";

            foreach (var name in ingredients.Keys)
            {
                String path = System.IO.Path.GetDirectoryName(files[0]) + "\\" + name + ".TXT";
                String data = "INGREDIENTS: " + ingredients[name].ToUpper();
                message += $"Wrote \"{name}.TXT\".\n"; ;

                if (File.Exists(path))
                    File.Delete(path);

                File.WriteAllText(path, data);
            }

            return message;
        }
    }
}
