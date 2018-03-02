using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HttpExhaustiver
{
    public partial class AddDicForm : Form
    {

        private Int32 modifyIndex;
        private MainForm parentForm;
        public AddDicForm(MainForm parentForm, Int32 modifyIndex)
        {
            InitializeComponent();
            this.parentForm = parentForm;
            this.modifyIndex = modifyIndex;
            if(modifyIndex>-1){
                textBox2.Text = parentForm.DicsListview.Items[modifyIndex].SubItems[1].Text;
                textBox1.Text = parentForm.DicsListview.Items[modifyIndex].SubItems[0].Text;
            }
        }

        private void AddDicForm_Load(object sender, EventArgs e)
        {
            this.Icon = MainForm.icon;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox2.Text) || String.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("输入内容有误");
                return;
            }
            if (!File.Exists(textBox2.Text))
            {
                MessageBox.Show("文件不存在");
                return;
            }
            if (parentForm != null)
            {
                parentForm.addOrModifyDic(textBox1.Text, textBox2.Text, modifyIndex);
            }
            
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = ofd.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
