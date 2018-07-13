using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace 蓝铅笔视频下载
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    public partial class MainWindow : Window
    {

        private string[,] playid;
        private string[,] title;
        private string[] playid_result;
        private string[] title_result;
        private string[] ffmpeg_Result;
        private int num = 0;

        public MainWindow()
        {
            InitializeComponent();
            Blue_Content.Text = "";
            Blue_Content_Copy.Text = "";
            result_content.Text = "";
            OutPut_Content.Text = "";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            Blue_Content_Copy.Text = "";
            result_content.Text = "";
            while (OutPut_Content.Text == "")
            {
                System.Windows.MessageBox.Show("输出地址为空！！！");
                goto start;
            }
            Read_Www(InPut_Content.Text);
            MatchCollection plplayid = Regex.Matches(Blue_Content.Text, "playid");
            MatchCollection playid_title = Regex.Matches(Blue_Content.Text, "title");

            num = plplayid.Count;
            playid = new string[num, 10000];
            title = new string[playid_title.Count, 1000];
            playid_result = new string[num];
            title_result = new string[playid_title.Count];
            ffmpeg_Result = new string[playid_result.Length];

            lable_num.Content = num;

            int plplayid_num = plplayid.Count;
            int playid_title_num = playid_title.Count;

            string Blue_Content_Text = Blue_Content.Text.ToString().Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");//格式化字符串

            if (num > 0)
            {
                //Address_Playid(Blue_Content_Text, plplayid_num, playid_title_num);
                //匹配地址关键字，并输出
                await Address_Playid_adress(Blue_Content_Text, plplayid_num);

                //匹配名称
                await Address_Playid_name(Blue_Content_Text, playid_title_num);

                assemble(playid_result, title_result);

                File_Write(ffmpeg_Result);
            }
            else
            {
                MessageBox.Show("网址内容无有效数据！！！");
            }
            start:;
        }
        /// <summary>
        /// 读取网页源代码
        /// </summary>
        /// <param name="url"></param>
        public void Read_Www(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(uri);
                myReq.UserAgent = "User-Agent:Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705";
                myReq.Accept = "*/*";
                myReq.KeepAlive = true;
                myReq.Headers.Add("Accept-Language", "zh-cn,en-us;q=0.5");
                HttpWebResponse result = (HttpWebResponse)myReq.GetResponse();
                Stream receviceStream = result.GetResponseStream();
                StreamReader readerOfStream = new StreamReader(receviceStream, System.Text.Encoding.GetEncoding("utf-8"));
                string strHTML = readerOfStream.ReadToEnd();
                Blue_Content.Text = strHTML;
                readerOfStream.Close();
                receviceStream.Close();
                result.Close();
            }
            catch { MessageBox.Show("无效的链接！！！"); }
        }

        /// <summary>
        /// 匹配字符并获取地址
        /// </summary>
        //private async Task<string[]> Address_Playid(string Blue_Content,int plplayid, int playid_title)
        //{


        //    assemble(playid_result, title_result);
        //    return title_result;
        //}

        /// <summary>
        /// 匹配地址关键字，并输出
        /// </summary>
        /// <param name="Blue_Content"></param>
        /// <param name="plplayid"></param>
        private async Task<string[]> Address_Playid_adress(string Blue_Content, int plplayid)
        {

            if (plplayid > 0)
            {

                Regex reg1 = new Regex("playid\":\"(.+?)_");
                MatchCollection playid_playid = reg1.Matches(Blue_Content);
                for (int i = 0; i < plplayid; i++)
                {
                    playid_result[i] = playid_playid[i].ToString();
                }
                //Parallel.For(0, plplayid.Count, i =>
                //{
                //    for (int j = 0; j < 32; j++)
                //    {
                //        int key_pid = playid_playid[i].Index;
                //        try
                //        {
                //            playid[i, j] = Blue_Content[key_pid + j + 9].ToString();
                //        }
                //        catch { }
                //    }
                //});
                for (int i = 0; i < plplayid; i++)
                {
                    try
                    {
                        await this.Dispatcher.BeginInvoke(new Action(() => Blue_Content_Copy.Text += (i + 1) + "-" + playid_result[i].ToString() + "\n"));
                    }
                    catch { }
                }
            }
            return playid_result;
        }
        /// <summary>
        /// 匹配名称
        /// </summary>
        /// <param name="Blue_Content"></param>
        /// <param name="playid_title"></param>
        private async Task<string[]> Address_Playid_name(string Blue_Content, int playid_title)
        {

            if (playid_title > 0)
            {
                Regex reg2 = new Regex("title\":\"(.+?)\"");
                MatchCollection title = reg2.Matches(Blue_Content);
                for (int i = 0; i < playid_title; i++)
                {
                    try
                    {
                        UniconToCN4Mixture(title[i].ToString(), i);
                    }
                    catch { }
                }
                for (int i = 0; i < playid_title; i++)
                {
                    try
                    {
                        await this.Dispatcher.BeginInvoke(new Action(() => Blue_Content_Copy.Text += title_result[i] + "\n"));
                    }
                    catch { }
                }
            }

            for (int i = 0; i < title_result.Length; i++)//去掉名称的空格
            {
                title_result[i] = title_result[i].Replace(" ", "");
            }
            return title_result;
        }

        /// <summary>
        /// 装配为命令
        /// </summary>
        private string[] assemble(string[] playid_result, string[] title_result)
        {
            string temp1 = "ffmpeg -i http://hls.videocc.net/";// http://ab-mts.videocc.net/
            string temp2 = "_3.m3u8 -c copy ";
            for (int i = 0; i < playid_result.Length; i++)
            {
                playid_result[i] = playid_result[i].Replace("playid\":\"", "");
                playid_result[i] = playid_result[i].Replace("_", "");

            }
            for (int i = 0; i < title_result.Length; i++)
            {
                title_result[i] = title_result[i].Replace("title\":\"", "");
                title_result[i] = title_result[i].Replace("\"", "");
            }
            string OutPut = string.Empty;
            if (OutPut_Content.Text != "")
                OutPut = OutPut_Content.Text + "\\";
            for (int i = 0; i < playid_result.Length; i++)
            {
                ffmpeg_Result[i] = temp1 + playid_result[i].Substring(0, 10) + "/" + playid_result[i].Substring(playid_result.Length - 1, 1) + "/" + playid_result[i] + temp2 + OutPut + title_result[i + 1] + ".mkv";
                result_content.Text += ffmpeg_Result[i] + "\n";
            }

            return ffmpeg_Result;
            //OutPut_Content.Text + "\\.bat", ffmpeg_Result, Encoding.Default);

        }

        /// <summary>
        /// 保存命令到文件
        /// </summary>
        /// <param name="ffmpeg_Result"></param>
        private void File_Write(string[] ffmpeg_Result)
        {
            try
            {
                string filePath = OutPut_Content.Text + "\\ffmpeg.txt";
                if (!File.Exists(filePath))
                {
                    FileStream fs1 = new FileStream(filePath, FileMode.Create, FileAccess.Write);//创建写入文件
                    StreamWriter sw = new StreamWriter(fs1);
                    for (int i = 0; i < ffmpeg_Result.Length; i++)
                    {
                        sw.WriteLine(ffmpeg_Result[i], Encoding.GetEncoding(1252));//开始写入值

                    }
                    sw.Close();
                    fs1.Close();
                }
                else
                {

                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write);
                    StreamWriter sr = new StreamWriter(fs);
                    for (int i = 0; i < ffmpeg_Result.Length; i++)
                    {
                        sr.WriteLine(ffmpeg_Result[i], Encoding.GetEncoding(1252));//开始写入值                
                    }
                    sr.Close();
                    fs.Close();
                }

            }
            catch { MessageBox.Show("无效的地址"); }

            //调用文件
            //Process proc = null;
            //try
            //{
            //    string targetDir = string.Format(OutPut_Content.Text);
            //    proc = new Process();
            //    proc.StartInfo.WorkingDirectory = targetDir;
            //    proc.StartInfo.FileName = "ffmpeg.bat";
            //    proc.StartInfo.Arguments = string.Format("10");
            //    //proc.StartInfo.CreateNoWindow = true;
            //    //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//这里设置DOS窗口不显示，经实践可行
            //    proc.Start();
            //    proc.WaitForExit();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            //}
        }

        /// <summary>
        /// 将Unicon字符串转成汉字String，可以包含其它信息（如数字和英文字符等）
        /// </summary>
        /// <param name="testContent">含Unicon字符串</param>
        /// <returns>含汉字字符串</returns>
        public string[] UniconToCN4Mixture(string testContent, int i)
        {
            int startIndex = 0;
            int tempi = 0;//计数，用于无Unicon字符串防止数组为空
            string temp_testContent = testContent;
            string startStr = string.Empty;
            string uniconStr = string.Empty;
            string newChar = string.Empty;
            while (true)
            {

                startIndex = testContent.IndexOf("\\u");
                if (startIndex == -1)
                {
                    if (tempi == 0)
                        title_result[i] = temp_testContent;
                    break;
                }
                startStr = testContent.Substring(startIndex);
                uniconStr = startStr.Substring(0, 6);
                if (string.IsNullOrEmpty(uniconStr))
                {
                    break;
                }
                newChar = UniconToCN(uniconStr);
                testContent = testContent.Replace(uniconStr, newChar);
                tempi++;
                title_result[i] = testContent;

            }
            return title_result;
        }

        /// <summary>
        /// 将Unicon字符串转成汉字String
        /// </summary>
        /// <param name="str">Unicon字符串</param>
        /// <returns>汉字字符串</returns>
        public string UniconToCN(string str)
        {
            string outStr = "";
            if (!string.IsNullOrEmpty(str))
            {
                string[] strlist = str.Replace("\\", "").Split('u');
                try
                {
                    for (int i = 1; i < strlist.Length; i++)
                    {
                        //将unicode字符转为10进制整数，然后转为char中文字符  
                        outStr += (char)int.Parse(strlist[i], System.Globalization.NumberStyles.HexNumber);
                    }
                }
                catch (FormatException ex)
                {
                    outStr = ex.Message;
                }
            }
            return outStr;
        }
        /// <summary>
        /// 调用cmd
        /// </summary>
        private async void Cmd(string ffmpeg_Result)
        {
            string CmdPath = @"C:\Windows\System32\cmd.exe";

            Process process = new Process();
            process.StartInfo.UseShellExecute = false;   // 是否使用外壳程序 
            process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值 
            process.StartInfo.RedirectStandardInput = true;  // 重定向输入流 
            process.StartInfo.RedirectStandardOutput = true;  //重定向输出流 
            process.StartInfo.RedirectStandardError = true;  //重定向错误流 
            process.StartInfo.FileName = CmdPath;//待输入的执行文件路径
            process.Start();
            //string output = process.StandardOutput.ReadToEnd();//获取exe处理之后的输出信息到output
            //string error = process.StandardError.ReadToEnd(); //获取错误信息到error
            //向cmd窗口发送输入信息
            await Task.Run(() => process.StandardInput.WriteLine(ffmpeg_Result + " &exit"));
            process.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令
            process.WaitForExit();  //等待程序执行完退出进程
            process.Close();
        }
        /// <summary>
        /// button打开cmd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (num != 0)
            {
                ProgressBar_Label.Visibility = Visibility.Visible;
                Progress_Bar.Visibility = Visibility.Visible;
                Progress_Bar.Maximum = num;
                for (int i = 0; i < ffmpeg_Result.Length; i++)
                {
                    await Task.Run(() => Cmd(ffmpeg_Result[i]));
                    ProgressBar_Label.Content = "创建任务中" + (i + 1) + "/" + num;
                    Progress_Bar.Value++;
                }
                MessageBox.Show("点击确定，关闭程序，请耐心等待下载完成！！！");
                //this.Hide(); //先隐藏主窗体
                //蓝铅笔视频下载.MainWindow dlg = new 蓝铅笔视频下载.MainWindow();
                //dlg.ShowDialog();
                this.Close();//原窗体关闭
            }
            else
                MessageBox.Show("无内容！！！");
        }

        /// <summary>
        /// button打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_openfile(object sender, RoutedEventArgs e)
        {

            //打开文件
            try
            {
                if (OutPut_Content.Text != "")
                {
                    string filePath = OutPut_Content.Text + "\\ffmpeg.txt";
                    System.Diagnostics.Process.Start("notepad.exe", filePath);
                }
                else
                    MessageBox.Show("文件未建立！！！");
            }
            catch { MessageBox.Show("文件未建立！！！"); }
        }

        /// <summary>
        /// button查找名称
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Find_Name_Click(object sender, RoutedEventArgs e)
        {
            int num1, num2;
            string id_resul;

            result_content.Text = "";
            int.TryParse(idnum1.Text.ToString(), out num1);
            int.TryParse(idnum2.Text.ToString(), out num2);
            string iid = "http://m.lanqb.com/v1/getTutorialRes?id=";
            MatchCollection playid_title;
            //ProgressBar_Label.Visibility = Visibility;//进度条初始化
            //Progress_Bar.Visibility = Visibility;
            //Progress_Bar.Value = 0;
            //Progress_Bar.Maximum = num2 - num1 + 1;
            if (num1 <= num2)
            {
                for (int i = num1; i <= num2; i++)
                {
                    Blue_Content.Text = "";
                    id_resul = iid + i;
                    Read_Www(id_resul);
                    playid_title = Regex.Matches(Blue_Content.Text, "title");
                    title_result = new string[playid_title.Count];

                    if (playid_title.Count != 0)
                    {
                        await Address_Playid_name(Blue_Content.Text, playid_title.Count);
                        await this.Dispatcher.InvokeAsync(new Action(() => result_content.Text += i + ". " + title_result[title_result.Length - 1] + "\n"));
                    }
                    else
                    {
                        await this.Dispatcher.InvokeAsync(new Action(() => result_content.Text += i + ". " + "\n"));
                    }
                    //await this.Dispatcher.InvokeAsync(new Action(() => ProgressBar_Label.Content = "创建任务中" + (Progress_Bar.Value + 1) + "/" + Progress_Bar.Maximum));
                    //await this.Dispatcher.InvokeAsync(new Action(() => Progress_Bar.Value++));
                }
            }
            else { MessageBox.Show("输入错误！！！"); }
        }
    }
}

