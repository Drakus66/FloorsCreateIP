using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STP.FloorsCreateIP
{
    public partial class LvlSelectForm : Form
    {
        public List<string> SelectedLevels
        {  get; }

        public LvlSelectForm(List<string> lvlNames)
        {
            InitializeComponent();
        }
    }
}
