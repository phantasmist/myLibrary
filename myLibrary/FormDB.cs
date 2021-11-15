using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace myLibrary
{
    public partial class FormDB : Form
    {
        DataTable dt = null;
        public FormDB(DataTable table)
        {
            InitializeComponent();
            dt = table;
        }

        private void drawDB()
        {
            try
            {
                dbGrid.DataSource = dt;
                //FormDB.Text = 

                // 소팅 기능 비활성화, 항상 addDate() 다음에 실행되기 때문에 걱정X
                //foreach (DataGridViewColumn column in dbGrid.Columns)
                //{
                    //column.SortMode = DataGridViewColumnSortMode.NotSortable;
                //}
            }
            catch (Exception e1)
            {
                return;
            }
        }

        private void FormDB_Load(object sender, EventArgs e)
        {
            drawDB();
        }
    }
}
