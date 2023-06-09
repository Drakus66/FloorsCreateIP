namespace STP.FloorsCreateIP
{
    partial class LvlSelectForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOk = new System.Windows.Forms.Button();
            this.btAll = new System.Windows.Forms.Button();
            this.btNothing = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(12, 12);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(276, 409);
            this.checkedListBox1.TabIndex = 0;
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(213, 463);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 1;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(132, 463);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 23);
            this.btOk.TabIndex = 2;
            this.btOk.Text = "Создать";
            this.btOk.UseVisualStyleBackColor = true;
            // 
            // btAll
            // 
            this.btAll.Location = new System.Drawing.Point(13, 428);
            this.btAll.Name = "btAll";
            this.btAll.Size = new System.Drawing.Size(55, 23);
            this.btAll.TabIndex = 3;
            this.btAll.Text = "Все";
            this.btAll.UseVisualStyleBackColor = true;
            // 
            // btNothing
            // 
            this.btNothing.Location = new System.Drawing.Point(74, 428);
            this.btNothing.Name = "btNothing";
            this.btNothing.Size = new System.Drawing.Size(55, 23);
            this.btNothing.TabIndex = 4;
            this.btNothing.Text = "Ничего";
            this.btNothing.UseVisualStyleBackColor = true;
            // 
            // LvlSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 498);
            this.Controls.Add(this.btNothing);
            this.Controls.Add(this.btAll);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.checkedListBox1);
            this.Name = "LvlSelectForm";
            this.Text = "Выберите уровни";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btAll;
        private System.Windows.Forms.Button btNothing;
    }
}