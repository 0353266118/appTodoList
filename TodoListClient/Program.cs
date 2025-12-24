using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json; // Thư viện để làm việc với JSON
using SharedModels;    // Sử dụng các class chung

TcpClient client = new TcpClient();

try
{
    client.Connect("127.0.0.1", 5050);
    Console.WriteLine("Connected to server. Welcome to TodoList!");
    NetworkStream stream = client.GetStream();

    // Bắt đầu một luồng riêng để lắng nghe server
    Task.Run(() => ListenForServerMessages(stream));

    // Luồng chính xử lý việc nhập liệu của người dùng
    ShowHelp();
    while (true)
    {
        Console.Write("> ");
        string input = Console.ReadLine();
        if (string.IsNullOrEmpty(input)) continue;

        string[] parts = input.Split(' ', 2);
        string command = parts[0].ToLower();
        Message messageToSend = null;

        switch (command)
        {
            case "add":
                if (parts.Length > 1)
                {
                    messageToSend = new Message { Action = "add", Payload = parts[1] };
                }
                else
                {
                    Console.WriteLine("Usage: add <task content>");
                }
                break;
            case "delete":
                if (parts.Length > 1)
                {
                    messageToSend = new Message { Action = "delete", Payload = parts[1] };
                }
                else
                {
                    Console.WriteLine("Usage: delete <task id>");
                }
                break;
            case "help":
                ShowHelp();
                break;
            case "exit":
                client.Close();
                return;
            default:
                Console.WriteLine("Unknown command. Type 'help' for a list of commands.");
                break;
        }

        if (messageToSend != null)
        {
            string jsonToSend = JsonConvert.SerializeObject(messageToSend);
            byte[] dataToSend = Encoding.UTF8.GetBytes(jsonToSend);
            stream.Write(dataToSend, 0, dataToSend.Length);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    client.Close();
}

// --- HÀM LẮNG NGHE SERVER (chạy trên luồng nền) ---
void ListenForServerMessages(NetworkStream stream)
{
    byte[] buffer = new byte[4096]; // Tăng buffer size để chứa danh sách lớn
    try
    {
        while (true)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) break; // Server ngắt kết nối

            string serverJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Message serverMessage = JsonConvert.DeserializeObject<Message>(serverJson);

            if (serverMessage.Action == "update_list")
            {
                var tasks = JsonConvert.DeserializeObject<List<TaskItem>>(serverMessage.Payload);
                UpdateConsoleUI(tasks);
            }
        }
    }
    catch
    {
        Console.WriteLine("\nServer has disconnected.");
    }
}

// --- CÁC HÀM HIỂN THỊ GIAO DIỆN ---
void UpdateConsoleUI(List<TaskItem> tasks)
{
    // Xóa màn hình console và vẽ lại từ đầu
    Console.Clear();
    Console.WriteLine("===== TODO LIST =====");
    if (tasks.Count == 0)
    {
        Console.WriteLine("No tasks yet. Use 'add <content>' to add one.");
    }
    else
    {
        foreach (var task in tasks)
        {
            Console.WriteLine($"[ID: {task.Id}] - {task.Content}");
        }
    }
    Console.WriteLine("=====================");
    Console.WriteLine("Commands: add <content>, delete <id>, help, exit");
    Console.Write("> "); // Hiển thị lại dấu nhắc cho người dùng
}

void ShowHelp()
{
    Console.WriteLine("\nAvailable commands:");
    Console.WriteLine("  add <content>   - Add a new task.");
    Console.WriteLine("  delete <id>     - Delete a task by its ID.");
    Console.WriteLine("  help            - Show this help message.");
    Console.WriteLine("  exit            - Close the application.");
}