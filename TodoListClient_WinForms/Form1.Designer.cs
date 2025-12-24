namespace TodoListClient_WinForms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            taskTextBox = new TextBox();
            addButton = new Button();
            deleteButton = new Button();
            tasksListBox = new ListBox();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            SuspendLayout();
            // 
            // taskTextBox
            // 
            taskTextBox.Location = new Point(100, 346);
            taskTextBox.Name = "taskTextBox";
            taskTextBox.Size = new Size(600, 27);
            taskTextBox.TabIndex = 1;
            // 
            // addButton
            // 
            addButton.Location = new Point(505, 394);
            addButton.Name = "addButton";
            addButton.Size = new Size(94, 29);
            addButton.TabIndex = 2;
            addButton.Text = "Add";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += addButton_Click;
            // 
            // deleteButton
            // 
            deleteButton.Location = new Point(653, 394);
            deleteButton.Name = "deleteButton";
            deleteButton.Size = new Size(94, 29);
            deleteButton.TabIndex = 3;
            deleteButton.Text = "Delete";
            deleteButton.UseVisualStyleBackColor = true;
            deleteButton.Click += deleteButton_Click;
            // 
            // tasksListBox
            // 
            tasksListBox.FormattingEnabled = true;
            tasksListBox.Location = new Point(35, 12);
            tasksListBox.Name = "tasksListBox";
            tasksListBox.Size = new Size(728, 324);
            tasksListBox.TabIndex = 0;
            tasksListBox.SelectedIndexChanged += tasksListBox_SelectedIndexChanged;
            tasksListBox.DoubleClick += tasksListBox_DoubleClick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(deleteButton);
            Controls.Add(addButton);
            Controls.Add(taskTextBox);
            Controls.Add(tasksListBox);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private TextBox taskTextBox;
        private Button addButton;
        private Button deleteButton;
        private ListBox tasksListBox;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;

    }
}
