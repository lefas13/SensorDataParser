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
            SuspendLayout();
            // 
            // mainButton
            // 
            mainButton.Dock = DockStyle.Fill;
            mainButton.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point);
            mainButton.Location = new Point(0, 0);
            mainButton.Name = "mainButton";
            mainButton.Size = new Size(800, 450);
            mainButton.TabIndex = 0;
            mainButton.Text = "Выбрать файл(ы) для загрузки в базу данных";
            mainButton.UseVisualStyleBackColor = true;
            mainButton.Click += CultivationFilesButtonClick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(mainButton);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "Обрабочик xml-документов";
            Load += MainFormLoad;
            ResumeLayout(false);
        }

        #endregion

        private Button mainButton;
    }
}