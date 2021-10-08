using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using mylib.fileworker;
using mylib.stringworker;

namespace wpfuctemplates
{
    public class SettingParameter
    {
        public SettingParameter(int t, string caption, string id)
        {
            DataType = t;
            Caption = caption;
            Precision = -1;
            Key = id;
            m_value = errValue();

            m_fixedValues = new List<string>();
        }

        private List<string> m_fixedValues; //набор фиксированных значений ()


        public int DataType {get; private set;}
        public string Key {get; private set;} //уникальный идентификатор, example: protocol_maxsize
        public int Precision {get; private set;}
        public string Caption {get; private set;}
        public List<string> fixedValues() {return m_fixedValues;}
        private string m_value;

        public string Value
        {
            get {return m_value;}
            set
            {
                m_value = errValue();
                switch (DataType)
                {
                    case (int)AppSettings.SettingDataType.stBoolChecked:
                    case (int)AppSettings.SettingDataType.stBoolCombo: 
                    {
                        m_value = value.Trim().ToLower();
                        if (m_value == "true" || m_value == "1") m_value = "true";
                        else if (m_value == "false" || m_value == "0") m_value = "false";
                        break;
                    }
                    case (int)AppSettings.SettingDataType.stStringLine:
                    case (int)AppSettings.SettingDataType.stStringCombo: 
                    {
                        m_value = value;
                        break;
                    }
                    case (int)AppSettings.SettingDataType.stIntLine:
                    case (int)AppSettings.SettingDataType.stIntCombo: 
                    {
                        int x = MyString.toInt(value);
                        if (x.ToString() != errValue()) m_value = String.Format("{0}", x);
                        break;
                    }
                    case (int)AppSettings.SettingDataType.stDoubleLine:
                    case (int)AppSettings.SettingDataType.stDoubleCombo: 
                    {
                        double x = MyString.toDouble(value);
                        double k = Math.Pow(10, Precision);
                        if (x.ToString() != errValue()) m_value = String.Format("{0}", Math.Round(x*k)/k);
                        break;
                    }
                    default: break;
                }
            }
        }
        public void setFixedValues(List<string> fixed_values)
        {
            m_fixedValues.Clear();
            foreach (string s in fixed_values)
                if (!m_fixedValues.Contains(s)) m_fixedValues.Add(s);

            switch (DataType)
            {
                case (int)AppSettings.SettingDataType.stDoubleCombo: 
                {
                    for (int i=m_fixedValues.Count-1; i>=0; i--)
                    {
                        double x = MyString.toDouble(m_fixedValues[i]);
                        if (x.ToString() == errValue()) m_fixedValues.RemoveAt(i);
                    }
                    break;
                }
                case (int)AppSettings.SettingDataType.stIntCombo: 
                {
                    for (int i=m_fixedValues.Count-1; i>=0; i--)
                    {
                        int x = MyString.toInt(m_fixedValues[i]);
                        if (x.ToString() == errValue()) m_fixedValues.RemoveAt(i);
                    }
                    break;
                }
                case (int)AppSettings.SettingDataType.stBoolCombo: 
                {
                    while (m_fixedValues.Count > 2) 
                        m_fixedValues.RemoveAt(m_fixedValues.Count-1);
                    break;
                }
                case (int)AppSettings.SettingDataType.stStringCombo: 
                {
                    break;
                }
                default: 
                {
                    m_fixedValues.Clear();
                    break;
                }
            }
        }
        public void setPrecision(int p)
        {
            if (p < 0 || p > 8) return;
            switch (DataType)
            {
                case (int)AppSettings.SettingDataType.stDoubleLine:
                case (int)AppSettings.SettingDataType.stDoubleCombo: {Precision = p; break;}
                default: break;
            }
        }

        public static string errValue() {return "-9999";}

    //    private static int static_id = 0;
    }
    //////////////////////////////////////////////////////////////////////
    public class AppSettings
    {
        public enum SettingDataType {stStringLine = 620, stStringCombo, stIntLine, stIntCombo, stDoubleLine, stDoubleCombo,
                                stBoolChecked, stBoolCombo};

        public AppSettings()
        {
            mw_size = new Size(800, 400);
            mw_point = new Point(50, 50);
            //tb_iconsize = 40;

            m_params = new List<SettingParameter>();
        }

        public Size mw_size {get; set;}
        public Point mw_point {get; set;}

       // public int tb_iconsize  {get; set;}

        private List<SettingParameter> m_params;
        public int paramCount() {return m_params.Count;}
        public bool paramsEmpty() {return (paramCount() == 0);}
        public SettingParameter paramAt(int i)
        {
            if (paramsEmpty()) return null;
            if (i < 0 || i >= paramCount()) return null;
            return m_params[i];
        }

        public void addParameter(SettingParameter p) => m_params.Add(p);
        public string paramValue(string key)
        {
            foreach (SettingParameter p in m_params)
                if (p.Key == key) return p.Value;
            return SettingParameter.errValue();
        }
        public void setParamValue(string key, string value)
        {
            foreach (SettingParameter p in m_params)
                if (p.Key == key) {p.Value = value; break;}
        }
        public void setFixedValues(string key, List<string> fixed_values)
        {
            foreach (SettingParameter p in m_params)
                if (p.Key == key) {p.setFixedValues(fixed_values); break;}
        }



    }

    //////////////////////////////////////////////////////////////////////
    public partial class MySettingsDialog : Window
    {
        public MySettingsDialog()
        {
            InitializeComponent();

            string icon_path = MyIconSources.getIconPath("apply");
            okImage.Source = (ImageSource)(new ImageSourceConverter().ConvertFromString(icon_path));
            icon_path = MyIconSources.getIconPath("cancel");
            cancelImage.Source = (ImageSource)(new ImageSourceConverter().ConvertFromString(icon_path));

            m_oldValues = new Dictionary<string, string>();
        }
        public void setTitle(string s) => this.Title = s;
        public void setBoxTitle(string s) => settingsBox.Header = s;
        private Dictionary<string, string> m_oldValues;

        public void initParams(AppSettings settings)
        {
            for (int i=0; i<settings.paramCount(); i++)
            {
                placeParameter(settings.paramAt(i));
            }
        }
        private void placeParameter(SettingParameter p)
        {
            Console.WriteLine("placeParameter: {0}/{1}", p.Key, p.Caption);
            StackPanel panel = new StackPanel();
            panel.Orientation = 0;
            Console.WriteLine("1");
            Label label = new Label();
            label.Width = 200;
            label.HorizontalContentAlignment = HorizontalAlignment.Right;
            label.Content = p.Caption;
            panel.Children.Add(label);

            Control ctrl = null;
            switch (p.DataType)
            {
                case (int)AppSettings.SettingDataType.stStringLine:
                case (int)AppSettings.SettingDataType.stIntLine:
                case (int)AppSettings.SettingDataType.stDoubleLine:
                {
                    ctrl = new TextBox();
                    break;
                }
                case (int)AppSettings.SettingDataType.stStringCombo:
                case (int)AppSettings.SettingDataType.stIntCombo:
                case (int)AppSettings.SettingDataType.stBoolCombo:
                case (int)AppSettings.SettingDataType.stDoubleCombo:
                {
                    ctrl = new ComboBox();
                    foreach(string s in p.fixedValues())
                    {
                        Console.WriteLine(s);
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = s;
                        ((ComboBox)ctrl).Items.Add(item);
                    }
                    break;
                }

                default: return;
            }


            if (ctrl != null)
            {
                ctrl.Uid = p.Key;
                ctrl.Width = 100;
                ctrl.Margin = new Thickness(20, 0, 0, 0);
                ctrl.Tag = p.DataType;
                setControlValue(ctrl);

                panel.Children.Add(ctrl);

            }

            settingsPanel.Children.Add(panel);
        }
        private void setControlValue(Control ctrl)
        {
            switch (ctrl.Tag)
            {
                case (int)AppSettings.SettingDataType.stStringLine:
                case (int)AppSettings.SettingDataType.stIntLine:
                case (int)AppSettings.SettingDataType.stDoubleLine:
                {
                    ((TextBox)ctrl).Text = m_oldValues[ctrl.Uid];
                    break;
                }
                case (int)AppSettings.SettingDataType.stStringCombo:
                case (int)AppSettings.SettingDataType.stIntCombo:
                case (int)AppSettings.SettingDataType.stDoubleCombo:
                {
                    ((ComboBox)ctrl).SelectedIndex = 0;
                    //((ComboBox)ctrl).Find
//                    ctrl = new ComboBox();
                    break;
                }
                case (int)AppSettings.SettingDataType.stBoolCombo:
                {
                    if (((ComboBox)ctrl).Items.Count != 2)
                        ctrl.IsEnabled = false;
                    else 
                    {
                        int index = (m_oldValues[ctrl.Uid]=="true") ? 1 : 0;
                        ((ComboBox)ctrl).Text = ((ComboBox)ctrl).Items[index].ToString();
                    }     
                    break;
                }
               default: break;
            }
        }
        
    }
}