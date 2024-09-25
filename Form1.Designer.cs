namespace WindowsMotors
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
			button1 = new Button();
			textBox1 = new TextBox();
			button2 = new Button();
			txtOutput = new TextBox();
			label1 = new Label();
			label2 = new Label();
			label3 = new Label();
			cmbCommand = new ComboBox();
			button3 = new Button();
			button4 = new Button();
			SuspendLayout();
			// 
			// button1
			// 
			button1.Location = new Point(635, 30);
			button1.Name = "button1";
			button1.Size = new Size(122, 29);
			button1.TabIndex = 0;
			button1.Text = "Send raw data";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// textBox1
			// 
			textBox1.Location = new Point(31, 30);
			textBox1.Name = "textBox1";
			textBox1.Size = new Size(587, 23);
			textBox1.TabIndex = 1;
			// 
			// button2
			// 
			button2.Location = new Point(635, 68);
			button2.Name = "button2";
			button2.Size = new Size(122, 27);
			button2.TabIndex = 3;
			button2.Text = "Set motors";
			button2.UseVisualStyleBackColor = true;
			button2.Click += button2_Click;
			// 
			// txtOutput
			// 
			txtOutput.Location = new Point(31, 186);
			txtOutput.Multiline = true;
			txtOutput.Name = "txtOutput";
			txtOutput.Size = new Size(512, 252);
			txtOutput.TabIndex = 6;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(31, 159);
			label1.Name = "label1";
			label1.Size = new Size(45, 15);
			label1.TabIndex = 7;
			label1.Text = "Output";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(31, 12);
			label2.Name = "label2";
			label2.Size = new Size(55, 15);
			label2.TabIndex = 8;
			label2.Text = "Raw data";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new Point(31, 68);
			label3.Name = "label3";
			label3.Size = new Size(64, 15);
			label3.TabIndex = 9;
			label3.Text = "Command";
			// 
			// cmbCommand
			// 
			cmbCommand.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbCommand.FormattingEnabled = true;
			cmbCommand.Items.AddRange(new object[] { "MSP_STATUS (101)", "MSP_RAW_IMU (102)" });
			cmbCommand.Location = new Point(28, 93);
			cmbCommand.Name = "cmbCommand";
			cmbCommand.Size = new Size(195, 23);
			cmbCommand.TabIndex = 10;
			// 
			// button3
			// 
			button3.Location = new Point(253, 90);
			button3.Name = "button3";
			button3.Size = new Size(122, 27);
			button3.TabIndex = 11;
			button3.Text = "Send command";
			button3.UseVisualStyleBackColor = true;
			button3.Click += button3_Click_1;
			// 
			// button4
			// 
			button4.Location = new Point(635, 164);
			button4.Name = "button4";
			button4.Size = new Size(139, 64);
			button4.TabIndex = 12;
			button4.Text = "button4";
			button4.UseVisualStyleBackColor = true;
			button4.Click += button4_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(button4);
			Controls.Add(button3);
			Controls.Add(cmbCommand);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(txtOutput);
			Controls.Add(button2);
			Controls.Add(textBox1);
			Controls.Add(button1);
			Name = "Form1";
			Text = "Form1";
			FormClosing += Form1_FormClosing;
			Load += Form1_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button button1;
		private TextBox textBox1;
		private Button button2;
		private TextBox txtOutput;
		private Label label1;
		private Label label2;
		private Label label3;
		private ComboBox cmbCommand;
		private Button button3;
		private Button button4;
	}
}