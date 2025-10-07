// File: ArcadiaLauncherForm.cs
// Namespace: Arcadia.UI.Forms
// NOTE: Ensure your project references System.Windows.Forms assembly

using System.Windows.Forms;

namespace Arcadia.UI.Forms
{
    public class ArcadiaLauncherForm : Form
    {
        public ArcadiaLauncherForm()
        {
            Text = "Arcadia Launcher";
            Width = 800;
            Height = 600;

            var label = new Label
            {
                Text = "Welcome to Arcadia",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 24)
            };

            Controls.Add(label);
        }
    }
}