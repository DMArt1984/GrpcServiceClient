using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Grpc.Core;
using Grpc.Net.Client;

namespace WpfAppClient1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GrpcChannel channel;
        Accounter.AccounterClient client;
        
        public MainWindow()
        {
            InitializeComponent();

            // события
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        // Загрузка окна
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // подключение
            channel = GrpcChannel.ForAddress("https://localhost:7254"); // адрес сервера актуален?
            client = new Accounter.AccounterClient(channel); // мы - клиент!

            // обновление списка
            await Update();
        }

        // При закрытии окна
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // ничего не делаем, а могли бы...
        }

        // Проверка связи
        private async void TestServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // поприветствовать сервер
                var result = await client.SayHelloAsync(new HelloRequest() { Name = "Админ" });
                // ответ сервера
                MessageBox.Show(result.Message, result.Status == StatusConnect.Ok ? "Связь в норме" : "Ошибка");

            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка " + ex.HResult.ToString("X"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обновление списка
        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            await Update();
        }
        private async Task Update()
        {
            try
            {
                // команда на получение списка
                ListReply workers = await client.ListWorkersAsync(new Google.Protobuf.WellKnownTypes.Empty());
                ObservableCollection<WorkerReply> collection = new ObservableCollection<WorkerReply>(workers.Workers.OrderBy(x => x.FirstName).ToList());
                // если на сервере возникнет исключение, то мы получим пустой список, в рамках данной задачи этого достаточно!

                // устанавливаем список в качестве контекста
                DataContext = collection;
            }
            catch (Exception ex) 
            { 
                MessageBox.Show(ex.Message, "Ошибка получения списка " + ex.HResult.ToString("X"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        // Добавление
        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            try {
                WorkerEdit workerEdit = new WorkerEdit(new WorkerReply());
                if (workerEdit.ShowDialog() == true)
                {
                    WorkerReply worker = workerEdit.worker;

                    if (worker == null)
                        return; // очень странно!

                    // проверка наличия имени
                    if (String.IsNullOrWhiteSpace(worker.FirstName))
                    {
                        MessageBox.Show("В следующий раз, хотя бы имя задайте!", "Маленькая просьба");
                        return; // попробуй еще раз
                    }

                    // команда на добавление
                    var result = await client.CreateWorkerAsync(new CreateWorkerRequest
                    {
                        Worker = worker
                    });

                    // выполнено
                    await Footer(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка добавления " + ex.HResult.ToString("X"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Редактирование
        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            try {
                // получаем выделенный объект
                WorkerReply? worker = workersList.SelectedItem as WorkerReply;

                // если ни одного объекта не выделено, выходим
                if (worker is null) return;

                // объект из окна в объект для команды
                WorkerEdit workerEdit = new WorkerEdit(new WorkerReply
                {
                    Id = worker.Id,
                    FirstName = worker.FirstName,
                    LastName = worker.LastName,
                    MiddleName = worker.MiddleName,
                    BirthDay = worker.BirthDay,
                    Sex = worker.Sex,
                    HaveChildren = worker.HaveChildren
                });

                if (workerEdit.ShowDialog() == true)
                {
                    // проверка наличия имени
                    if (String.IsNullOrWhiteSpace(worker.FirstName))
                    {
                        MessageBox.Show("В следующий раз, хотя бы имя задайте!", "Маленькая просьба");
                        return; // попробуй еще раз
                    }

                    // команда на изменение
                    var result = await client.UpdateWorkerAsync(new UpdateWorkerRequest { 
                        Worker= workerEdit.worker,
                    });

                    // выполнено
                    await Footer(result);
                }
                }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка редактирования " + ex.HResult.ToString("X"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Удаление
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            try {
                // получаем выделенный объект
                WorkerReply? worker = workersList.SelectedItem as WorkerReply;

                // если ни одного объекта не выделено, выходим
                if (worker is null) return;

                // команда на удаление
                var result = await client.DeleteWorkerAsync(new DeleteWorkerRequest { Id = worker.Id });

                // выполнено
                await Footer(result);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка удаления " + ex.HResult.ToString("X"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Завершающие действия после выполнения некоторых команд
        private async Task Footer(Answer answer)
        {
            // сервер что-то хочет сообщить!
            if (answer?.Code != 0)
                MessageBox.Show(answer.Text, "Код ответа сервера " + answer.Code.ToString("X"));

            // обновление списка
            await Update();
        }

    }
}
