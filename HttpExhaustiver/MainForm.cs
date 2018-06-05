using HttpExhaustiver.entity;
using HttpExhaustiver.handle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace HttpExhaustiver
{
    public partial class MainForm : Form
    {

        public static Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe");
        private static bool running = false;

        public MainForm()
        {
            InitializeComponent();
            init();

        }
        [STAThread]
        private void MainForm_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.Icon = icon;

        }

        private void init()
        {
            ConfigTimeOutCombox.SelectedIndex = 4;
            ConfigEncodeComboBox.SelectedIndex = 0;
            ConfigThreadNumComboBox.SelectedIndex = 0;
            ConfigVerificationTypeComboBox.SelectedIndex = 0;
            TestEncodeComboBox.SelectedIndex = 0;
            TestProtocolComboBox.SelectedIndex = 0;
            ConfigMethodComboBox.SelectedIndex = 0;
            ConfigProtocolComboBox.SelectedIndex = 0;
            ConfigCalcComboBox.SelectedIndex = 0;
            try
            {
                loadConfig();
            }
            catch { }
            textBox1.BackColor = Color.White;
            textBox1.ForeColor = Color.Black;
        }

        private void testSendThread()
        {
            TestSendButton.Enabled = false;
            TestResultTextBox.Text = "请求中......";
            try
            {
                String protocol = TestProtocolComboBox.Text.ToLower();
                Int32 port = Convert.ToInt32(TestPortTextBox.Text);
                string encode = TestEncodeComboBox.Text.Equals("自动") ? "UTF-8" : TestEncodeComboBox.Text;
                String context = TestSendTextBox.Text.Trim();
                context = context.Replace("\r\n", "\n");
                context = context.Replace("\n", "\r\n");
                byte[] data = Encoding.GetEncoding(encode).GetBytes(context);
                if (protocol.Equals("http"))
                {
                    HttpHandle.HttpResult result = new HttpHandle().httpSendData(TestHostTextBox.Text, port, 15000, encode, data);
                    TestResultTextBox.Text = "";
                    if (result != null)
                    {
                        TestResultTextBox.Text = result.Header + "\r\n" + result.Body;
                    }
                    return;
                }
                if (protocol.Equals("https"))
                {
                    HttpHandle.HttpResult result = new HttpHandle().httpsSendData(TestHostTextBox.Text, port, 15000, encode, data);
                    TestResultTextBox.Text = "";
                    if (result != null)
                    {
                        TestResultTextBox.Text = result.Header + "\r\n" + result.Body;
                    }
                    return;
                }
            }
            catch
            {
                TestResultTextBox.Text = "";
            }
            finally
            {
                TestSendButton.Enabled = true;
            }

        }
        [STAThread]
        private void TestSendButton_Click(object sender, EventArgs e)
        {
            TestSendTextBox_MouseLeave(sender, e);
            Thread testSendThreads = new Thread(testSendThread);
            testSendThreads.IsBackground = true;
            testSendThreads.Start();
        }
        [STAThread]
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            DialogResult r = MessageBox.Show("Really want to exit?", "Remind", MessageBoxButtons.YesNo);
            if (r == DialogResult.Yes)
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        private void TestProtocolComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TestProtocolComboBox.Text.ToLower().Equals("https"))
            {
                TestPortTextBox.Text = "443";
            }
            else
            {
                if (TestPortTextBox.Text.Trim().Equals("443"))
                {
                    TestPortTextBox.Text = "80";
                }
            }
        }

        private void TestSendTextBox_MouseLeave(object sender, EventArgs e)
        {
            //解析Header
            String context = TestSendTextBox.Text;
            context = context.Replace("\r\n", "\n");
            if (String.IsNullOrEmpty(context))
            {
                return;
            }
            String[] lines = context.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            lines = lines[0].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //解析请求方式
            if (lines[0].ToLower().StartsWith("get"))
            {
                TestMethodComboBox.Text = "GET";
            }
            else
            {
                TestMethodComboBox.Text = "POST";
            }
            Dictionary<String, String> headers = new Dictionary<string, string>();
            for (int i = 1; i < lines.Length; i++)
            {
                if (String.IsNullOrEmpty(lines[i]))
                {
                    break;
                }
                Int32 splitModder = lines[i].IndexOf(":");
                try
                {
                    if (splitModder < 0)
                    {
                        continue;
                    }
                    String fieldName = lines[i].Substring(0, splitModder);
                    String fieldValue = lines[i].Substring(splitModder + 1, lines[i].Length - splitModder - 1).Trim();
                    headers.Add(fieldName.Trim(), fieldValue.Trim());
                }
                catch { }
            }
            //获得HOST
            if (headers.ContainsKey("Host"))
            {
                String host = headers["Host"].Trim();
                String[] tags = host.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                TestHostTextBox.Text = tags[0];
                Int32 port = 80;
                if (TestProtocolComboBox.Text.ToLower().Equals("https"))
                {
                    port = 443;
                }
                if (tags.Length == 2)
                {
                    port = Convert.ToInt32(tags[1]);
                }
                TestPortTextBox.Text = Convert.ToString(port);
            }
            //获得Body
            if (TestMethodComboBox.Text.ToUpper().Equals("POST"))
            {
                Int32 splitModder = context.IndexOf("\n\n");
                String body = context.Substring(splitModder + 1, context.Length - splitModder - 1).Trim();
                String encode = TestEncodeComboBox.Text.Equals("自动") ? "UTF-8" : TestEncodeComboBox.Text;
                Int32 length = Encoding.GetEncoding(encode).GetByteCount(body);
                StringBuilder sber = new StringBuilder(lines[0]).Append("\n");
                if (headers.ContainsKey("Content-Length"))
                {
                    headers["Content-Length"] = Convert.ToString(length);
                }
                else
                {
                    headers.Add("Content-Length", Convert.ToString(length));
                }
                foreach (String key in headers.Keys)
                {
                    sber.Append(key + ": " + headers[key]).Append("\n");
                }
                sber.Append("\n");
                if (String.IsNullOrEmpty(body))
                {
                    body = "\n";
                }
                sber.Append(body);
                context = sber.ToString();
            }
            else
            {
                context = context.Trim() + "\n\n";
            }
            if (!context.Equals(TestSendTextBox.Text))
            {
                TestSendTextBox.Text = context;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            running = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                ExhausitiverConfig config = parseConfig();

                String json = JsonHandle.toJson(config);
                json = JsonHandle.JsonTree(json);
                String fileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".conf";
                FileStream fs = new FileStream(fileName, FileMode.Create);
                byte[] data = System.Text.Encoding.Default.GetBytes(json);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
                MessageBox.Show("保存成功(" + fileName + ")");
            }
            catch { }




        }

        private ExhausitiverConfig parseConfig()
        {
            ExhausitiverConfig config = new ExhausitiverConfig();
            ExhausitiverEntity general = new ExhausitiverEntity();
            general.Protocol = ConfigProtocolComboBox.Text;
            general.Host = ConfigHostTextBox.Text.Trim();
            general.Method = ConfigMethodComboBox.Text;
            general.Port = Convert.ToInt32(ConfigPortTextBox.Text);
            general.TimeOut = Convert.ToInt32(ConfigTimeOutCombox.Text.Replace("秒", "")) * 1000;
            general.Encode = ConfigEncodeComboBox.Text;
            String body = ConfigBodyTextBox.Text;
            body = body.Replace("\r\n", "\n");
            body = body.Replace("\n", "\r\n");
            general.Data = Encoding.GetEncoding(ConfigEncodeComboBox.Text.Equals("自动") ? "UTF-8" : ConfigEncodeComboBox.Text).GetBytes(body);
            general.Body = body;
            general.ThreadNum = Convert.ToInt32(ConfigThreadNumComboBox.Text);
            config.General = general;
            ExhaustiverVerification verification = new ExhaustiverVerification();
            verification.CalcType = ConfigCalcComboBox.SelectedIndex;
            verification.VerificationType = ConfigVerificationTypeComboBox.SelectedIndex;
            verification.Value = ConfigVerificationValueTextBox.Text;
            verification.SuccessThenStop=(SuccessStopCheckBox.Checked)?true:false;
            config.Verification = verification;
            List<ExhausitiverDic> dics = new List<ExhausitiverDic>();
            for (int i = 0; i < DicsListview.Items.Count; i++)
            {
                ExhausitiverDic dic = new ExhausitiverDic();
                dic.ParamName = DicsListview.Items[i].SubItems[0].Text;
                dic.Path = DicsListview.Items[i].SubItems[1].Text;
                dics.Add(dic);
            }
            config.Dics = dics;
            String json = JsonHandle.toJson(config);
            FileStream fs = new FileStream(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".conf", FileMode.Create);
            byte[] data = System.Text.Encoding.Default.GetBytes(json);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
            return config;
        }

        private void loadConfig()
        {
            loadConfig("");
        }

        private void loadConfig(String fileName)
        {
            try
            {
                if (String.IsNullOrEmpty(fileName))
                {
                    fileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".conf";
                }
                StreamReader sr = new StreamReader(fileName, Encoding.Default);
                String line;
                StringBuilder configContext = new StringBuilder();
                while ((line = sr.ReadLine()) != null)
                {
                    configContext.AppendLine(line);
                }
                sr.Close();
                ExhausitiverConfig config = (ExhausitiverConfig)JsonHandle.toBean<ExhausitiverConfig>(configContext.ToString());
                ConfigProtocolComboBox.Text = config.General.Protocol;
                ConfigHostTextBox.Text = config.General.Host;
                ConfigMethodComboBox.Text = config.General.Method;
                ConfigPortTextBox.Text = Convert.ToString(config.General.Port);
                ConfigTimeOutCombox.Text = (config.General.TimeOut * 1000) + "秒";
                ConfigEncodeComboBox.Text = config.General.Encode;
                ConfigBodyTextBox.Text = Encoding.GetEncoding(ConfigEncodeComboBox.Text.Equals("自动") ? "UTF-8" : ConfigEncodeComboBox.Text).GetString(config.General.Data);
                ConfigVerificationTypeComboBox.SelectedIndex = config.Verification.CalcType;
                ConfigCalcComboBox.SelectedIndex = config.Verification.VerificationType;
                ConfigVerificationValueTextBox.Text = config.Verification.Value;
                ConfigThreadNumComboBox.Text = Convert.ToString(config.General.ThreadNum);
                SuccessStopCheckBox.Checked = config.Verification.SuccessThenStop;
                foreach (ExhausitiverDic dic in config.Dics)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.SubItems[0].Text = dic.ParamName;
                    lvi.SubItems.Add(dic.Path);
                    DicsListview.Items.Add(lvi);
                }
            }
            catch { }
        }


        private void ConfigVerificationValueTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void ConfigBodyTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void ConfigEncodeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ConfigTimeOutCombox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void DicsListview_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ConfigProtocolComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ConfigMethodComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ConfigHostTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void ConfigPortTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void ConfigThreadNumComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Int32 threadNum = Convert.ToInt32(ConfigThreadNumComboBox.Text);
            ThreadPool.SetMaxThreads(threadNum, threadNum + 1);
            ThreadPool.SetMinThreads(threadNum, threadNum - 1);
        }

        private void ConfigCalcComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ConfigVerificationTypeComboBox.Text.Equals("响应码"))
            {
                ConfigCalcComboBox.Enabled = false;
                ConfigCalcComboBox.Text = "等于";
            }
            else
            {
                ConfigCalcComboBox.Enabled = true;
            }
        }

        private void ConfigVerificationTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (ofd.FileName != "")
                {
                    loadConfig(ofd.FileName);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            if (!initExhausitiver())
            {
                return;
            }
            Thread batchPoolThread = new Thread(batchPool);
            batchPoolThread.Start();

        }
        private int currentPersion = 0;
        private int speed = 0;
        private void batchPool()
        {
            ResultListView.Items.Clear();
            BatchProgressBar.Value = 0;
            currentPersion = 0;
            ThreadStartButton.Enabled = false;
            ThreadStartButton.Enabled = false;
            running = true;
            try
            {
                for (int i = 0; i < Convert.ToInt32(ConfigThreadNumComboBox.Text); i++)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(exhausitiverThread), i);
                }
                long whileNumbers = 0;
                while (true)
                {
                    Thread.Sleep(50);
                    if (whileNumbers > long.MaxValue - 100)
                    {
                        whileNumbers = 0;
                    }
                    whileNumbers++;
                    int workerThreads = 0;
                    int maxWordThreads = 0;
                    //int   
                    int compleThreads = 0;
                    ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                    ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);
                    BatchProgressBar.Value = Convert.ToInt32(currentPersion * 100 / dicHandle.totalNums);
                    try
                    {
                        ExhaustiverStatusLabel.Text = paramPersion.SubItems[1].Text;
                        HtmlResultTextBox.Text = paramPersion.SubItems[5].Text;
                        HtmlResultTextBox.SelectionStart = HtmlResultTextBox.TextLength;
                    }
                    catch { }
                    RemailNumberStripStatusLable.Text = "剩余数量：" + Convert.ToString(dicHandle.totalNums - currentPersion) + "  ";
                    RemainThreadToolStripStatusLabel.Text = "剩余线程：" + Convert.ToString(maxWordThreads - workerThreads) + "  ";
                    if (whileNumbers % 20 == 0)
                    {
                        SpeedStripStatusLabel.Text = "速度：" + Convert.ToString(speed) + "个/秒 ";
                        speed = 0;
                    }

                    if (workerThreads == maxWordThreads)
                    {
                        RemainThreadToolStripStatusLabel.Text = "剩余线程：0  ";
                        break;
                    }
                }
            }
            catch { }
            finally
            {
                running = false;
                ThreadStartButton.Enabled = true;
                ExhaustiverStatusLabel.Text = "等待中";
                ThreadStartButton.Enabled = true;
                BatchProgressBar.Value = 100;
                dicHandle.closeReaders();
                MessageBox.Show("执行完毕");
            }


        }
        private ListViewItem paramPersion = null;
        List<String> currentParamNames = new List<string>();
        private void exhausitiverThread(Object obj)
        {
            Dictionary<String, String> currentParam = dicHandle.nextParam();
            while (currentParam != null && currentParam.Count > 0 && running && dicHandle.totalNums >= currentPersion)
            {
                try
                {
                    Int32 length = commConfig.General.Body.Length;
                    string postBody = commConfig.General.Body;
                    string currentParamJson = JsonHandle.toJson(currentParam);
                    foreach (string paraName in currentParam.Keys)
                    {
                        postBody = postBody.Replace("${" + paraName + "}", currentParam[paraName]);
                    }
                    ExhausitiverEntity threadExhausitiverEntity = ExhausitiverEntity.parseExhausitiver(commConfig.General.Protocol, commConfig.General.Encode, commConfig.General.TimeOut, postBody);
                    ExhaustiverResult result = ExhausitiverHandle.doExhausitiver(threadExhausitiverEntity, commConfig.Verification);
                    result.Param = currentParam;
                    if (result.Success == true)
                    {
                        if (SuccessStopCheckBox.Checked) {
                            running = false;
                        }
                        ExhaustiverResultTextBox.AppendText(currentParamJson + "\r\n");
                    }
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = "0";
                    lvi.SubItems.Add(currentParamJson);
                    lvi.SubItems.Add(Convert.ToString(result.Code));
                    lvi.SubItems.Add(Convert.ToString(result.Length));
                    lvi.SubItems.Add(result.Success ? "成功" : "失败");
                    lvi.SubItems.Add(threadExhausitiverEntity.Body + "\n\n" + result.Result);
                    if (result.Success)
                    {
                        lvi.ForeColor = Color.Green;
                    }
                    paramPersion = lvi;
                    lviQueues.Enqueue(lvi);
                    if (result.Success)
                    {
                        if (SuccessStopCheckBox.Checked)
                        {
                            break;
                        }
                    }
                }
                catch { }
                finally
                {
                    speed++;
                    currentPersion++;
                    currentParam = dicHandle.nextParam();
                }
                Thread.Sleep(1);
            }
        }

        ExhausitiverConfig commConfig = null;
        DicManageHandle dicHandle = null;

        private bool initExhausitiver()
        {
            currentParamNames = matchExport(ConfigBodyTextBox.Text, new Regex("\\$\\{([A-Za-z0-9_]+)\\}"));
            if (currentParamNames == null || currentParamNames.Count < 1)
            {
                MessageBox.Show("请在正文填写参数：${参数名}");
                return false;
            }
            ExhausitiverConfig config = parseConfig();
            commConfig = config;
            if (config.Dics == null || config.Dics.Count < 1)
            {
                MessageBox.Show("请先导入字典");
                return false;
            }
            Dictionary<String, ExhausitiverDic> dics = new Dictionary<string, ExhausitiverDic>();
            foreach (ExhausitiverDic dic in config.Dics)
            {
                if (!currentParamNames.Contains(dic.ParamName))
                {
                    MessageBox.Show("字典字段名无效：" + dic.ParamName);
                    return false;
                }
                try
                {
                    dics.Add(dic.Path, dic);
                }
                catch { }
            }
            dicHandle = new DicManageHandle(dics);
            DicStripStatusLabel.Text = "总数：" + Convert.ToString(dicHandle.totalNums);
            //DicStripStatusLabel.Text = "总数：" + 1000000;
            return true;
        }
        public static List<String> matchExport(String context, Regex reg)
        {
            try
            {
                MatchCollection result = reg.Matches(context);
                List<String> results = new List<string>();
                foreach (Match m in result)
                {
                    if (String.IsNullOrEmpty(m.Value))
                    {
                        continue;
                    }
                    results.Add(m.Groups[1].Value);
                }
                return results;
            }
            catch
            {
                return null;
            }
        }

        private void 删除字典ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DicsListview.Items.Remove(DicsListview.SelectedItems[0]);

            }
            catch { }
        }

        private void 清空字典ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DicsListview.Items.Clear();
        }

        private void 添加字典ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddDicForm form = new AddDicForm(this, -1);
            form.ShowDialog();
        }

        public void addOrModifyDic(string paraName, string filePath, Int32 modifyIndex)
        {
            if (modifyIndex > -1)
            {
                DicsListview.Items[modifyIndex].SubItems[0].Text = paraName;
                DicsListview.Items[modifyIndex].SubItems[1].Text = filePath;
                return;
            }
            for (int i = 0; i < DicsListview.Items.Count; i++)
            {
                if (DicsListview.Items[i].SubItems[0].Text.Equals(paraName))
                {
                    DicsListview.Items[i].SubItems[1].Text = filePath;
                    return;
                }
            }
            ListViewItem lvi = new ListViewItem();
            lvi.SubItems[0].Text = paraName;
            lvi.SubItems.Add(filePath);
            DicsListview.Items.Add(lvi);
            return;
        }

        private void 修改字典ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AddDicForm form = new AddDicForm(this, DicsListview.SelectedItems[0].Index);
                form.ShowDialog();

            }
            catch { }
        }

        private void ConfigBodyTextBox_MouseLeave(object sender, EventArgs e)
        {
            //解析Header
            String context = ConfigBodyTextBox.Text;
            context = context.Replace("\r\n", "\n");
            if (String.IsNullOrEmpty(context))
            {
                return;
            }
            String[] lines = context.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            lines = lines[0].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //解析请求方式
            if (lines[0].ToLower().StartsWith("get"))
            {
                ConfigMethodComboBox.Text = "GET";
            }
            else
            {
                ConfigMethodComboBox.Text = "POST";
            }
            Dictionary<String, String> headers = new Dictionary<string, string>();
            for (int i = 1; i < lines.Length; i++)
            {
                
                Int32 splitModder = lines[i].IndexOf(":");
                try
                {
                    if (splitModder < 0)
                    {
                        continue;
                    }
                    String fieldName = lines[i].Substring(0, splitModder);
                    String fieldValue = lines[i].Substring(splitModder + 1, lines[i].Length - splitModder - 1).Trim();
                    headers.Add(fieldName.Trim(), fieldValue.Trim());
                }
                catch { }
            }
            //获得HOST
            if (headers.ContainsKey("Host"))
            {
                String host = headers["Host"].Trim();
                String[] tags = host.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                ConfigHostTextBox.Text = tags[0];
                Int32 port = 80;
                if (ConfigProtocolComboBox.Text.ToLower().Equals("https"))
                {
                    port = 443;
                }
                if (tags.Length == 2)
                {
                    port = Convert.ToInt32(tags[1]);
                }
                ConfigPortTextBox.Text = Convert.ToString(port);
            }
            //获得Body
            if (ConfigMethodComboBox.Text.ToUpper().Equals("POST"))
            {
                Int32 splitModder = context.IndexOf("\n\n");
                String body = context.Substring(splitModder + 1, context.Length - splitModder - 1).Trim();
                String encode = ConfigEncodeComboBox.Text.Equals("自动") ? "UTF-8" : ConfigEncodeComboBox.Text;
                Int32 length = Encoding.GetEncoding(encode).GetByteCount(body);
                StringBuilder sber = new StringBuilder(lines[0]).Append("\n");
                if (headers.ContainsKey("Content-Length"))
                {
                    headers["Content-Length"] = Convert.ToString(length);
                }
                else
                {
                    headers.Add("Content-Length", Convert.ToString(length));
                }
                foreach (String key in headers.Keys)
                {
                    sber.Append(key + ": " + headers[key]).Append("\n");
                }
                sber.Append("\n");
                if (String.IsNullOrEmpty(body))
                {
                    body = "\n";
                }
                sber.Append(body);
                context = sber.ToString();
            }
            else
            {
                context = context.Trim() + "\n\n";
            }
            if (!context.Equals(ConfigBodyTextBox.Text))
            {
                ConfigBodyTextBox.Text = context;
            }
        }

        private Queue<ListViewItem> lviQueues = new Queue<ListViewItem>();
        private void ExhaustiverTimer_Tick(object sender, EventArgs e)
        {
            try {
                ExhaustiverTimer.Enabled = false;
                List<ListViewItem> lvis = new List<ListViewItem>();
                if (ResultListView.Items.Count > 5000)
                {
                    ResultListView.Items.Clear();
                }
                int startIndex = ResultListView.Items.Count;
                while (lviQueues != null && lviQueues.Count > 0)
                {
                    startIndex++;
                    try
                    {
                        ListViewItem lvi = lviQueues.Dequeue();
                        lvi.SubItems[0].Text = Convert.ToString(startIndex + 1);
                        if (lvi == null)
                        {
                            return;
                        }
                        lvis.Add(lvi);
                        if (lvis.Count > 50)
                        {
                            ResultListView.Items.AddRange(lvis.ToArray());
                            lvis.Clear();
                        }
                    }
                    catch { }
                }
                if (lvis.Count > 0)
                {
                    ResultListView.Items.AddRange(lvis.ToArray());
                }
            }
            catch { }
            finally { ExhaustiverTimer.Enabled = true; }
            
        }

        private void 退出程序_Click(object sender, EventArgs e)
        {

            DialogResult r = MessageBox.Show("Really want to exit?", "Remind", MessageBoxButtons.YesNo);
            if (r == DialogResult.Yes)
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (running)
            {
                MessageBox.Show("执行过程中不能释放字典文件占用");
                return;
            }
            if (dicHandle != null)
            {
                dicHandle.closeReaders();
            }
            MessageBox.Show("字典文件释放成功");
        }

        private void ResultListView_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                HtmlResultTextBox.Text = ResultListView.SelectedItems[0].SubItems[5].Text;
            }
            catch { }

        }
    }
}
