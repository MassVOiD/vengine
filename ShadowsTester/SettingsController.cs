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
                subScatBar.Value = (int)(mesh.MainMaterial.SubsurfaceScatteringMultiplier * (float)subScatBar.Maximum);
                vdaoMultBar.Value = (int)(mesh.MainMaterial.VDAOMultiplier * (float)vdaoMultBar.Maximum);
                vdaoSamplingBar.Value = (int)(mesh.MainMaterial.VDAOSamplingMultiplier * (float)vdaoSamplingBar.Maximum);
                vdaoRefractionBar.Value = (int)(mesh.MainMaterial.VDAORefreactionMultiplier * (float)vdaoRefractionBar.Maximum);
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

        private void ambientLightBar_Scroll(object sender, EventArgs e)
        {
            GLThread.DisplayAdapter.Pipeline.PostProcessor.VDAOGlobalMultiplier = ((float)ambientLightBar.Value / (float)ambientLightBar.Maximum);
        }

        private void indirectLightBar_Scroll(object sender, EventArgs e)
        {
            GLThread.DisplayAdapter.Pipeline.PostProcessor.RSMGlobalMultiplier = ((float)indirectLightBar.Value / (float)indirectLightBar.Maximum)*5.0f;
        }

        private void aoGlobalBar_Scroll(object sender, EventArgs e)
        {
            GLThread.DisplayAdapter.Pipeline.PostProcessor.AOGlobalModifier = 0.1f + ((float)aoGlobalBar.Value / (float)aoGlobalBar.Maximum) * 5.0f;
        }

        public void UpdatePerformance()
        {
            if(!this.Created)
                return;
            this.Invoke(new Action(() =>
            {
                deferredms.Text = GLThread.DisplayAdapter.Pipeline.PostProcessor.LastDeferredTime.ToString();
                ssaoms.Text = GLThread.DisplayAdapter.Pipeline.PostProcessor.LastSSAOTime.ToString();
                indirectms.Text = GLThread.DisplayAdapter.Pipeline.PostProcessor.LastIndirectTime.ToString();
                combinerms.Text = GLThread.DisplayAdapter.Pipeline.PostProcessor.LastCombinerTime.ToString();
                fogms.Text = GLThread.DisplayAdapter.Pipeline.PostProcessor.LastFogTime.ToString();
                mrtms.Text = GLThread.DisplayAdapter.Pipeline.PostProcessor.LastMRTTime.ToString();
                hdrms.Text = GLThread.DisplayAdapter.Pipeline.PostProcessor.LastHDRTime.ToString();
            }));
        }
    }
}
