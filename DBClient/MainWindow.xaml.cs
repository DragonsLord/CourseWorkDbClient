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
using System.Data.Entity;

namespace DBClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CourseWorkEntities context; // контекст БД

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                context = new CourseWorkEntities(); // Створити з'єднання з БД
                // завантажити в локальне сховище дані з таблиць
                context.Addressers.Load();
                context.Topics.Load();
                context.Messages.Load();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

            TablesBinding();
        }

        /// <summary>
        /// Прив'язка даних до таблиць застосування
        /// </summary>
        private void TablesBinding()
        {
            AddressersTable.ItemsSource = context.Addressers.Local;
            TopicsTable.ItemsSource = context.Topics.Local;
            MessagesTable.ItemsSource = context.Messages.Local;

            // Прив'язка зовнішніх посилань
            ((DataGridComboBoxColumn)MessagesTable.Columns[2]).ItemsSource = context.Topics.Local.ToBindingList();
            ((DataGridComboBoxColumn)MessagesTable.Columns[3]).ItemsSource = context.Addressers.Local.ToBindingList();
            ((DataGridComboBoxColumn)MessagesTable.Columns[4]).ItemsSource = context.Addressers.Local.ToBindingList();
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            try
            {
                await context.SaveChangesAsync(); // Зберегти дані в БД в асинхронному режимі
            }
            catch (Exception) { System.Windows.Forms.MessageBox.Show(String.Format("Couldn't save data.\n Incorrect data input")); }
            this.Cursor = Cursors.Arrow;
        }

        //Відображення вкладених даних(повідомлень) для таблиці Тем
        private void TopicsTable_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e)
        {
            var current_topic = e.Row.Item as Topic;
            var messages = (DataGrid)e.DetailsElement;
            if (messages.ItemsSource == null && e.Row.IsVisible)
            {
                messages.ItemsSource = current_topic.Messages; // прив'язатись до повідомлень з поточною темою
            }
        }

        /// <summary>
        /// Згенерувати звіт по діяльності депутата
        /// </summary>
        private void FormReport()
        {
            var report = context.GetWorkReport(); // викликати збережену процедуру GetWorkReport

            this.ReportBlock.Inlines.Add(new Run("Звіт по діяльності депутата") { FontSize = 20, FontWeight = FontWeights.Bold, FontFamily = new FontFamily("Times New Roman") });
            this.ReportBlock.Inlines.Add(new LineBreak());
            foreach (var report_line in report)
            {
                this.ReportBlock.Inlines.Add(new Run(String.Format("Питань {0}: {1}", report_line.Status, report_line.Amount)) { FontSize = 18, FontFamily = new FontFamily("Times New Roman") });
                this.ReportBlock.Inlines.Add(new LineBreak());
                var topics = from t in context.Topics where t.Status == report_line.Status select t;
                foreach (var topic in topics)
                {
                    this.ReportBlock.Inlines.Add(new Run(String.Format("\t- {0} [{1}]", topic.Name, topic.CreationDate.ToShortDateString())) { FontSize = 14, FontFamily = new FontFamily("Calibri") });
                    this.ReportBlock.Inlines.Add(new LineBreak());
                }
                this.ReportBlock.Inlines.Add(new LineBreak());
            }
        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            ReportBlock.Inlines.Clear();
            FormReport();
        }

        /// <summary>
        /// Перевірка коректності видалення даних з таблиці Адресантів
        /// </summary>
        private void AddressersTable_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            { 
                foreach (Addresser row in AddressersTable.SelectedItems)
                {
                    if (row.RecievedMessages.Count != 0 || row.SentMessages.Count != 0)
                    {
                        e.Handled = true;
                        System.Windows.Forms.MessageBox.Show("Can't delete because it's used in another table.");
                    }
                }
            }
        }

        /// <summary>
        /// Перевірка коректності видалення даних з таблиці Тем
        /// </summary>
        private void TopicsTable_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                foreach (Topic row in TopicsTable.SelectedItems)
                {
                    if (row.Messages.Count != 0)
                    {
                        e.Handled = true;
                        System.Windows.Forms.MessageBox.Show("Can't delete because it's used in another table.");
                    }
                }
            }
        }
    }
}
