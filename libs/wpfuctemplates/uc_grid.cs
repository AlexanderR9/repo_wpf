using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Data;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

using mylib.stringworker;
using mylib.notifymodelbase;


namespace wpfuctemplates
{
    public class ConvertItemToIndex : IValueConverter
    {
        private DataGrid dg = null;
        public ConvertItemToIndex(DataGrid table) :base() {dg = table;}


        #region IValueConverter Members
        //Convert the Item to an Index
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                //Get the CollectionView from the DataGrid that is using the converter
                //DataGrid dg = (DataGrid)Application.Current.MainWindow.FindName("DG1");
                CollectionView cv = (CollectionView)dg.Items;
                //Get the index of the item from the CollectionView
                int rowindex = cv.IndexOf(value)+1;
                return rowindex.ToString();
            }
            catch (Exception e)
            {
                throw new NotImplementedException(e.Message);
            }
        }
        //One way binding, so ConvertBack is not implemented
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


    //////////////////////////USER CONTROL CLASS////////////////////////////////
    public partial class MyGridTeplate : UserControl
    {
         public MyGridTeplate()
        {
            InitializeComponent();
            m_rowCount = 0;
            m_vrowCount = 0;
            //rowsLabel.DataContext = this;
            viewSource = new CollectionViewSource();
        }
        private int m_rowCount;
        private int m_vrowCount;
        private CollectionViewSource viewSource;



        public string filterText1() {return searchText1.Text;}
        public string filterText2() {return searchText2.Text;}
        public void setBoxTitle(string s) => box.Header = s;
        public int colCount() {return table.Columns.Count;}
        //public int rowCount() {return table.Items.Count;}
        public DataGrid TableObj {get {return table;}}
        public CollectionViewSource ViewSource {get {return viewSource;}}
        public string RowsInfo {get {return String.Format("Records count: {0}/{1}", visibleRows(), m_rowCount);}}
        public void updateRowCount(int n, int vn)
        {
            m_rowCount = n;
            m_vrowCount = vn;
            rowsLabel.Content = RowsInfo;
        }
        private int visibleRows()
        {
            int n = 0;
            foreach (var item in viewSource.View) n++;
            return n;
        }

        public void setViewSource<T>(ObservableCollection<T> source)
        {
            viewSource.Source = source;  
            table.ItemsSource = viewSource.View;
        }
        public void setItemsSource(ObservableCollection<TableModelBase> source)
        {
            table.ItemsSource = source;
        }
        public void setSource(TableViewModelBase vm)
        {
            table.ItemsSource = vm.ModelContainer;
            rowsLabel.DataContext = vm;
        }
        private DataGridColumn colAt(int col_index)
        {
            foreach (DataGridColumn col in table.Columns)
                if (col.DisplayIndex == col_index) return col;
            return null;
        }
        public void setHeaderLabels(List<string> list)
        {
            foreach (string s in list)
                addColumn(s);
        }
        private void addColumn(string header)
        {
            DataGridTextColumn col = new DataGridTextColumn();           
            col.Header = header;
            col.DisplayIndex = colCount();
            table.Columns.Add(col);
        }
        public void setColumnBinding(int col_index, string binding_path)
        {
            Binding binding = new Binding(binding_path); 
            //binding.UpdateSourceTrigger = PropertUpdateSourceTriggeryChanged;
            DataGridColumn col = colAt(col_index);
            if (col != null) ((DataGridTextColumn)col).Binding = binding;      
            else Console.WriteLine("ERR: col by index {0} not found", col_index);      
        }
        private string cellText(int row, int col)
        {
            var content = table.Columns[col].GetCellContent(table.Items[row]);
            if (content != null)
            {
                if (content is TextBlock) 
                    return ((TextBlock)content).Text;
            }
            return "";    
        }
        private void searchText1_TextChanged(object sender, EventArgs e)
        {
            viewSource.View.Refresh();
            rowsLabel.Content = RowsInfo;
        }
        /*
        private void searchText2_TextChanged(object sender, EventArgs e)
        {
            viewSource.View.Refresh();
            rowsLabel.Content = RowsInfo;
        }
        */



    }
}