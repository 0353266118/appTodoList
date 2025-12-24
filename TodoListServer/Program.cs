using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json; // Thư viện để làm việc với JSON
using SharedModels;    // Sử dụng các class chung

// --- PHẦN DỮ LIỆU CHUNG VÀ TRẠNG THÁI CỦA SERVER ---

// static: Để tất cả các luồng (mỗi luồng xử lý 1 client) đều truy cập được vào CÙNG MỘT danh sách
List<TaskItem> tasks = new List<TaskItem>();
List<TcpClient> allClients = new List<TcpClient>();
int nextTaskId = 0;

// lockObject: Giống như một cái "chìa khóa".
// Luồng nào giữ chìa khóa thì mới được phép chỉnh sửa danh sách tasks, tránh xung đột.
object lockObject = new object();

Console.WriteLine("Starting TodoList Server...");
TcpListener server = new TcpListener(IPAddress.Any, 8989);
server.Start();
Console.WriteLine("Server started. Waiting for clients...");

// Vòng lặp chính của server: Chỉ chấp nhận kết nối và giao việc cho luồng khác
while (true)
{
    TcpClient client = server.AcceptTcpClient();

    // Thêm client mới vào danh sách quản lý
    allClients.Add(client);
    Console.WriteLine($"Client connected! Total clients: {allClients.Count}");

    // Tạo một luồng mới (Task) để xử lý riêng client này
    // mà không làm ảnh hưởng đến việc chấp nhận các client khác.
    Task.Run(() => HandleClient(client));
}

// --- HÀM XỬ LÝ CHO TỪNG CLIENT ---
void HandleClient(TcpClient client)
{
    NetworkStream stream = client.GetStream();
    // Gửi danh sách công việc hiện tại cho client vừa kết nối
    SendCurrentTasksToOneClient(client);

    try
    {
        while (true)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) break; // Client ngắt kết nối

            string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            SharedModels.Message message = JsonConvert.DeserializeObject<SharedModels.Message>(dataReceived);

            // Xử lý yêu cầu dựa trên Action
            lock (lockObject) // <-- Yêu cầu "chìa khóa" trước khi thay đổi dữ liệu chung
            {
                switch (message.Action)
                {
                    case "add":
                        var newTask = new TaskItem { Id = nextTaskId++, Content = message.Payload };
                        tasks.Add(newTask);
                        Console.WriteLine($"Added new task: '{message.Payload}'");
                        break;
                    case "delete":
                        int taskIdToDelete = int.Parse(message.Payload);
                        var taskToRemove = tasks.SingleOrDefault(t => t.Id == taskIdToDelete);
                        if (taskToRemove != null)
                        {
                            tasks.Remove(taskToRemove);
                            Console.WriteLine($"Deleted task ID: {taskIdToDelete}");
                        }
                        break;
                    case "update":
                        // 1. Giải nén payload thành một đối tượng TaskItem
                        TaskItem updatedTaskData = JsonConvert.DeserializeObject<TaskItem>(message.Payload);

                        // 2. Tìm công việc cũ trong danh sách dựa trên ID
                        var taskToUpdate = tasks.SingleOrDefault(t => t.Id == updatedTaskData.Id);

                        // 3. Nếu tìm thấy, cập nhật lại nội dung
                        if (taskToUpdate != null)
                        {
                            taskToUpdate.Content = updatedTaskData.Content;
                            Console.WriteLine($"Updated task ID {taskToUpdate.Id} to '{taskToUpdate.Content}'");
                        }
                        break;
                }
            } // <-- Trả lại "chìa khóa"

            // Sau khi có thay đổi, thông báo cho TẤT CẢ client
            BroadcastTasksToAllClients();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error with a client: {ex.Message}");
    }
    finally
    {
        // Dọn dẹp khi client ngắt kết nối
        allClients.Remove(client);
        client.Close();
        Console.WriteLine($"Client disconnected. Total clients: {allClients.Count}");
    }
}

// --- CÁC HÀM GỬI DỮ LIỆU ---

// Gửi danh sách công việc cho TẤT CẢ client
void BroadcastTasksToAllClients()
{
    string tasksJson = JsonConvert.SerializeObject(tasks);
    SharedModels.Message messageToSend = new SharedModels.Message { Action = "update_list", Payload = tasksJson };
    string messageJson = JsonConvert.SerializeObject(messageToSend);
    byte[] dataToSend = Encoding.UTF8.GetBytes(messageJson);

    // Tạo một bản sao của danh sách client để tránh lỗi khi có client ngắt kết nối lúc đang gửi
    foreach (var client in new List<TcpClient>(allClients))
    {
        try
        {
            NetworkStream stream = client.GetStream();
            stream.Write(dataToSend, 0, dataToSend.Length);
        }
        catch
        {
            // Nếu có lỗi khi gửi (client đã ngắt kết nối), loại bỏ nó
            allClients.Remove(client);
        }
    }
    Console.WriteLine("Broadcasted updated task list to all clients.");
}

// Gửi danh sách công việc chỉ cho một client (dùng khi client mới kết nối)
void SendCurrentTasksToOneClient(TcpClient client)
{
    string tasksJson = JsonConvert.SerializeObject(tasks);
    SharedModels.Message messageToSend = new SharedModels.Message { Action = "update_list", Payload = tasksJson };
    string messageJson = JsonConvert.SerializeObject(messageToSend);
    byte[] dataToSend = Encoding.UTF8.GetBytes(messageJson);

    try
    {
        NetworkStream stream = client.GetStream();
        stream.Write(dataToSend, 0, dataToSend.Length);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to send initial list to client: {ex.Message}");
    }
}