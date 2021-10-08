using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using mylib.fileworker;
using System.Collections.Generic;

namespace wpfuctemplates
{
    public abstract class MyMainWindowTeplate : Window
    {
        protected AppSettings m_appSettings;
        protected abstract void initWidgets();

        public MyMainWindowTeplate()
        {
            Console.WriteLine("constructor MyMainWindowTeplate");
            string key = "toolbar.iconsize";
            m_appSettings = new AppSettings();
            m_appSettings.addParameter(new SettingParameter((int)AppSettings.SettingDataType.stIntCombo, "Tool bar icon size", key));
            m_appSettings.setFixedValues(key, new List<string>() {"32", "40", "48", "56"});

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }
        public static string appSettingsFile() 
        {
            return String.Format("{0}/app_settings.json", MyFileWorker.appPath());
        }
        protected virtual void saveAppSettings()
        {
            string jsonString = JsonSerializer.Serialize(m_appSettings);
            File.WriteAllText(appSettingsFile(), jsonString);
        }
        protected virtual void loadAppSettings()
        {
            if (!File.Exists(appSettingsFile())) return;
            string jsonString = File.ReadAllText(appSettingsFile());
            m_appSettings = JsonSerializer.Deserialize<AppSettings>(jsonString);
        }
        private void setAppIcon()
        {
            string icon_path = String.Format("{0}/app.ico", MyFileWorker.appPath());
            this.Icon = new BitmapImage(new Uri(icon_path));
        }      
        protected virtual void load()
        {
            this.Width = m_appSettings.mw_size.Width;
            this.Height = m_appSettings.mw_size.Height;
            this.Left = m_appSettings.mw_point.X;
            this.Top = m_appSettings.mw_point.Y;
        }
        protected virtual void save()
        {
            m_appSettings.mw_size = new Size(this.Width, this.Height);
            m_appSettings.mw_point = new Point(this.Left, this.Top);
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            setAppIcon();
            loadAppSettings();
            initWidgets();
            load();
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            save();
            saveAppSettings();
        }
        protected void appSettings()
        {
            MySettingsDialog dlg = new MySettingsDialog();
            dlg.Owner = this;
            dlg.setTitle("Application settings!");
            dlg.initParams(m_appSettings);
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.ShowDialog();
        }

    }
}