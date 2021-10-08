using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using mylib.fileworker;

namespace wpfuctemplates
{
    

    public partial class MyToolBarTeplate : UserControl
    {
        public enum ToolBarActionTypes {atExit = 501, atAppSettings, atReadFile, atOpen, atStart, atStop,
                                            atAdd, atRemove, atApply, atCancel, atClear, atTimer, atClock};

        public int ActionsSize 
        {
            get 
            {
                Button action = (Button)bar.Items[0];
                if (action != null) return (int)action.Width;  
                return 24;
            } 
            set {setActionsSize(value);}
        }
        public event Action<object, int> OnActionEvent;

        public MyToolBarTeplate()
        {
            InitializeComponent();
            m_actionTypes = new List<int>();

            addAction((int)ToolBarActionTypes.atAppSettings);
            addAction((int)ToolBarActionTypes.atExit);
            setActionsSize(32);
        }
        
        public void addAction(int type)
        {
            if (m_actionTypes.Contains(type)) return;
            m_actionTypes.Add(type);

            Button action = new Button();
            action.Uid = String.Format("{0}", type);

            string icon_path = MyIconSources.getIconPath(keyByActionType(type));
            Image img = new Image();
            img.Source = (ImageSource)(new ImageSourceConverter().ConvertFromString(icon_path));
            action.Content = img;
            bar.Items.Add(action);

            action.Click += OnActionClick;
        }
        private void OnActionClick(object sender, RoutedEventArgs e)
        {
            string uid = ((UIElement)sender).Uid;
            int a_type = Convert.ToInt32(uid);
            OnActionEvent?.Invoke(this, a_type);
        }

        public void setActionsSize(int size)
        {
            if (size < 20 || size > 100) return;

            foreach(Button action in bar.Items)
            {
                action.Width = size;
                action.Height = size;
            }
        }
        public void setActionEnabled(int type, bool enable)
        {
            foreach(Button action in bar.Items)
            {
                if (action.Uid == String.Format("{0}", type))
                {
                    action.IsEnabled = enable;
                }
            }
        }

        public static string keyByActionType(int type)
        {
            switch (type)
            {
                case (int)ToolBarActionTypes.atExit: return "exit";
                case (int)ToolBarActionTypes.atAppSettings: return "app_settings";
                case (int)ToolBarActionTypes.atReadFile: return "read_file";
                case (int)ToolBarActionTypes.atStart: return "start";
                case (int)ToolBarActionTypes.atStop: return "stop";
                case (int)ToolBarActionTypes.atOpen: return "open";
                case (int)ToolBarActionTypes.atAdd: return "add";
                case (int)ToolBarActionTypes.atRemove: return "remove";
                case (int)ToolBarActionTypes.atClear: return "clean";
                case (int)ToolBarActionTypes.atApply: return "apply";
                case (int)ToolBarActionTypes.atCancel: return "cancel";
                case (int)ToolBarActionTypes.atTimer: return "timer";
                case (int)ToolBarActionTypes.atClock: return "clock";


                default: break;
            }
            return "???";
        }

        private List<int> m_actionTypes; 

    }
}