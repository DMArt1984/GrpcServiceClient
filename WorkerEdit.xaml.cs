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
using System.Windows.Shapes;

namespace WpfAppClient1
{
    /// <summary>
    /// Логика взаимодействия для WorkerEdit.xaml
    /// </summary>
    public partial class WorkerEdit : Window
    {
        public WorkerReply worker { get; private set; }
        public WorkerEdit(WorkerReply worker)
        {
            InitializeComponent();
            this.worker = worker;
            DataContext = worker;
        }

        void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
