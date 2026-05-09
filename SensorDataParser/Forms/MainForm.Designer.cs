namespace SensorDataParser
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            mainButton = new Button();
            btnOpenAnalysis = new Button();
            SuspendLayout();
            // 
            // mainButton
            // 
            mainButton.Dock = DockStyle.Top;
            mainButton.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point);
            mainButton.Location = new Point(0, 0);
            mainButton.Margin = new Padding(3, 4, 3, 4);
            mainButton.Name = "mainButton";
            mainButton.Size = new Size(914, 290);
            mainButton.TabIndex = 0;
            mainButton.Text = "Выбрать файл(ы) для загрузки в базу данных";
            mainButton.UseVisualStyleBackColor = true;
            mainButton.Click += CultivationFilesButtonClick;
            // 
            // btnOpenAnalysis
            // 
            btnOpenAnalysis.Dock = DockStyle.Top;
            btnOpenAnalysis.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point);
            btnOpenAnalysis.Location = new Point(0, 290);
            btnOpenAnalysis.Margin = new Padding(3, 4, 3, 4);
            btnOpenAnalysis.Name = "btnOpenAnalysis";
            btnOpenAnalysis.Size = new Size(914, 290);
            btnOpenAnalysis.TabIndex = 1;
            btnOpenAnalysis.Text = "Анализ данных (Графики)";
            btnOpenAnalysis.UseVisualStyleBackColor = true;
            btnOpenAnalysis.Click += btnOpenAnalysis_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(914, 600);
            Controls.Add(btnOpenAnalysis);
            Controls.Add(mainButton);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            Text = "Обрабочик xml-документов";
            Load += MainFormLoad;
            ResumeLayout(false);
        }

        #endregion

        private Button mainButton;
        private Button btnOpenAnalysis;
    }
}