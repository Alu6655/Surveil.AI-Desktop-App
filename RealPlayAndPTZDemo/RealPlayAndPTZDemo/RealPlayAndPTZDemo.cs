using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetSDKCS;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.SqlClient;
using System.Threading;
using Dapper;
using Timer = System.Windows.Forms.Timer;

namespace RealPlayAndPTZDemo
{
    public partial class RealPlayAndPTZDemo : Form
    {
       
        string getframe;
        
        static GetCam cam = new GetCam();
        Timer timer = new Timer();
        int count = 0;
        int[] GetCamId = new int[8];
        
        #region Field 字段
        private const int m_WaitTime = 5000;
        private const int SyncFileSize = 5* 1024 *1204;
        private static fDisConnectCallBack m_DisConnectCallBack;
        private static fHaveReConnectCallBack m_ReConnectCallBack;
        private static fRealDataCallBackEx2 m_RealDataCallBackEx2;
        private static fSnapRevCallBack m_SnapRevCallBack;

        private IntPtr m_LoginID = IntPtr.Zero;
        private NET_DEVICEINFO_Ex m_DeviceInfo;
        private IntPtr m_RealPlayID = IntPtr.Zero;
        private IntPtr m_RealPlayID1 = IntPtr.Zero;
        private IntPtr m_RealPlayID2 = IntPtr.Zero;
        private IntPtr m_RealPlayID3 = IntPtr.Zero;
        private IntPtr m_RealPlayID4 = IntPtr.Zero;
        private IntPtr m_RealPlayID5 = IntPtr.Zero;
        private IntPtr m_RealPlayID6 = IntPtr.Zero;
        private IntPtr m_RealPlayID7 = IntPtr.Zero;
        private IntPtr m_RealPlayID8 = IntPtr.Zero;
        private uint m_SnapSerialNum = 1;
        private bool m_IsInSave = false;
        private int SpeedValue = 4;
        private const int MaxSpeed = 8;
        private const int MinSpeed = 1;
        #endregion
        

        public RealPlayAndPTZDemo()
        {
            InitializeComponent();
            this.Load += new EventHandler(RealPlayAndPTZDemo_Load);
           
        }

        private void RealPlayAndPTZDemo_Load(object sender, EventArgs e)
        {
            m_DisConnectCallBack = new fDisConnectCallBack(DisConnectCallBack);
            m_ReConnectCallBack = new fHaveReConnectCallBack(ReConnectCallBack);
            m_RealDataCallBackEx2 = new fRealDataCallBackEx2(RealDataCallBackEx);
            m_SnapRevCallBack = new fSnapRevCallBack(SnapRevCallBack);
           
            try
            {
                NETClient.Init(m_DisConnectCallBack, IntPtr.Zero, null);
                NETClient.SetAutoReconnect(m_ReConnectCallBack, IntPtr.Zero);
                NETClient.SetSnapRevCallBack(m_SnapRevCallBack, IntPtr.Zero);
                InitOrLogoutUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Process.GetCurrentProcess().Kill();
            }
            //Build a list
           
            var dataSource = new List<GetRefreshRate>();
            int i = 5;
            while (i <= 500)
            {
                dataSource.Add(new GetRefreshRate() { Name = i.ToString(), Value = i });
                i = i + 5;
            }

            //dataSource.Add(new GetRefreshRate() { Name = "blah", Value = "blah" });
            //dataSource.Add(new GetRefreshRate() { Name = "blah", Value = "blah" });
            //dataSource.Add(new GetRefreshRate() { Name = "blah", Value = "blah" });

            //Setup data binding
            this.comboBox1.DataSource = dataSource;
            this.comboBox1.DisplayMember = "Name";
            this.comboBox1.ValueMember = "Value";

            // make it readonly
            this.comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

        }

        //public void Logout()
        //{
        //    bool result = NETClient.Logout(m_LoginID);
        //    if (!result)
        //    {
        //        MessageBox.Show(this, NETClient.GetLastError());
        //        return;
        //    }
        //    m_LoginID = IntPtr.Zero;
        //    realplay_pictureBox.Refresh();
        //    realplay_pictureBox1.Refresh();
        //    InitOrLogoutUI();
        //}
        public void AgainIn()
        {
            
            //ushort port = 0;
            //try
            //{
            //    port = Convert.ToUInt16(37777);
            //}
            //catch
            //{
            //    MessageBox.Show("Input port error(输入端口错误)!");
            //    return;
            //}
            //m_DeviceInfo = new NET_DEVICEINFO_Ex();
            //// m_LoginID = NETClient.LoginWithHighLevelSecurity("192.168.1.19", port, "admin", "dev@2022", EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
            //m_LoginID = NETClient.LoginWithHighLevelSecurity("192.168.1.19", port, "admin", "dev@2022", EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
            //if (IntPtr.Zero == m_LoginID)
            //{
            //    MessageBox.Show(this, NETClient.GetLastError());
            //    return;
            //}
            //LoginUI();
            cam.GetRefreshRate(getframe);
            var GetScore = cam.Dc;
            foreach (var item in GetScore)
            {
                GetCamId[count] = item.Did;
                count++;
            }
            count = 0;

            Stream(GetCamId[0]);
            Stream1(GetCamId[1]);
            //Stream2(GetCamId[2]);
            Array.Clear(GetCamId, 0, GetCamId.Length);
         
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {

                //Logout();
                //AgainIn();
                AgainIn();
            }
            catch(Exception ex)
            {
                throw ex;
            }


        }
        #region CallBack 回调
        private void DisConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            this.BeginInvoke((Action)UpdateDisConnectUI);
        }

        private void UpdateDisConnectUI()
        {
            this.Text = "RealPlayAndPTZDemo(实时预览与云台Demo) --- Offline(离线)";
        }

        private void ReConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            this.BeginInvoke((Action)UpdateReConnectUI);
        }
        private void UpdateReConnectUI()
        {
            this.Text = "RealPlayAndPTZDemo(实时预览与云台Demo) --- Online(在线)";
        }

        private void RealDataCallBackEx(IntPtr lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr param, IntPtr dwUser)
        {
            //do something such as save data,send data,change to YUV. 比如保存数据，发送数据，转成YUV等.
        }

        private void SnapRevCallBack(IntPtr lLoginID, IntPtr pBuf, uint RevLen, uint EncodeType, uint CmdSerial, IntPtr dwUser)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "capture";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (EncodeType == 10) //.jpg
            {
                DateTime now = DateTime.Now;
                string fileName = "async"+ CmdSerial.ToString()+ ".jpg";
                string filePath = path + "\\" + fileName;
                byte[] data = new byte[RevLen];
                Marshal.Copy(pBuf, data, 0, (int)RevLen);
                using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    stream.Write(data, 0, (int)RevLen);
                    stream.Flush();
                    stream.Dispose();
                }
            }
        }
        #endregion
        private void port_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
       
        private void login_button_Click(object sender, EventArgs e)
        {
            
            //if(IntPtr.Zero == m_LoginID)
            //{
            //    ushort port = 0;
            //    try
            //    {
            //        port = Convert.ToUInt16(37777);
            //    }
            //    catch
            //    {
            //        MessageBox.Show("Input port error(输入端口错误)!");
            //        return;
            //    }
            //    try
            //    {
            //        m_DeviceInfo = new NET_DEVICEINFO_Ex();
            //        // m_LoginID = NETClient.LoginWithHighLevelSecurity("192.168.1.19", port, "admin", "dev@2022", EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
            //        m_LoginID = NETClient.LoginWithHighLevelSecurity("192.168.1.19", port, "admin", "dev@2022", EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
            //        if (IntPtr.Zero == m_LoginID)
            //        {
            //            MessageBox.Show(this, NETClient.GetLastError());
            //            return;
            //        }
            //        LoginUI();
            //        cam.GetRefreshRate();
            //        var GetScore = cam.Dc;
            //        foreach (var item in GetScore)
            //        {
            //            GetCamId[count] = item.Did;
            //            count++;
            //        }
            //        count = 0;
            //        Stream(GetCamId[0]);
            //        Stream1(GetCamId[1]);
            //        timer.Start();
            //        //Stream2(GetCamId[2]);


            //        //Stream3();
            //        //Stream4();
            //        //Stream5();
            //        //Stream6();
            //        //Stream7();
            //        //Stream8();
            //        Array.Clear(GetCamId, 0, GetCamId.Length);
            //    }
            //    catch(Exception ex)
            //    {
            //        throw ex;
            //    }




            //}
            
            
                //bool result = NETClient.Logout(m_LoginID);
                //if (!result)
                //{
                //    MessageBox.Show(this, NETClient.GetLastError());
                //    return;
                //}
                //m_LoginID = IntPtr.Zero;
                timer.Stop();
                realplay_pictureBox.ResetBindings();
                realplay_pictureBox1.Refresh();
            //login_button.Text = "Login(登出)";

            //realplay_pictureBox2.Refresh();
            //realplay_pictureBox2.Refresh();
            //realplay_pictureBox3.Refresh();
            //realplay_pictureBox4.Refresh();
            //realplay_pictureBox5.Refresh();
            //realplay_pictureBox6.Refresh();
            //realplay_pictureBox7.Refresh();
            //realplay_pictureBox8.Refresh();
            this.Hide();
            Login lg = new Login();
            lg.Show();



        }

        //private void start_realplay_button_Click(object sender, EventArgs e)
        //{
        //    if (IntPtr.Zero == m_RealPlayID)
        //    {
        //        // realplay 监视
        //        EM_RealPlayType type;
        //        if(streamtype_comboBox.SelectedIndex == 0)
        //        {
        //            type = EM_RealPlayType.Realplay;
        //        }
        //        else
        //        {
        //            type = EM_RealPlayType.Realplay_1;
        //        }
        //        m_RealPlayID = NETClient.RealPlay(m_LoginID, channel_comboBox.SelectedIndex, realplay_pictureBox.Handle, type);
        //        realplay_pictureBox.Refresh();
        //        if (IntPtr.Zero == m_RealPlayID)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
        //        start_realplay_button.Text = "StopReal(停止监视)";
        //        channel_comboBox.Enabled = false;
        //        streamtype_comboBox.Enabled = false;
        //        save_button.Enabled = true;
        //    }
        //    else
        //    {
        //        // stop realplay 关闭监视
        //        bool ret = NETClient.StopRealPlay(m_RealPlayID);
        //        if (!ret)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        m_RealPlayID = IntPtr.Zero;
        //        start_realplay_button.Text = "StartReal(开始监视)";
        //        realplay_pictureBox.Refresh();
        //        channel_comboBox.Enabled = true;
        //        streamtype_comboBox.Enabled = true;
        //        save_button.Enabled = false;
        //        if (m_IsInSave)
        //        {
        //            m_IsInSave = false;
        //            save_button.Text = "StartSave(开始保存)";
        //        }
        //    }
        //}

        //private void capture_button_Click(object sender, EventArgs e)
        //{
        //    #region remote async snapshot 远程异步抓图
        //    NET_SNAP_PARAMS asyncSnap = new NET_SNAP_PARAMS();
        //    asyncSnap.Channel = (uint)channel_comboBox.SelectedIndex;
        //    asyncSnap.Quality = 6;
        //    asyncSnap.ImageSize = 2;
        //    asyncSnap.mode = 0;
        //    asyncSnap.InterSnap = 0;
        //    asyncSnap.CmdSerial = m_SnapSerialNum;
        //    bool ret = NETClient.SnapPictureEx(m_LoginID, asyncSnap, IntPtr.Zero);
        //    if (!ret)
        //    {
        //        MessageBox.Show(this, NETClient.GetLastError());
        //        return;
        //    }
        //    m_SnapSerialNum++;
        //    #endregion

        //    #region client capture 本地抓图
        //    //if (IntPtr.Zero == m_RealPlayID)
        //    //{
        //    //    MessageBox.Show(this, "Please realplay first(请先打开监视)!");
        //    //    return;
        //    //}
        //    //string path = AppDomain.CurrentDomain.BaseDirectory + "capture";
        //    //if (!Directory.Exists(path))
        //    //{
        //    //    Directory.CreateDirectory(path);
        //    //}
        //    //string filePath = path + "\\" + "client" + m_SnapSerialNum.ToString() + ".jpg";
        //    //bool result = NETClient.CapturePicture(m_RealPlayID, filePath, EM_NET_CAPTURE_FORMATS.JPEG);
        //    //if (!result)
        //    //{
        //    //    MessageBox.Show(this, NETClient.GetLastError());
        //    //    return;
        //    //}
        //    //MessageBox.Show(this, "client capture success(本地抓图成功)!");
        //    #endregion
        //}

        //private void save_button_Click(object sender, EventArgs e)
        //{
        //    if (IntPtr.Zero == m_RealPlayID)
        //    {
        //        MessageBox.Show(this, "Please realplay first(请先打开监视)!");
        //        return;
        //    }
        //    if (m_IsInSave)
        //    {
        //        bool ret = NETClient.StopSaveRealData(m_RealPlayID);
        //        if (!ret)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        m_IsInSave = false;
        //        //save_button.Text = "StartSave(开始保存)";
        //    }
        //    else
        //    {
        //        SaveFileDialog saveFileDialog = new SaveFileDialog();
        //        saveFileDialog.FileName = "data";
        //        saveFileDialog.Filter = "|*.dav";
        //        string path = AppDomain.CurrentDomain.BaseDirectory + "savedata";
        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }
        //        saveFileDialog.InitialDirectory = path;
        //        var res = saveFileDialog.ShowDialog();
        //        if (res == System.Windows.Forms.DialogResult.OK)
        //        {
        //            m_IsInSave = NETClient.SaveRealData(m_RealPlayID, saveFileDialog.FileName); //call saverealdata function.
        //            if (!m_IsInSave)
        //            {
        //                saveFileDialog.Dispose();
        //                MessageBox.Show(this, NETClient.GetLastError());
        //                return;
        //            }
        //            //save_button.Text = "StopSave(停止保存)";
        //        }
        //        saveFileDialog.Dispose();
        //    }
        //}

        //#region PTZ Control 云台控制

        //private void step_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    SpeedValue = step_comboBox.SelectedIndex + 1;
        //}

        //private void PTZControl(EM_EXTPTZ_ControlType type, int param1, int param2, bool isStop)
        //{
        //    bool ret = NETClient.PTZControl(m_LoginID, channel_comboBox.SelectedIndex, type, param1, param2, 0, isStop, IntPtr.Zero);
        //    if (!ret)
        //    {
        //        MessageBox.Show(this, NETClient.GetLastError());
        //    }
        //}

        //private void topleft_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFTTOP, SpeedValue, SpeedValue, false);
        //}

        //private void topleft_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFTTOP, SpeedValue, SpeedValue, true);
        //}

        //private void top_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.UP_CONTROL, 0, SpeedValue, false);
        //}

        //private void top_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.UP_CONTROL, 0, SpeedValue, true);
        //}

        //private void topright_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHTTOP, SpeedValue, SpeedValue, false);
        //}

        //private void topright_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHTTOP, SpeedValue, SpeedValue, true);
        //}

        //private void left_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFT_CONTROL, 0, SpeedValue, false);
        //}

        //private void left_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFT_CONTROL, 0, SpeedValue, true);
        //}

        //private void right_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHT_CONTROL, 0, SpeedValue, false);
        //}

        //private void right_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHT_CONTROL, 0, SpeedValue, true);
        //}

        //private void bottomleft_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFTDOWN, SpeedValue, SpeedValue, false);
        //}

        //private void bottomleft_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.LEFTDOWN, SpeedValue, SpeedValue, true);
        //}

        //private void bottom_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.DOWN_CONTROL, 0, SpeedValue, false);
        //}

        //private void bottom_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.DOWN_CONTROL, 0, SpeedValue, true);
        //}

        //private void bottomright_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHTDOWN, SpeedValue, SpeedValue, false);
        //}

        //private void bottomright_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.RIGHTDOWN, SpeedValue, SpeedValue, true);
        //}

        //private void zoomadd_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.ZOOM_ADD_CONTROL, 0, SpeedValue, false);
        //}

        //private void zoomadd_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.ZOOM_ADD_CONTROL, 0, SpeedValue, true);
        //}

        //private void zoomdec_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.ZOOM_DEC_CONTROL, 0, SpeedValue, false);
        //}

        //private void zoomdec_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.ZOOM_DEC_CONTROL, 0, SpeedValue, true);
        //}

        //private void focusadd_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.FOCUS_ADD_CONTROL, 0, SpeedValue, false);
        //}

        //private void focusadd_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.FOCUS_ADD_CONTROL, 0, SpeedValue, true);
        //}

        //private void focusdec_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.FOCUS_DEC_CONTROL, 0, SpeedValue, false);
        //}

        //private void focusdec_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.FOCUS_DEC_CONTROL, 0, SpeedValue, true);
        //}

        //private void apertureadd_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.APERTURE_ADD_CONTROL, 0, SpeedValue, false);
        //}

        //private void apertureadd_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.APERTURE_ADD_CONTROL, 0, SpeedValue, true);
        //}

        //private void aperturedec_button_MouseDown(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.APERTURE_DEC_CONTROL, 0, SpeedValue, false);
        //}

        //private void aperturedec_button_MouseUp(object sender, MouseEventArgs e)
        //{
        //    PTZControl(EM_EXTPTZ_ControlType.APERTURE_DEC_CONTROL, 0, SpeedValue, true);
        //}
        //#endregion

        #region Update UI 更新UI
        private void InitOrLogoutUI()
        {
            //step_comboBox.Enabled = false;
            //step_comboBox.Items.Clear();
            //login_button.Text = "Login(登录)";
            //channel_comboBox.Items.Clear();
            //channel_comboBox.Enabled = false;
            //streamtype_comboBox.Items.Clear();
            //streamtype_comboBox.Enabled = false;
            //start_realplay_button.Enabled = false;
            //capture_button.Enabled = false;
            //save_button.Enabled = false;
            //topleft_button.Enabled = false;
            //topright_button.Enabled = false;
            //top_button.Enabled = false;
            //left_button.Enabled = false;
            //right_button.Enabled = false;
            //bottom_button.Enabled = false;
            //bottomleft_button.Enabled = false;
            //bottomright_button.Enabled = false;
            //zoomadd_button.Enabled = false;
            //zoomdec_button.Enabled = false;
            //focusadd_button.Enabled = false;
            //focusdec_button.Enabled = false;
            //apertureadd_button.Enabled = false;
            //aperturedec_button.Enabled = false;
            m_RealPlayID = IntPtr.Zero;
            //start_realplay_button.Text = "StartReal(开始监视)";
            realplay_pictureBox.Refresh();
            realplay_pictureBox1.Refresh();
            realplay_pictureBox2.Refresh();
            //save_button.Text = "StartSave(开始保存)";
            this.Text = "RealPlayAndPTZDemo(实时预览与云台Demo)";
        }
        private void LoginUI()
        {
            //step_comboBox.Enabled = true;
            //for (int i = MinSpeed; i <= MaxSpeed; i++)
            //{
            //    step_comboBox.Items.Add(i);
            //}
            //step_comboBox.SelectedIndex = SpeedValue - 1;
            login_button.Text = "Logout(登出)";
            //channel_comboBox.Enabled = true;
            //streamtype_comboBox.Enabled = true;
            //start_realplay_button.Enabled = true;
            //capture_button.Enabled = true;
            //topleft_button.Enabled = true;
            //topright_button.Enabled = true;
            //top_button.Enabled = true;
            //left_button.Enabled = true;
            //right_button.Enabled = true;
            //bottom_button.Enabled = true;
            //bottomleft_button.Enabled = true;
            //bottomright_button.Enabled = true;
            //zoomadd_button.Enabled = true;
            //zoomdec_button.Enabled = true;
            //focusadd_button.Enabled = true;
            //focusdec_button.Enabled = true;
            //apertureadd_button.Enabled = true;
            //aperturedec_button.Enabled = true;
            //for (int i = 1; i <= m_DeviceInfo.nChanNum; i++)
            //{
            //    channel_comboBox.Items.Add(i);
            //}
            //streamtype_comboBox.Items.Add("Main Stream(主码流)");
            //streamtype_comboBox.Items.Add("Extra Stream(辅码流)");
            //channel_comboBox.SelectedIndex = 0;
            //streamtype_comboBox.SelectedIndex = 0;
            //this.Text = "RealPlayAndPTZDemo(实时预览与云台Demo) --- Online(在线)";
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            NETClient.Cleanup();
        }

        public void Stream(int CameraId)
        {
            //if(CameraId==0)
            //{
            //    CameraId = 1;
            //}
            try
            {
                if (IntPtr.Zero == new IntPtr(0x0000000000000000))
                {
                    // realplay 监视
                    EM_RealPlayType type;
                    if (0 == 0)
                    {
                        type = EM_RealPlayType.Realplay;
                    }
                    NETClient.StopRealPlay(m_RealPlayID);
                    m_RealPlayID = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox.Handle, type);
                    if (IntPtr.Zero == m_RealPlayID)
                    {
                        MessageBox.Show(this, NETClient.GetLastError());
                        return;
                    }
                    NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                    //start_realplay_button.Text = "StopReal(停止监视)";
                    //channel_comboBox.Enabled = false;
                    //streamtype_comboBox.Enabled = false;
                    //save_button.Enabled = true;
                }
                else
                {
                    // stop realplay 关闭监视
                    bool ret = NETClient.StopRealPlay(m_RealPlayID);
                    if (!ret)
                    {
                        MessageBox.Show(this, NETClient.GetLastError());
                        return;
                    }
                    m_RealPlayID = IntPtr.Zero;
                    //start_realplay_button.Text = "StartReal(开始监视)";
                    realplay_pictureBox.Refresh();
                    //channel_comboBox.Enabled = true;
                    //streamtype_comboBox.Enabled = true;
                    //save_button.Enabled = false;
                    //if (m_IsInSave)
                    //{
                    //    m_IsInSave = false;
                    //    save_button.Text = "StartSave(开始保存)";
                    //}
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void Stream1(int CameraId)
        {
            //if (CameraId == 0)
            //{
            //    CameraId = 2;
            //}
            try
            {
                if (IntPtr.Zero == new IntPtr(0x0000000000000000))
                {
                    // realplay 监视
                    EM_RealPlayType type;
                    if (0 == 0)
                    {
                        type = EM_RealPlayType.Realplay;
                    }
                    NETClient.StopRealPlay(m_RealPlayID1);
                    m_RealPlayID1 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox1.Handle, type);

                    if (IntPtr.Zero == m_RealPlayID)
                    {
                        MessageBox.Show(this, NETClient.GetLastError());
                        return;
                    }
                    NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                    //start_realplay_button.Text = "StopReal(停止监视)";
                    //channel_comboBox.Enabled = false;
                    //streamtype_comboBox.Enabled = false;
                    //save_button.Enabled = true;
                }
                else
                {
                    // stop realplay 关闭监视
                    bool ret = NETClient.StopRealPlay(m_RealPlayID);
                    if (!ret)
                    {
                        MessageBox.Show(this, NETClient.GetLastError());
                        return;
                    }
                    m_RealPlayID = IntPtr.Zero;
                    //start_realplay_button.Text = "StartReal(开始监视)";
                    realplay_pictureBox1.Refresh();
                    //channel_comboBox.Enabled = true;
                    //streamtype_comboBox.Enabled = true;
                    //save_button.Enabled = false;
                    //if (m_IsInSave)
                    //{
                    //    m_IsInSave = false;
                    //    save_button.Text = "StartSave(开始保存)";
                    //}
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        //public void Stream2(int CameraId)
        //{
        //    if (CameraId == 0)
        //    {
        //        CameraId = 3;
        //    }
        //    try
        //    {
        //        if (IntPtr.Zero == new IntPtr(0x0000000000000000))
        //        {
        //            // realplay 监视
        //            EM_RealPlayType type;
        //            if (0 == 0)
        //            {
        //                type = EM_RealPlayType.Realplay;
        //            }
        //            NETClient.StopRealPlay(m_RealPlayID2);
        //            m_RealPlayID2 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox2.Handle, type);
        //            if (IntPtr.Zero == m_RealPlayID)
        //            {
        //                MessageBox.Show(this, NETClient.GetLastError());
        //                return;
        //            }
        //            NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
        //            //start_realplay_button.Text = "StopReal(停止监视)";
        //            //channel_comboBox.Enabled = false;
        //            //streamtype_comboBox.Enabled = false;
        //            //save_button.Enabled = true;
        //        }
        //        else
        //        {
        //            // stop realplay 关闭监视
        //            bool ret = NETClient.StopRealPlay(m_RealPlayID);
        //            if (!ret)
        //            {
        //                MessageBox.Show(this, NETClient.GetLastError());
        //                return;
        //            }
        //            m_RealPlayID = IntPtr.Zero;
        //            //start_realplay_button.Text = "StartReal(开始监视)";
        //            realplay_pictureBox2.Refresh();
        //            //channel_comboBox.Enabled = true;
        //            //streamtype_comboBox.Enabled = true;
        //            //save_button.Enabled = false;
        //            //if (m_IsInSave)
        //            //{
        //            //    m_IsInSave = false;
        //            //    save_button.Text = "StartSave(开始保存)";
        //            //}
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public void Stream3(int CameraId)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                m_RealPlayID3 = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox3.Handle, type);
                if (IntPtr.Zero == m_RealPlayID)
                {
                    MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                NETClient.SetRealDataCallBack(m_RealPlayID3, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID);
                if (!ret)
                {
                    MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                m_RealPlayID = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox3.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream4(int CameraId)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                m_RealPlayID = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox4.Handle, type);
                if (IntPtr.Zero == m_RealPlayID)
                {
                    MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID);
                if (!ret)
                {
                    MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                m_RealPlayID = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox4.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        public void Stream5(int CameraId)
        {
            if (IntPtr.Zero == new IntPtr(0x0000000000000000))
            {
                // realplay 监视
                EM_RealPlayType type;
                if (0 == 0)
                {
                    type = EM_RealPlayType.Realplay;
                }
                m_RealPlayID = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox5.Handle, type);
                if (IntPtr.Zero == m_RealPlayID)
                {
                    MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
                //start_realplay_button.Text = "StopReal(停止监视)";
                //channel_comboBox.Enabled = false;
                //streamtype_comboBox.Enabled = false;
                //save_button.Enabled = true;
            }
            else
            {
                // stop realplay 关闭监视
                bool ret = NETClient.StopRealPlay(m_RealPlayID);
                if (!ret)
                {
                    MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                m_RealPlayID = IntPtr.Zero;
                //start_realplay_button.Text = "StartReal(开始监视)";
                realplay_pictureBox5.Refresh();
                //channel_comboBox.Enabled = true;
                //streamtype_comboBox.Enabled = true;
                //save_button.Enabled = false;
                //if (m_IsInSave)
                //{
                //    m_IsInSave = false;
                //    save_button.Text = "StartSave(开始保存)";
                //}
            }
        }
        //public void Stream6(int CameraId)
        //{
        //    if (IntPtr.Zero == new IntPtr(0x0000000000000000))
        //    {
        //        // realplay 监视
        //        EM_RealPlayType type;
        //        if (0 == 0)
        //        {
        //            type = EM_RealPlayType.Realplay;
        //        }
        //        m_RealPlayID = NETClient.RealPlay(m_LoginID,CameraId, realplay_pictureBox6.Handle, type);
        //        if (IntPtr.Zero == m_RealPlayID)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
        //        //start_realplay_button.Text = "StopReal(停止监视)";
        //        //channel_comboBox.Enabled = false;
        //        //streamtype_comboBox.Enabled = false;
        //        //save_button.Enabled = true;
        //    }
        //    else
        //    {
        //        // stop realplay 关闭监视
        //        bool ret = NETClient.StopRealPlay(m_RealPlayID);
        //        if (!ret)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        m_RealPlayID = IntPtr.Zero;
        //        //start_realplay_button.Text = "StartReal(开始监视)";
        //        realplay_pictureBox6.Refresh();
        //        //channel_comboBox.Enabled = true;
        //        //streamtype_comboBox.Enabled = true;
        //        //save_button.Enabled = false;
        //        //if (m_IsInSave)
        //        //{
        //        //    m_IsInSave = false;
        //        //    save_button.Text = "StartSave(开始保存)";
        //        //}
        //    }
        //}
        //public void Stream7(int CameraId)
        //{
        //    if (IntPtr.Zero == new IntPtr(0x0000000000000000))
        //    {
        //        // realplay 监视
        //        EM_RealPlayType type;
        //        if (0 == 0)
        //        {
        //            type = EM_RealPlayType.Realplay;
        //        }
        //        m_RealPlayID = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox7.Handle, type);
        //        if (IntPtr.Zero == m_RealPlayID)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
        //        //start_realplay_button.Text = "StopReal(停止监视)";
        //        //channel_comboBox.Enabled = false;
        //        //streamtype_comboBox.Enabled = false;
        //        //save_button.Enabled = true;
        //    }
        //    else
        //    {
        //        // stop realplay 关闭监视
        //        bool ret = NETClient.StopRealPlay(m_RealPlayID);
        //        if (!ret)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        m_RealPlayID = IntPtr.Zero;
        //        //start_realplay_button.Text = "StartReal(开始监视)";
        //        realplay_pictureBox7.Refresh();
        //        //channel_comboBox.Enabled = true;
        //        //streamtype_comboBox.Enabled = true;
        //        //save_button.Enabled = false;
        //        //if (m_IsInSave)
        //        //{
        //        //    m_IsInSave = false;
        //        //    save_button.Text = "StartSave(开始保存)";
        //        //}
        //    }
        //}
        //public void Stream8(int CameraId)
        //{
        //    if (IntPtr.Zero == new IntPtr(0x0000000000000000))
        //    {
        //        // realplay 监视
        //        EM_RealPlayType type;
        //        if (0 == 0)
        //        {
        //            type = EM_RealPlayType.Realplay;
        //        }
        //        m_RealPlayID = NETClient.RealPlay(m_LoginID, CameraId, realplay_pictureBox8.Handle, type);
        //        if (IntPtr.Zero == m_RealPlayID)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        NETClient.SetRealDataCallBack(m_RealPlayID, m_RealDataCallBackEx2, IntPtr.Zero, EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO | EM_REALDATA_FLAG.PCM_AUDIO_DATA | EM_REALDATA_FLAG.RAW_DATA | EM_REALDATA_FLAG.YUV_DATA);
        //        //start_realplay_button.Text = "StopReal(停止监视)";
        //        //channel_comboBox.Enabled = false;
        //        //streamtype_comboBox.Enabled = false;
        //        //save_button.Enabled = true;
        //    }
        //    else
        //    {
        //        // stop realplay 关闭监视
        //        bool ret = NETClient.StopRealPlay(m_RealPlayID);
        //        if (!ret)
        //        {
        //            MessageBox.Show(this, NETClient.GetLastError());
        //            return;
        //        }
        //        m_RealPlayID = IntPtr.Zero;
        //        //start_realplay_button.Text = "StartReal(开始监视)";
        //        realplay_pictureBox8.Refresh();
        //        //channel_comboBox.Enabled = true;
        //        //streamtype_comboBox.Enabled = true;
        //        //save_button.Enabled = false;
        //        //if (m_IsInSave)
        //        //{
        //        //    m_IsInSave = false;
        //        //    save_button.Text = "StartSave(开始保存)";
        //        //}
        //    }
        //}

        private void realplay_pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text=="Start")
            { 
             getframe = comboBox1.SelectedValue.ToString();
                if (IntPtr.Zero == IntPtr.Zero)
                {
                    ushort port = 0;
                    try
                    {
                        port = Convert.ToUInt16(37777);
                    }
                    catch
                    {
                        MessageBox.Show("Input port error(输入端口错误)!");
                        return;
                    }
                    try
                    {
                        m_DeviceInfo = new NET_DEVICEINFO_Ex();
                        // m_LoginID = NETClient.LoginWithHighLevelSecurity("192.168.1.19", port, "admin", "dev@2022", EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
                        m_LoginID = NETClient.LoginWithHighLevelSecurity("192.168.1.19", port, "admin", "dev@2022", EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
                        if (IntPtr.Zero == m_LoginID)
                        {
                            MessageBox.Show(this, NETClient.GetLastError());
                            return;
                        }
                        cam.GetRefreshRate(getframe);
                        var GetScore = cam.Dc;
                        foreach (var item in GetScore)
                        {
                            GetCamId[count] = item.Did;
                            count++;
                        }
                        count = 0;
                        Stream(GetCamId[0]);
                        Stream1(GetCamId[1]);
                        timer.Interval = (int.Parse(getframe) * 1000); // 10 secs
                        timer.Tick += new EventHandler(timer_Tick);
                        comboBox1.Enabled = false;
                        button1.Text = "Stop";
                        timer.Start();
                        //Stream2(GetCamId[2]);


                        //Stream3();
                        //Stream4();
                        //Stream5();
                        //Stream6();
                        //Stream7();
                        //Stream8();
                        Array.Clear(GetCamId, 0, GetCamId.Length);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            else
            {
                bool ret = NETClient.StopRealPlay(m_RealPlayID);
                bool ret1 = NETClient.StopRealPlay(m_RealPlayID1);
                bool ret2 = NETClient.StopRealPlay(m_RealPlayID2);
                bool ret3 = NETClient.StopRealPlay(m_RealPlayID3);
                bool ret4 = NETClient.StopRealPlay(m_RealPlayID4);
                bool ret5 = NETClient.StopRealPlay(m_RealPlayID5);
                bool ret6 = NETClient.StopRealPlay(m_RealPlayID6);
                bool ret7 = NETClient.StopRealPlay(m_RealPlayID7);
                bool ret8 = NETClient.StopRealPlay(m_RealPlayID8);
                if (!ret&& !ret1 && !ret2 && !ret3 && !ret4 && !ret5 && !ret6 && !ret7 && !ret8)
                {
                    MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                comboBox1.Enabled = true;
                button1.Text = "Start";
                m_RealPlayID = IntPtr.Zero;
                m_RealPlayID1 = IntPtr.Zero;
                m_RealPlayID2 = IntPtr.Zero;
                m_RealPlayID3 = IntPtr.Zero;
                m_RealPlayID4 = IntPtr.Zero;
                m_RealPlayID5 = IntPtr.Zero;
                m_RealPlayID6 = IntPtr.Zero;
                m_RealPlayID7 = IntPtr.Zero;
                m_RealPlayID8 = IntPtr.Zero;
                realplay_pictureBox.Refresh();
                realplay_pictureBox1.Refresh();
                realplay_pictureBox2.Refresh();
                realplay_pictureBox3.Refresh();
                realplay_pictureBox4.Refresh();
                realplay_pictureBox5.Refresh();
                realplay_pictureBox6.Refresh();
                realplay_pictureBox7.Refresh();
                realplay_pictureBox8.Refresh();
                timer.Stop();

            }

        }
    }
    class GetCam
    {
        SqlConnection  con = new SqlConnection("Data Source=192.168.1.101; Initial Catalog = RTODE; User ID = sa; Password=DE@2022");
        public List<DetectionClass> Dc;
        public void GetRefreshRate(string frametime) {
                try
                {
                frametime = "-" + frametime;
                var query = @"Select Did,sum(Score) as Score from Object_Detection where DateTimee>DATEADD(SECOND,"+int.Parse(frametime)+",CURRENT_TIMESTAMP)  group by Did order by Score desc";
                con.Open();
                var rs = con.Query<DetectionClass>(query);
                con.Close();
                Dc=rs.ToList();
                }

                catch (Exception ex)
                {
                    con.Close();
                    MessageBox.Show(ex.ToString());
                    throw ex;
                }
            
        }
    }
}
