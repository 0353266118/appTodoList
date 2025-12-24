using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using SharedModels;

namespace TodoListClient_WinForms
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private List<TaskItem> currentTasks = new List<TaskItem>();
        // CancellationTokenSource để ra hiệu cho luồng nền dừng lại một cách an toàn
        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
            // Đăng ký sự kiện FormClosing để dọn dẹp tài nguyên khi đóng Form
            this.FormClosing += Form1_FormClosing;
        }

        // Sự kiện chạy KHI CỬA SỔ ĐƯỢC TẢI LÊN
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Todo List Client";
            deleteButton.Enabled = false;
            addButton.Enabled = false;

            cts = new CancellationTokenSource();

            // Bắt đầu kết nối và lắng nghe trên một luồng nền để không làm treo giao diện
            Task.Run(() => ConnectAndListen(cts.Token));
        }

        // Toàn bộ logic mạng chạy trên luồng nền
        private void ConnectAndListen(CancellationToken cancellationToken)
        {
            try
            {
                client = new TcpClient();
                // Hãy chắc chắn cổng này khớp với Server (ví dụ: 8989)
                client.Connect("localhost", 8989);
                stream = client.GetStream();

                // Khi kết nối thành công, nhờ luồng UI hiển thị thông báo và bật nút Add
                this.Invoke(new Action(() => {
                    MessageBox.Show("Connected to server successfully!");
                    addButton.Enabled = true;
                }));

                // Bắt đầu vòng lặp lắng nghe server
                byte[] buffer = new byte[4096];
                while (!cancellationToken.IsCancellationRequested) // Vòng lặp sẽ dừng khi token bị hủy
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string serverJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        SharedModels.Message serverMessage = JsonConvert.DeserializeObject<SharedModels.Message>(serverJson);

                        if (serverMessage.Action == "update_list")
                        {
                            var tasksFromServer = JsonConvert.DeserializeObject<List<TaskItem>>(serverMessage.Payload);
                            this.Invoke(new Action(() => UpdateTaskListUI(tasksFromServer)));
                        }
                    }
                    else
                    {
                        // Tạm nghỉ 100ms để tránh chiếm dụng CPU
                        Task.Delay(100).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, nhờ luồng UI hiển thị
                if (!IsDisposed)
                {
                    this.Invoke(new Action(() => {
                        MessageBox.Show($"Connection failed or lost: {ex.Message}");
                        addButton.Enabled = false;
                        deleteButton.Enabled = false;
                    }));
                }
            }
        }

        // Sự kiện chạy NGAY TRƯỚC KHI CỬA SỔ BỊ ĐÓNG LẠI
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ra hiệu cho luồng nền dừng lại
            cts?.Cancel();
            // Đóng kết nối và giải phóng tài nguyên
            stream?.Close();
            client?.Close();
        }

        // HÀM CẬP NHẬT GIAO DIỆN (Giữ nguyên)
        private void UpdateTaskListUI(List<TaskItem> tasks)
        {
            currentTasks = tasks;
            tasksListBox.Items.Clear();
            foreach (var task in currentTasks)
            {
                tasksListBox.Items.Add($"[ID: {task.Id}] - {task.Content}");
            }
        }

        // HÀM GỬI DỮ LIỆU (Giữ nguyên)
        private void SendMessageToServer(SharedModels.Message message)
        {
            if (stream == null || !client.Connected)
            {
                MessageBox.Show("Not connected to server.");
                return;
            }
            try
            {
                string jsonToSend = JsonConvert.SerializeObject(message);
                byte[] dataToSend = Encoding.UTF8.GetBytes(jsonToSend);
                stream.Write(dataToSend, 0, dataToSend.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending data: {ex.Message}");
            }
        }

        // CÁC SỰ KIỆN CỦA NÚT BẤM (Giữ nguyên)
        private void addButton_Click(object sender, EventArgs e)
        {
            string taskContent = taskTextBox.Text;
            if (string.IsNullOrWhiteSpace(taskContent))
            {
                MessageBox.Show("Please enter a task content.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var message = new SharedModels.Message { Action = "add", Payload = taskContent };
            SendMessageToServer(message);
            taskTextBox.Clear();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = tasksListBox.SelectedIndex;
            if (selectedIndex == -1)
            {
                MessageBox.Show("Please select a task to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            TaskItem selectedTask = currentTasks[selectedIndex];
            var message = new SharedModels.Message { Action = "delete", Payload = selectedTask.Id.ToString() };
            SendMessageToServer(message);
        }

        private void tasksListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            deleteButton.Enabled = (tasksListBox.SelectedIndex != -1);
        }
    }
}