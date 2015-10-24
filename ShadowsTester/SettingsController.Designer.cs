namespace ShadowsTester
{
    partial class SettingsController
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
            if(disposing && (components != null))
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
            this.label1 = new System.Windows.Forms.Label();
            this.aoRangeBar = new System.Windows.Forms.TrackBar();
            this.aoStrengthBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.ambientLightBar = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.indirectLightBar = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.aoGlobalBar = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.aoCutoffBar = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.vdaoMultBar = new System.Windows.Forms.TrackBar();
            this.label7 = new System.Windows.Forms.Label();
            this.vdaoSamplingBar = new System.Windows.Forms.TrackBar();
            this.label8 = new System.Windows.Forms.Label();
            this.vdaoRefractionBar = new System.Windows.Forms.TrackBar();
            this.label9 = new System.Windows.Forms.Label();
            this.subScatBar = new System.Windows.Forms.TrackBar();
            this.label10 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.selectedMeshLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.aoRangeBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aoStrengthBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ambientLightBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.indirectLightBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aoGlobalBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aoCutoffBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.vdaoMultBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.vdaoSamplingBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.vdaoRefractionBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.subScatBar)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 45);
            this.label1.TabIndex = 0;
            this.label1.Text = "AO range";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // aoRangeBar
            // 
            this.aoRangeBar.Location = new System.Drawing.Point(132, 25);
            this.aoRangeBar.Maximum = 100;
            this.aoRangeBar.Name = "aoRangeBar";
            this.aoRangeBar.Size = new System.Drawing.Size(384, 45);
            this.aoRangeBar.TabIndex = 1;
            this.aoRangeBar.Value = 50;
            this.aoRangeBar.Scroll += new System.EventHandler(this.aoRangeBar_Scroll);
            // 
            // aoStrengthBar
            // 
            this.aoStrengthBar.Location = new System.Drawing.Point(132, 76);
            this.aoStrengthBar.Maximum = 100;
            this.aoStrengthBar.Name = "aoStrengthBar";
            this.aoStrengthBar.Size = new System.Drawing.Size(384, 45);
            this.aoStrengthBar.TabIndex = 3;
            this.aoStrengthBar.Value = 50;
            this.aoStrengthBar.Scroll += new System.EventHandler(this.aoStrengthBar_Scroll);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 45);
            this.label2.TabIndex = 2;
            this.label2.Text = "AO strength";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ambientLightBar
            // 
            this.ambientLightBar.Location = new System.Drawing.Point(132, 16);
            this.ambientLightBar.Maximum = 100;
            this.ambientLightBar.Name = "ambientLightBar";
            this.ambientLightBar.Size = new System.Drawing.Size(384, 45);
            this.ambientLightBar.TabIndex = 5;
            this.ambientLightBar.Value = 50;
            this.ambientLightBar.Scroll += new System.EventHandler(this.ambientLightBar_Scroll);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 45);
            this.label3.TabIndex = 4;
            this.label3.Text = "Ambient light";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // indirectLightBar
            // 
            this.indirectLightBar.Location = new System.Drawing.Point(132, 67);
            this.indirectLightBar.Maximum = 100;
            this.indirectLightBar.Name = "indirectLightBar";
            this.indirectLightBar.Size = new System.Drawing.Size(384, 45);
            this.indirectLightBar.TabIndex = 7;
            this.indirectLightBar.Value = 50;
            this.indirectLightBar.Scroll += new System.EventHandler(this.indirectLightBar_Scroll);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(6, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 45);
            this.label4.TabIndex = 6;
            this.label4.Text = "Indirect light";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // aoGlobalBar
            // 
            this.aoGlobalBar.Location = new System.Drawing.Point(132, 118);
            this.aoGlobalBar.Maximum = 100;
            this.aoGlobalBar.Name = "aoGlobalBar";
            this.aoGlobalBar.Size = new System.Drawing.Size(384, 45);
            this.aoGlobalBar.TabIndex = 9;
            this.aoGlobalBar.Value = 50;
            this.aoGlobalBar.Scroll += new System.EventHandler(this.aoGlobalBar_Scroll);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(6, 118);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(120, 45);
            this.label5.TabIndex = 8;
            this.label5.Text = "AO global";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // aoCutoffBar
            // 
            this.aoCutoffBar.Location = new System.Drawing.Point(132, 166);
            this.aoCutoffBar.Maximum = 100;
            this.aoCutoffBar.Name = "aoCutoffBar";
            this.aoCutoffBar.Size = new System.Drawing.Size(384, 45);
            this.aoCutoffBar.TabIndex = 11;
            this.aoCutoffBar.Scroll += new System.EventHandler(this.aoCutoffBar_Scroll);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(6, 166);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 45);
            this.label6.TabIndex = 10;
            this.label6.Text = "AO cutoff";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // vdaoMultBar
            // 
            this.vdaoMultBar.Location = new System.Drawing.Point(132, 217);
            this.vdaoMultBar.Maximum = 100;
            this.vdaoMultBar.Name = "vdaoMultBar";
            this.vdaoMultBar.Size = new System.Drawing.Size(384, 45);
            this.vdaoMultBar.TabIndex = 13;
            this.vdaoMultBar.Value = 50;
            this.vdaoMultBar.Scroll += new System.EventHandler(this.vdaoMultBar_Scroll);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(6, 217);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(120, 45);
            this.label7.TabIndex = 12;
            this.label7.Text = "VDAO multiplier";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // vdaoSamplingBar
            // 
            this.vdaoSamplingBar.Location = new System.Drawing.Point(132, 268);
            this.vdaoSamplingBar.Maximum = 100;
            this.vdaoSamplingBar.Name = "vdaoSamplingBar";
            this.vdaoSamplingBar.Size = new System.Drawing.Size(384, 45);
            this.vdaoSamplingBar.TabIndex = 15;
            this.vdaoSamplingBar.Value = 50;
            this.vdaoSamplingBar.Scroll += new System.EventHandler(this.vdaoSamplingBar_Scroll);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(6, 268);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(120, 45);
            this.label8.TabIndex = 14;
            this.label8.Text = "VDAO sampling";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // vdaoRefractionBar
            // 
            this.vdaoRefractionBar.Location = new System.Drawing.Point(132, 319);
            this.vdaoRefractionBar.Maximum = 100;
            this.vdaoRefractionBar.Name = "vdaoRefractionBar";
            this.vdaoRefractionBar.Size = new System.Drawing.Size(384, 45);
            this.vdaoRefractionBar.TabIndex = 17;
            this.vdaoRefractionBar.Scroll += new System.EventHandler(this.vdaoRefractionBar_Scroll);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(6, 319);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(120, 45);
            this.label9.TabIndex = 16;
            this.label9.Text = "VDAO refraction";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // subScatBar
            // 
            this.subScatBar.Location = new System.Drawing.Point(132, 121);
            this.subScatBar.Maximum = 100;
            this.subScatBar.Name = "subScatBar";
            this.subScatBar.Size = new System.Drawing.Size(384, 45);
            this.subScatBar.TabIndex = 19;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(6, 121);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(120, 45);
            this.label10.TabIndex = 18;
            this.label10.Text = "Subsurface scattering";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectedMeshLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 575);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(571, 22);
            this.statusStrip1.TabIndex = 20;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // selectedMeshLabel
            // 
            this.selectedMeshLabel.Name = "selectedMeshLabel";
            this.selectedMeshLabel.Size = new System.Drawing.Size(101, 17);
            this.selectedMeshLabel.Text = "No mesh selected";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.aoRangeBar);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.subScatBar);
            this.groupBox1.Controls.Add(this.vdaoRefractionBar);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.aoStrengthBar);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.vdaoSamplingBar);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.vdaoMultBar);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.aoCutoffBar);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Location = new System.Drawing.Point(15, 186);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(537, 381);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mesh properties";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.indirectLightBar);
            this.groupBox2.Controls.Add(this.ambientLightBar);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.aoGlobalBar);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(540, 166);
            this.groupBox2.TabIndex = 22;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "World properties";
            // 
            // SettingsController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 597);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusStrip1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SettingsController";
            this.Opacity = 0.75D;
            this.Text = "SettingsController";
            ((System.ComponentModel.ISupportInitialize)(this.aoRangeBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aoStrengthBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ambientLightBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.indirectLightBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aoGlobalBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aoCutoffBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.vdaoMultBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.vdaoSamplingBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.vdaoRefractionBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.subScatBar)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar aoRangeBar;
        private System.Windows.Forms.TrackBar aoStrengthBar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar ambientLightBar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar indirectLightBar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar aoGlobalBar;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar aoCutoffBar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TrackBar vdaoMultBar;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TrackBar vdaoSamplingBar;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TrackBar vdaoRefractionBar;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TrackBar subScatBar;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel selectedMeshLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}