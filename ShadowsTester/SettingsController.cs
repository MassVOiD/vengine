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

namespace ShadowsTester
{
    public partial class SettingsController : Form
    {
        public static SettingsController Instance;
        public SettingsController()
        {
            Instance = this;
            InitializeComponent();
           // this.Show();
        }

        private Mesh3d SelectedMesh = null;

        public void SetMesh(Mesh3d mesh)
        {
            SelectedMesh = mesh;
            this.Invoke(new Action(() =>
            {
                aoRangeBar.Value = (int)(mesh.MainMaterial.AORange * (float)aoRangeBar.Maximum);
                aoStrengthBar.Value = (int)(mesh.MainMaterial.AOStrength * (float)aoStrengthBar.Maximum);
                aoCutoffBar.Value = (int)(mesh.MainMaterial.AOAngleCutoff * (float)aoCutoffBar.Maximum);
                selectedMeshLabel.Text = mesh.Name == null ? "Unnamed mesh" : mesh.Name;
            }));
        }

        private void aoRangeBar_Scroll(object sender, EventArgs e)
        {
            if(SelectedMesh == null)
                return;
            SelectedMesh.MainMaterial.AORange = ((float)aoRangeBar.Value / (float)aoRangeBar.Maximum);
        }

        private void aoStrengthBar_Scroll(object sender, EventArgs e)
        {
            if(SelectedMesh == null)
                return;
            SelectedMesh.MainMaterial.AOStrength = ((float)aoStrengthBar.Value / (float)aoStrengthBar.Maximum);
        }

        private void aoCutoffBar_Scroll(object sender, EventArgs e)
        {
            if(SelectedMesh == null)
                return;
            SelectedMesh.MainMaterial.AOAngleCutoff = ((float)aoCutoffBar.Value / (float)aoCutoffBar.Maximum);
        }

        private void vdaoMultBar_Scroll(object sender, EventArgs e)
        {
            if(SelectedMesh == null)
                return;
            SelectedMesh.MainMaterial.VDAOMultiplier = ((float)vdaoMultBar.Value / (float)vdaoMultBar.Maximum);
        }

        private void vdaoSamplingBar_Scroll(object sender, EventArgs e)
        {
            if(SelectedMesh == null)
                return;
            SelectedMesh.MainMaterial.VDAOSamplingMultiplier = ((float)vdaoSamplingBar.Value / (float)vdaoSamplingBar.Maximum);
        }

        private void vdaoRefractionBar_Scroll(object sender, EventArgs e)
        {
            if(SelectedMesh == null)
                return;
            SelectedMesh.MainMaterial.VDAORefreactionMultiplier = ((float)vdaoRefractionBar.Value / (float)vdaoRefractionBar.Maximum);
        }
    }
}
