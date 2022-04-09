using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PasteIntoFile.Properties;

namespace PasteIntoFile {
    public partial class TemplateEdit : MasterForm {
        public TemplateEdit() {
            InitializeComponent();

            foreach (Control element in GetAllChild(this)) {
                // ReSharper disable once UnusedVariable (to convince IDE that these resource strings are actually used)
                string[] usedResourceStrings = { Resources.str_filename_template, Resources.str_accept, Resources.str_cancel, Resources.str_template_edit_info };
                element.Text = Resources.ResourceManager.GetString(element.Text) ?? element.Text;
            }

            Icon = Resources.icon;
            Text = Resources.str_edit_template;

            // setup link in info text
            var linkStart = labelInfo.Text.IndexOf(@"<a>", StringComparison.Ordinal);
            var linkEnd = labelInfo.Text.IndexOf(@"</a>", StringComparison.Ordinal);
            if (linkStart >= 0 && linkEnd > linkStart) {
                labelInfo.Text = labelInfo.Text.Remove(linkStart, 3).Remove(linkEnd - 3, 4);
                labelInfo.LinkArea = new LinkArea(linkStart, linkEnd - linkStart - 3);
            }

            // setup predefined templates
            textTemplate.Items.AddRange(new object[]{
                Settings.Default.filenameTemplate,
                "{0:yyyy-MM-dd HH-mm-ss}",
                "{0:yyyyMMdd_HHmmss}",
                "{0:yyyy-MM-dd}_{1:000}",
                "PasteIntoFile_{1:000}_{0:fffffff}",
            });
            textTemplate.Text = Settings.Default.filenameTemplate;
        }


        private void buttonAccept_Click(object sender, EventArgs e) {
            Settings.Default.filenameTemplate = textTemplate.Text;
            Settings.Default.Save();
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void textTemplate_Update(object sender = null, EventArgs e = null) {
            try {
                labelPreview.Text = ((Dialog)Owner)?.formatFilenameTemplate(textTemplate.Text);
                labelPreview.ForeColor = SystemColors.ControlText;
                buttonAccept.Enabled = true;

                var i = labelPreview.Text.IndexOfAny(Path.GetInvalidFileNameChars());
                if (i >= 0)
                    throw new FormatException(string.Format(Resources.str_invalid_character, labelPreview.Text[i]));
            } catch (FormatException ex) {
                labelPreview.Text = ex.Message;
                labelPreview.ForeColor = Color.Red;
                buttonAccept.Enabled = false;
            }
        }

        private void labelInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start(Resources.str_template_edit_info_url);
        }

        private void TemplateEdit_Shown(object sender, EventArgs e) {
            // Auto size dialog height
            // All tableLayout rows are set to 'autosize' except for the last -> it's height is a measure for leftover space 
            Height -= tableLayoutPanel1.GetRowHeights()[tableLayoutPanel1.RowCount - 1];
            MinimumSize = Size;
        }

    }
}
