using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FloorsCreateIP
{
    public partial class LvlSelectForm : Form
    {
        public List<string> SelectedLevels
        {  
            get; 
            private set;
        }

        public bool okStart
        {
            get;
            private set;
        }

        public LvlSelectForm(List<string> lvlNames)
        {
            InitializeComponent();
            okStart = false;
            
            foreach (string lvlName in lvlNames) 
            {
                lbLevelNames.Items.Add(lvlName);
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            okStart = true;
            SelectedLevels = lbLevelNames.CheckedItems.Cast<string>().ToList();
            this.Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            okStart = false;
            SelectedLevels = null;
            this.Close();
        }

        private void btAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbLevelNames.Items.Count; i++) 
            {
                lbLevelNames.SetItemCheckState(i, CheckState.Checked);
            }
        }

        private void btNothing_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbLevelNames.Items.Count; i++)
            {
                lbLevelNames.SetItemCheckState(i, CheckState.Unchecked);
            }
        }
    }
}
