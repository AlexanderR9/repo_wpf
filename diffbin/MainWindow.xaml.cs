using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using user_control;

namespace diff_bin
{
    public partial class MainWindow : Window
    {
        private FLDataModel m_vm = null;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

        }
        private void setAppIcon()
        {
            //string icon_path = String.Format("{0}/icon.png", appPath());
            //this.Icon = new BitmapImage(new Uri(icon_path));
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            setAppIcon();
            m_vm = new FLDataModel();
            m_vm.OnCompareFinishedEvent += compareFinished;
            m_vm.OnErrEvent += err;
            
            //m_vm.BasePath = appPath();


            //mainGrid.DataContext = m_vm;
            oriTextBox.DataContext = m_vm.ORIFile;
            oriButton.DataContext = m_vm.ORIFile;
            oriProgress.DataContext = m_vm.ORIFile;

            modButton.DataContext = m_vm.MODFile;
            modTextBox.DataContext = m_vm.MODFile;
            modProgress.DataContext = m_vm.MODFile;

            desButton.DataContext = m_vm.DESFile;
            desTextBox.DataContext = m_vm.DESFile;
            desProgress.DataContext = m_vm.DESFile;
        }
        private void compareFinished(object sender)
        {
            protocol.addMessage("Compare finished!");
            protocol.addMessage("-------------------------------------------------------");
        }
        private void err(object sender, string msg)
        {
            protocol.addMessage(msg, (int)MyProtocolTeplate.MsgType.mtErr);
        }
        public static string appPath() {return Environment.CurrentDirectory;}
        
        
        private void event_DragLeave(object sender, DragEventArgs e)
        {
            Button btn = sender as Button;
            //if (btn != null) 
              //  btn.Background = new SolidColorBrush(Colors.Cornsilk);
        }
        private void event_DragEnter(object sender, DragEventArgs e)
        {
            //Console.WriteLine("event_DragEnter");
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Button btn = sender as Button;
                if (btn != null)
                {
                    //btn.Background = new SolidColorBrush(Colors.White);
                }
//                Console.WriteLine("DragDropEffects.Copy;");    
                e.Effects = DragDropEffects.Link;
            }
        }
        private void event_PreviewDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            Button btn = sender as Button;
            if (btn == null) return;

            //btn.Background = new SolidColorBrush(Colors.Cornsilk);

            string[] arr = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> drop_data = new List<string>();
            foreach(string s in arr) drop_data.Add(s);
            checkDropData(drop_data, btn.Name);


            //Console.WriteLine("file_Drop: [{0}],  sender_name={1}", s, btn.Name);
            //file_label.Content = (string)e.Data.GetData(DataFormats.FileDrop);
        }        
        private void checkDropData(List<string> drop_data, string btn_name)
        {
            int msg_err_type = (int)MyProtocolTeplate.MsgType.mtErr;
            Console.WriteLine("checkDropData()  drop_data size: {0},  btn_name: {1}", drop_data.Count(), btn_name);
            if (drop_data.Count() == 0)
            {
                protocol.addMessage("drop data is empty.", msg_err_type);
                return;
            }
            if (drop_data.Count() > 1)
            {
                protocol.addMessage(String.Format("droping data size: {0} > 1 ,  choose only one file.", drop_data.Count()), msg_err_type);
                return;
            }
            string f_name = drop_data[0].Trim();
            if (f_name == String.Empty)
            {
                protocol.addMessage(String.Format("file name is empty."), msg_err_type);
                return;
            }

            FileInfo fi = new FileInfo(f_name);
            if (!fi.Exists)
            {
                protocol.addMessage(String.Format("this not file:  {0}", f_name), msg_err_type);
                return;            
            }
            Console.WriteLine(fi.Extension);
            if (!fi.Extension.Trim().ToLower().Contains("bin"))
            {
                protocol.addMessage(String.Format("this not *.bin file: {0}", f_name), msg_err_type);
                return;            
            }

            string err;
            m_vm.loadFile(f_name, btn_name, out err);
            if (err != "") protocol.addMessage(err, msg_err_type);
            else protocol.addMessage(String.Format("File loaded OK: {0}", f_name), (int)MyProtocolTeplate.MsgType.mtOk);
                
        }


    }
}
