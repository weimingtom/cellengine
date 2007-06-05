﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Runtime.Serialization.Formatters;
using System.IO;

namespace CellGameEdit.PM
{
    [Serializable]
    public partial class CommandForm : Form, ISerializable
    {
        public string id;

        public CommandForm(String name)
        {
            InitializeComponent();

            id = name;
            this.Text = id;
            
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected CommandForm(SerializationInfo info, StreamingContext context)
        {
            InitializeComponent();

            try
            {

                id = (String)info.GetValue("id", typeof(String));
                this.Text = id;

                ArrayList heads = (ArrayList)info.GetValue("heads", typeof(ArrayList));
                ArrayList cells = (ArrayList)info.GetValue("cells", typeof(ArrayList));
                int ColumnsCount = (int)info.GetValue("ColumnsCount", typeof(int));
                int RowsCount = (int)info.GetValue("RowsCount", typeof(int));

                for (int c = 0; c < ColumnsCount; c++)
                {
                    string head = (String)heads[c];
                    dataGridView1.Columns.Add(head, head);
                }

                for (int r = 0; r < RowsCount; r++)
                {
                    string[] row = new string[ColumnsCount];
                    
                    for (int c = 0; c < ColumnsCount; c++)
                    {
                        int i = r * ColumnsCount + c;
                        row[c] = (String)cells[i];
                    }

                    dataGridView1.Rows.Add(row);
                }
                

            }catch(Exception err){
          
            }
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", id);

            ArrayList heads = new ArrayList();
            ArrayList cells = new ArrayList();
            int ColumnsCount = this.dataGridView1.Columns.Count;
            int RowsCount = this.dataGridView1.Rows.Count - 1;

            for (int c = 0; c < ColumnsCount; c++)
            {
                string head = getHeadText(c);
                heads.Add(head);
            }

            for (int r = 0; r < RowsCount; r++)
            {
                for (int c = 0; c < this.dataGridView1.Columns.Count; c++)
                {
                    int i = r * this.dataGridView1.Columns.Count + c;
                    string cell = getCellText(r,c);
                    if (cell == null) cell = "";
                    cells.Add(cell);
                }
            }

            info.AddValue("heads",heads);
            info.AddValue("cells",cells);
            info.AddValue("ColumnsCount", ColumnsCount);
            info.AddValue("RowsCount", RowsCount);

        }

        public void OutputCustom(int index, String script, System.IO.StringWriter output)
        {
            lock (this)
            {
                int ColumnsCount = this.dataGridView1.Columns.Count;
                int RowsCount = this.dataGridView1.Rows.Count - 1;

                String[][] cell_matrix_c_r = new string[ColumnsCount][];
                for (int c = 0; c < ColumnsCount; c++)
                {
                    cell_matrix_c_r[c] = new string[RowsCount];
                    for (int r = 0; r < RowsCount; r++)
                    {
                        cell_matrix_c_r[c][r] = getCellText(r, c);
                    }
                }

                String[][] cell_matrix_r_c = new string[RowsCount][];
                for (int r = 0; r < RowsCount; r++)
                {
                    cell_matrix_r_c[r] = new string[ColumnsCount];
                    for (int c = 0; c < ColumnsCount; c++)
                    {
                        cell_matrix_r_c[r][c] = getCellText(r, c);
                    }
                }


                try
                {
                    String table = Util.getFullTrunkScript(script, "#<TABLE>", "#<END TABLE>");

                    bool fix = false;

                    // column heads
                    do
                    {

                        String[] heads = new string[ColumnsCount];
                        
                        for (int c = 0; c < heads.Length; c++)
                        {
                            string TEXT = getHeadText(c);

                            heads[c] = Util.replaceKeywordsScript(table, "#<COLUMN HEAD>", "#<END COLUMN HEAD>",
                                new string[] { "<INDEX>", "<TEXT>"},
                                new string[] { c.ToString(), TEXT}
                                );
                        }
                        string temp = Util.replaceSubTrunksScript(table, "#<COLUMN HEAD>", "#<END COLUMN HEAD>", heads);
                        if (temp == null)
                        {
                            fix = false;
                        }
                        else
                        {
                            fix = true;
                            table = temp;
                        }
                    } while (fix);

                    // cells
                    do
                    {
                        String[] cells = new string[ColumnsCount * RowsCount];
                        for (int r = 0; r < RowsCount; r++)
                        {
                            for (int c = 0; c < ColumnsCount; c++)
                            {
                                int i = r * ColumnsCount + c;

                                string TEXT = getCellText(r, c);
                                if (TEXT == null) TEXT = "";
                                cells[i] = Util.replaceKeywordsScript(table, "#<CELL>", "#<END CELL>",
                                    new string[] { "<COLUMN INDEX>", "<ROW INDEX>", "<TEXT>" },
                                    new string[] { c.ToString(), r.ToString(), TEXT }
                                    );
                            }
                           
                        }
                        string temp = Util.replaceSubTrunksScript(table, "#<CELL>", "#<END CELL>", cells);
                        if (temp == null)
                        {
                            fix = false;
                        }
                        else
                        {
                            fix = true;
                            table = temp;
                        }
                    } while (fix);

                    // rows
                    //#<ROWS>                /* 表行数据 开始 横向输出 */
                    //    <INDEX>                /* (int)行 号码 */
                    //    <ARRAY>                /* (obj)[]行 数据*/
                    //    <ARRAY STR>            /* (str)[]行 数据*/
                    //    <ARRAY NUM>            /* (int)[]行 数据*/
                    //    <ARRAY SMART>          /* (ato)[]行 数据*/
                    //#<END ROWS>            /* 表行数据 结束*/
                    do
                    {
                        String[] rows = new string[RowsCount];
                        for (int r = 0; r < RowsCount; r++)
                        {
                            string ARRAY = Util.toArray1D(ref cell_matrix_r_c[r]);
                            string ARRAY_NUM = Util.toNumberArray1D(ref cell_matrix_r_c[r]);
                            string ARRAY_STR = Util.toStringArray1D(ref cell_matrix_r_c[r]);
                            string ARRAY_SMART = Util.toSmartArray1D(ref cell_matrix_r_c[r]);

                            rows[r] = Util.replaceKeywordsScript(table, "#<ROWS>", "#<END ROWS>",
                                new string[] { "<INDEX>", "<ARRAY>", "<ARRAY STR>", "<ARRAY NUM>", "<ARRAY SMART>" },
                                new string[] { r.ToString(), ARRAY, ARRAY_STR, ARRAY_NUM, ARRAY_SMART }
                                );
                        }
                        string temp = Util.replaceSubTrunksScript(table, "#<ROWS>", "#<END ROWS>", rows);
                        if (temp == null)
                        {
                            fix = false;
                        }
                        else
                        {
                            fix = true;
                            table = temp;
                        }
                    } while (fix);


                    // columns
                    //#<COLUMNS>             /* 表列数据 开始 纵向输出 */
                    //    <INDEX>                /* (int)列 号码 */
                    //    <ARRAY>                /* (obj)[]列 数据*/
                    //    <ARRAY STR>            /* (str)[]列 数据*/
                    //    <ARRAY NUM>            /* (int)[]列 数据*/
                    //    <ARRAY SMART>          /* (ato)[]列 数据*/
                    //#<END COLUMNS>         /* 表列数据 结束*/
                    do
                    {
                        String[] columns = new string[ColumnsCount];
                        for (int c = 0; c < ColumnsCount; c++)
                        {
                            string ARRAY = Util.toArray1D(ref cell_matrix_c_r[c]);
                            string ARRAY_NUM = Util.toNumberArray1D(ref cell_matrix_c_r[c]);
                            string ARRAY_STR = Util.toStringArray1D(ref cell_matrix_c_r[c]);
                            string ARRAY_SMART = Util.toSmartArray1D(ref cell_matrix_c_r[c]);

                            columns[c] = Util.replaceKeywordsScript(table, "#<COLUMNS>", "#<END COLUMNS>",
                                new string[] { "<INDEX>", "<ARRAY>", "<ARRAY STR>", "<ARRAY NUM>", "<ARRAY SMART>" },
                                new string[] { c.ToString(), ARRAY, ARRAY_STR, ARRAY_NUM, ARRAY_SMART }
                                );
                        }
                        string temp = Util.replaceSubTrunksScript(table, "#<COLUMNS>", "#<END COLUMNS>", columns);
                        if (temp == null)
                        {
                            fix = false;
                        }
                        else
                        {
                            fix = true;
                            table = temp;
                        }
                    } while (fix);
                   



                    //matrix
                    string matrix = Util.toArray2D(ref cell_matrix_r_c);
                    string strMatrix = Util.toStringArray2D(ref cell_matrix_r_c);
                    string numMatrix = Util.toNumberArray2D(ref cell_matrix_r_c);
                    string smartMatrix = Util.toSmartArray2D(ref cell_matrix_r_c);

                    table = Util.replaceKeywordsScript(table, "#<TABLE>", "#<END TABLE>",
                        new string[] { 
                            "<NAME>", 
                            "<TABLE INDEX>",
                            "<COLUMN COUNT>",
                            "<ROW COUNT>",
                            "<TABLE MATRIX> ",
                            "<TABLE MATRIX STR>",         /*(str)[][] 表文字二维数组 */
                            "<TABLE MATRIX NUM>",         /*(str)[][] 表数字二维数组 */
                            "<TABLE MATRIX SMART>",       /*(str)[][] 表自动二维数组 */
                        },
                        new string[] { 
                            this.id, 
                            index.ToString(),
                            ColumnsCount.ToString(),
                            RowsCount.ToString(),
                            matrix,
                            strMatrix,
                            numMatrix,
                            smartMatrix,
                        }
                     );

                    output.WriteLine(table);
                    //Console.WriteLine(map);
                }
                catch (Exception err) { Console.WriteLine(this.id + " : " + err.StackTrace + "  at  " + err.Message); }

            }
        }

        private void CommandForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private string getCellText(int r,int c)
        {   
            string text = "";
            try
            {
                int dc = c;
                if (dataGridView1.Columns[c].DisplayIndex != c)
                {
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        if (column.DisplayIndex == c)
                        {
                            dc = dataGridView1.Columns.IndexOf(column);
                            break;
                        }
                    }
                }
               text  = dataGridView1.Rows[r].Cells[dc].Value.ToString();
            }
            catch (Exception err) { 
                text = ""; 
            }
            return text;
        }

        private string getHeadText(int c)
        {
            string text = "";
            try
            {
                int dc = c;
                if (dataGridView1.Columns[c].DisplayIndex != c)
                {
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        if (column.DisplayIndex == c)
                        {
                            dc = dataGridView1.Columns.IndexOf(column);
                            break;
                        }
                    }
                }
                text = dataGridView1.Columns[dc].HeaderText;
            }
            catch (Exception err)
            {
                text = "";
            }
            return text;
        }

//----------------------------------------------------------------------------------------------------------------------------------------------------------


        private void 属性ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PropertyEdit edit = new PropertyEdit(this.dataGridView1);
            edit.MdiParent = this.MdiParent;
            edit.Show();
        }

        // add column
        private void 文本ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String name = "ColumnName";
            TextDialog nameDialog = new TextDialog(name);
            if (nameDialog.ShowDialog() == DialogResult.OK)
            {
                this.dataGridView1.Columns.Add(nameDialog.getText(), nameDialog.getText());
            }

            
        }

        private void dataGridView1_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {

        }

        // delete column
        int PopedColumnIndex = -1;
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                PopedColumnIndex = e.ColumnIndex;
                columnMenu.Show(
                    dataGridView1,
                    e.Location.X + dataGridView1.GetColumnDisplayRectangle(e.ColumnIndex, false).Location.X,
                    e.Location.Y + dataGridView1.GetColumnDisplayRectangle(e.ColumnIndex, false).Location.Y
                    );
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Columns.RemoveAt(PopedColumnIndex);
            }
            catch (Exception err) { }
        }

        private void 重命名列ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                String name = dataGridView1.Columns[PopedColumnIndex].HeaderText;
                TextDialog nameDialog = new TextDialog(name);
                if (nameDialog.ShowDialog() == DialogResult.OK)
                {
                    dataGridView1.Columns[PopedColumnIndex].HeaderText = nameDialog.getText();
                    dataGridView1.Columns[PopedColumnIndex].Name = nameDialog.getText();
                }
                
            }
            catch (Exception err) { }
        }


        // delete row
        int PopedRowIndex = -1;
        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                PopedRowIndex = e.RowIndex;
                rowMenu.Show(
                    dataGridView1,
                    e.Location.X + dataGridView1.GetRowDisplayRectangle(e.RowIndex, false).Location.X,
                    e.Location.Y + dataGridView1.GetRowDisplayRectangle(e.RowIndex, false).Location.Y
                    );
            }
        }

        private void 删除行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Rows.RemoveAt(PopedRowIndex);
            }
            catch (Exception err) { }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dataLable.Text = "行=" + e.RowIndex;
            try
            {
                dataLable.Text += " 列=" + dataGridView1.Columns[e.ColumnIndex].DisplayIndex;
            }
            catch (Exception err) {
            }
        }

        private void 表属性ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PropertyEdit edit = new PropertyEdit(this.dataGridView1);
            edit.MdiParent = this.MdiParent;
            edit.Show();
        }



        private void dataGridView1_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
        {
            Console.WriteLine("ColumnIndex = "+e.Column.DisplayIndex);

            //if (dataGridView1.Columns[e.Column.DisplayIndex] != e.Column)
            //{
            //    DataGridViewColumn d = dataGridView1.Columns[e.Column.DisplayIndex];
            //    DataGridViewColumn s = e.Column;
            //    int src = dataGridView1.Columns.IndexOf(s);
            //    int dst = dataGridView1.Columns.IndexOf(d);
                
            //    dataGridView1.Columns.RemoveAt(dst);
            //    dataGridView1.Columns.Insert(dst,s);

            //    dataGridView1.Columns.RemoveAt(src);
            //    dataGridView1.Columns.Insert(src, d);

            //    //dataGridView1.Columns[e.Column.DisplayIndex] = e.Column;
            //    //dataGridView1.Columns[temp.DisplayIndex] = temp;

            //}

        }


     



 


    }
}