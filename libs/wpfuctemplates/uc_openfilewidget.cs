using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;

using mylib.fileworker;
using mylib.stringworker;

namespace wpfuctemplates
{
    public partial class MyOpenFileWidgetTeplate : UserControl
    {
        public MyOpenFileWidgetTeplate()
        {
            InitializeComponent();
            CurrentPath = "";
            FileFilter = "";

            string icon_path = MyIconSources.getIconPath("open");
            Image img = new Image();
            img.Source = (ImageSource)(new ImageSourceConverter().ConvertFromString(icon_path));
            openButton.Content = img;
        }


        public string CurrentPath
        {
            get { return (string)GetValue(DPUserProperty); }
            set { SetValue(DPUserProperty, value); }
        }
        public static readonly DependencyProperty DPUserProperty =
            DependencyProperty.Register("CurrentPath", typeof(string), typeof(MyOpenFileWidgetTeplate));


        public void setBoxTitle(string s) => box.Header = s;
        public void setLabelText(string s) => label.Content = s;
        public string FileFilter {get; set;} //exapmle: .png или .html
        public bool invalidCurrentPath()
        {
            if (CurrentPath.Trim() == "") return true;
            return !File.Exists(CurrentPath);
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            string my_filter = FileFilter;
            if (MyString.takeLeft(ref my_filter, 1) == "." && my_filter.Length > 2) 
                openFileDialog.Filter = String.Format("filter files (*{0})|*{0}|All files (*.*)|*.*", FileFilter);
            else openFileDialog.Filter = String.Format("All files (*.*)|*.*");


            if (CurrentPath.Trim() == "")
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            else openFileDialog.InitialDirectory = MyFileWorker.filePath(CurrentPath);

            openFileDialog.FilterIndex = 0;
            openFileDialog.ShowDialog();

            string old_value = CurrentPath;
            CurrentPath = openFileDialog.FileName;
            if (CurrentPath == "") CurrentPath = old_value;
        }
    }
}