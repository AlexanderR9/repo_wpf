using System;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace mylib.notifymodelbase
{
    public class NotifyModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property_name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }
    }

    //////////////////////Notify classes for tables//////////////////////////////////////
    public class TableModelBase : NotifyModelBase
    {
        public TableModelBase() :base() {RowVisible = true;}
        public bool RowVisible {get; set;}
    }
    public class TableViewModelBase : NotifyModelBase
    {
        public ObservableCollection<TableModelBase> ModelContainer { get; set; }
        public TableViewModelBase() => ModelContainer = new ObservableCollection<TableModelBase>();
        public void clearData() {ModelContainer.Clear();}
        public bool isEmpty() {return (RowCount == 0);}
        public void append(TableModelBase m) => ModelContainer.Add(m);

        public int RowCount {get => ModelContainer.Count; set {OnPropertyChanged("RowCount");}}
        public int VisibleRowCount {get => visibleRows(); set {OnPropertyChanged("VisibleRowCount");}}
        public string RowsInfo {get => String.Format("Row count: {0}/{1}", VisibleRowCount, RowCount); 
                    set {OnPropertyChanged("RowsInfo");}}

        private int visibleRows()
        {
            int n = 0;
            foreach (TableModelBase tm in ModelContainer)
                if (tm.RowVisible) n++;
            return n;    
        }
    }

}