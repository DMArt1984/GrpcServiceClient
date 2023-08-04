using System.IO;

namespace WpfAppClient1
{
    static public class Settings
    {
        // получение строки подключения в БД
        static public string GetConnectString(string path = "setting.txt")
        {
            string connect = "https://localhost:7254";
            if (string.IsNullOrEmpty(path))
                return connect;

            // чтение
            using (StreamReader reader = new StreamReader(path))
            {
                string text = reader.ReadLine();
                connect = string.IsNullOrWhiteSpace(text) ? connect : text;
            }
            // выход
            return connect;
        }
    }
}
