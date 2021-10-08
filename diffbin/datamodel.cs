using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Threading;



namespace diff_bin
{
    public class NotifyModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property_name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }
    }

    struct BinState
    {
        //BinState() {reset();}
        public bool loading_now;
        public string bytes_size;

        public int state_color; //0-файл не загружен, 1-файл загружается, 2-файл загружен успешно, 3-ошибка загрузки
        public int progress_value;
        public int len_color; //0-не счем сравнивать, 1-нулевая длина, 2-длина ок, 3-длина не совпадает с ori файлом

        public void reset() {loading_now = false; bytes_size = "???"; state_color = 0; progress_value = 0; len_color = 0;}
    
    }

    public class FLBinModel : NotifyModelBase
    {
        public event Action<object> OnLoadFinishedEvent;
        public FLBinModel() :base() 
        {
            BinData = new List<byte>();
            LoadingNow = false;
            m_state = new BinState();
            m_state.reset();
            FileName = "";
            DirName = "";
        }

        public string FileName {get; private set;} //short name
        public string DirName {get; private set;}
        public List<byte> BinData {get;}
        public bool isLoaded() {return (m_state.state_color == 2);}
        public bool lenFault() {return (m_state.len_color == 3);}
        public bool lenOk() {return (m_state.len_color == 2);}
        public void resetState()
        {
            m_state.reset();
            BinData.Clear();
            FileName = DirName = "";

            OnPropertyChanged("BytesSize");
            OnPropertyChanged("StateColor");
            OnPropertyChanged("LenColor");
        }
        public void copyBinData(List<byte> data)
        {
            data.Clear();
            data.AddRange(BinData);
        }
        public bool hasDifference(List<byte> data)
        {
            if (BinData.Count != data.Count) return true;
            for (int i=0; i<BinData.Count; i++)
                if (data[i] != BinData[i]) return true;
            return false;
        }
        public void setFaultLen() 
        {
            m_state.len_color = 3;
            OnPropertyChanged("LenColor");
        }
        public string BytesSize 
        {
            get {return m_state.bytes_size;} 
            set {m_state.bytes_size = value; OnPropertyChanged("BytesSize");}
        }
        public bool LoadingNow
        {
            get {return m_state.loading_now;} 
            private set {m_state.loading_now = value; OnPropertyChanged("LoadingNow");}
        }
        public int Progress
        {
            get {return m_state.progress_value;} 
            private set {m_state.progress_value = value; OnPropertyChanged("Progress");}
        }
        public SolidColorBrush StateColor
        {
            get 
            {
                switch (m_state.state_color)
                {
                    case 0: return new SolidColorBrush(Colors.Cornsilk);
                    case 1: return new SolidColorBrush(Colors.Yellow);
                    case 2: return new SolidColorBrush(Colors.Chartreuse);
                    default:break;
                }
                return new SolidColorBrush(Colors.DarkOrange);
            } 
        }
        public SolidColorBrush LenColor
        {
            get 
            {
                switch (m_state.len_color)
                {
                    case 3: return new SolidColorBrush(Colors.Red);
                    case 1: return new SolidColorBrush(Colors.DarkOrange);
                    case 2: return new SolidColorBrush(Colors.Black);
                    default:break;
                }
                return new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
            } 
        }
        private void startLoading()
        {
            BinData.Clear();
            LoadingNow = true;
            BytesSize = "---";
            Progress = 0;
            m_state.state_color = 1;
            OnPropertyChanged("StateColor");
        }
        private void finishedLoading()
        {
            LoadingNow = false;
            BytesSize = String.Format("{0}", BinData.Count);
            if (BinData.Count > 0) 
            {
                m_state.state_color = 2;
                m_state.len_color = 2;
            }
            else 
            {
                m_state.state_color = 3;
                m_state.len_color = 1;
            }
            OnPropertyChanged("StateColor");
            OnPropertyChanged("LenColor");
        }

        private BinState m_state;

        public void loadFile(string f_name)
        {
            FileName = Path.GetFileName(f_name);
            DirName = Path.GetDirectoryName(f_name);
            //Console.WriteLine("f_name={0}   dir={1}  file={2}", f_name, DirName, FileName);
            startLoading();
            using (FileStream fs = File.OpenRead(f_name))
            {
                //Console.WriteLine("FLBinModel::loadFile() open read");
                BinaryReader br = new BinaryReader(fs);
                long f_size = br.BaseStream.Length; 
                int j = 0;
                for (int i=0; i<f_size; i++)
                {
                    BinData.Add(br.ReadByte());

                    j++;
                    if (j == 5) {Progress = (int)(100*i/f_size); j = 0;}
                }
                br.Dispose();
            }
           finishedLoading();
        }
        public async void loadFileAsync(string f_name)
        {
            if (LoadingNow) return;
            await Task.Run(()=>loadFile(f_name));
            OnLoadFinishedEvent?.Invoke(this);
        } 
    }


    public class FLDataModel : NotifyModelBase
    {
        public event Action<object> OnCompareFinishedEvent;
        public event Action<object, string> OnErrEvent;

        public FLDataModel() :base() 
        {
            initObjects();
            //reset(); 
            //BasePath = ""; 
            m_diffData = new List<int>();
        }
        public FLBinModel ORIFile = null;
        public FLBinModel MODFile = null;
        public FLBinModel DESFile = null;
        private void resetAll()
        {
            ORIFile.resetState();
            MODFile.resetState();
            DESFile.resetState();
        }

        private List<int> m_diffData;

        //public string BasePath {get; set;}
        private void initObjects()
        {
            ORIFile = new FLBinModel();
            MODFile = new FLBinModel();
            DESFile = new FLBinModel();

            ORIFile.OnLoadFinishedEvent += finished;
            MODFile.OnLoadFinishedEvent += finished;
            DESFile.OnLoadFinishedEvent += finished;
        }


        private void finished(object sender)
        {
            Console.WriteLine("loading finished");
            checkLens();
            //Thread.Sleep(100);
            if (allLoadedOk()) 
            {
                compare();
                OnCompareFinishedEvent?.Invoke(this);

                //Thread.Sleep(1000);
                resetAll();
            }
        }
        private void checkLens()
        {
            if (!ORIFile.isLoaded()) return;
            if (MODFile.isLoaded() && MODFile.BytesSize != ORIFile.BytesSize)
            {
                MODFile.setFaultLen();
                OnErrEvent?.Invoke(this, String.Format("invalid file size: {0}", MODFile.BytesSize));
                
            }
            if (DESFile.isLoaded() && DESFile.BytesSize != ORIFile.BytesSize) 
            {
                DESFile.setFaultLen();
                OnErrEvent?.Invoke(this, String.Format("invalid file size: {0}", DESFile.BytesSize));
            }
        }
        private void compare()
        {
            Console.WriteLine("compare()");
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss:fff"));

            m_diffData.Clear();
            int n = ORIFile.BinData.Count;
            for (int i=0; i<n; i++)
            {
                if (ORIFile.BinData[i] != MODFile.BinData[i])
                    m_diffData.Add(i);
            }
            Console.WriteLine("m_diffData size: {0}", m_diffData.Count);
            n = m_diffData.Count;
            if (m_diffData.Count == 0) return; //нет разницы

            //create des+ori
            List<byte> list = new List<byte>();
            DESFile.copyBinData(list);
            for (int i=0; i<n; i++)
            {
                int diff_index = m_diffData[i];
                list[diff_index] = ORIFile.BinData[diff_index];
            }
            createDiffFile(list, ORIFile.FileName);

            //create des+mod
            DESFile.copyBinData(list);
            for (int i=0; i<n; i++)
            {
                int diff_index = m_diffData[i];
                list[diff_index] = MODFile.BinData[diff_index];
            }
            createDiffFile(list, MODFile.FileName);

            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss:fff"));
        }
        private void createDiffFile(List<byte> list, string f_name)
        {
            string diff_file_name = DESFile.FileName.Substring(0, DESFile.FileName.Length - 4);
            
            if (f_name == ORIFile.FileName) diff_file_name += String.Format("_ORI.bin");
            else if (f_name == MODFile.FileName) diff_file_name += String.Format("_MOD.bin");
            else diff_file_name += String.Format("_???.bin");
            //diff_file_name += String.Format("_{0}", f_name);


            diff_file_name = String.Format("{0}\\{1}", DESFile.DirName, diff_file_name);
            if (File.Exists(diff_file_name)) 
            {
                Console.WriteLine("try delete file {0}", f_name);
                File.Delete(diff_file_name);
            }

            if (DESFile.hasDifference(list))
            {
                writeBinFile(diff_file_name, list);
            }
        }
        private void writeBinFile(string f_name, List<byte> data)
        {
            int arr_size = data.Count;
            Console.WriteLine("try writeBinFile {0},  data size {1}", f_name, arr_size);
            byte[] arr = new byte[arr_size];
            for (int i=0; i<arr_size; i++) arr[i] = data[i];

            using (BinaryWriter bw = new BinaryWriter(File.Open(f_name, FileMode.OpenOrCreate)))
            {
                bw.Write(arr, 0, arr_size);
            }

        }
        private bool allLoadedOk()
        {
            return (ORIFile.lenOk() && MODFile.lenOk() && DESFile.lenOk());
        }
        public void loadFile(string f_name, string btn_name, out string err)
        {
            err = "";
            Console.WriteLine("try read file: {0}", f_name); 

            if (btn_name.ToLower().Contains("ori")) ORIFile.loadFileAsync(f_name);
            else if (btn_name.ToLower().Contains("mod")) MODFile.loadFileAsync(f_name);
            else if (btn_name.ToLower().Contains("des")) DESFile.loadFileAsync(f_name);
        }

    }




}


