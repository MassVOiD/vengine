using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VEngine;
using VEngine.FileFormats;

namespace ObjToSceneConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.FileOk += (o, ex) => textBox1.Text = dialog.FileName;
            dialog.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.FileOk += (o, ex) => textBox2.Text = dialog.FileName;
            dialog.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                textBox3.Text = dialog.SelectedPath;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Media.SearchPath = "media";
            var meshes = Object3dInfo.LoadSceneFromObj(textBox1.Text, textBox2.Text);
            var s = GameScene.FromMesh3dList(meshes, textBox3.Text + "/", textBox4.Text);
            System.IO.File.WriteAllText(textBox3.Text + "/" + textBox5.Text + ".scene", s);
            button4.Text = "Done";
        }
    }
}
