namespace DemoAppWinForms;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TextBox textBoxProject;
    private System.Windows.Forms.Button buttonCapture;

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
        this.components = new System.ComponentModel.Container();
        this.textBoxProject = new System.Windows.Forms.TextBox();
        this.buttonCapture = new System.Windows.Forms.Button();
        // 
        // textBoxProject
        // 
        this.textBoxProject.Location = new System.Drawing.Point(30, 30);
        this.textBoxProject.Size = new System.Drawing.Size(200, 23);
        this.textBoxProject.Text = "PrimeGlobalPeople";
        // 
        // buttonCapture
        // 
        this.buttonCapture.Location = new System.Drawing.Point(250, 30);
        this.buttonCapture.Size = new System.Drawing.Size(120, 23);
        this.buttonCapture.Text = "Capture Screens";
        this.buttonCapture.Click += new System.EventHandler(this.buttonCapture_Click);
        // 
        // Form1
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.textBoxProject);
        this.Controls.Add(this.buttonCapture);
        this.Text = "Form1";
    }

    #endregion
}
