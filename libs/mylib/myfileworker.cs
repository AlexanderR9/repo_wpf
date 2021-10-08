using System;
using System.IO;
using System.Collections.Generic;
using mylib.stringworker;

namespace mylib.fileworker
{
    public static class MyIconSources
    {
        //дефолтный путь к иконкам
        public static string defaultIconsDir() {return "../icons";}

        //возвращает список имен файлов (без полного пути, но с рассширением) типа png, в указанной папке или в дефолтной папке
        public static List<string> iconFiles(out string err, string dir_path = "")
        {
            List<string> list = new List<string>();
            err = String.Empty;
            if (dir_path.Trim() == String.Empty) dir_path = defaultIconsDir();
            if (!Directory.Exists(dir_path)) 
            {
                err = String.Format("folder not found - [{0}]", dir_path); 
                return list;
            }

            list = MyFileWorker.dirFiles(dir_path);
            return list;
        }

        //возвращает полный путь к файлу иконки по ключу
        public static string getIconPath(string key)
        {
            key = key.Trim().ToLower();
            string file_name = "unknown.png";
            if (key == "exit") file_name = "exit.png";
            else if (key == "app_settings") file_name = "settings.png";
            else if (key == "ok") file_name = "ok.png";
            else if (key == "apply") file_name = "apply.png";
            else if (key == "cancel") file_name = "cancel.png";
            else if (key == "start") file_name = "start.png";
            else if (key == "stop") file_name = "stop.png";
            else if (key == "add") file_name = "add.png";
            else if (key == "remove") file_name = "remove.png";
            else if (key == "clean") file_name = "clean.png";
            else if (key == "timer") file_name = "timer.png";
            else if (key == "clock") file_name = "clock.png";
            else if (key == "open") file_name = "open.png";
            else if (key == "load" || key == "read_file") file_name = "load.png";
            else if (key == "arrow_up") file_name = "up-arrow.png";
            else if (key == "arrow_down") file_name = "down-arrow.png";
            else if (key == "ball_orange") file_name = "ball_orange.png";

            else if (key == "usa_flag") file_name = "flags/usa.png";
            else if (key == "rus_flag") file_name = "flags/rus.png";
            else if (key == "germany_flag") file_name = "flags/germany.png";
            else if (key == "england_flag") file_name = "flags/england.png";
            else if (key == "brazil_flag") file_name = "flags/brazil.png";
            else if (key == "japan_flag") file_name = "flags/japan.png";
            else if (key == "canada_flag") file_name = "flags/canada.png";
            else if (key == "china_flag") file_name = "flags/china.png";
            else if (key == "ireland_flag") file_name = "flags/ireland.png";
            else if (key == "chf_flag") file_name = "flags/chf.png";
            else if (key == "netherlands_flag") file_name = "flags/netherlands.png";
            else if (key == "india_flag") file_name = "flags/india.png";
            else if (key == "argentina_flag") file_name = "flags/argentina.png";

            return String.Format("{0}/{1}", defaultIconsDir(), file_name);
        }
    }

    //класс для работы с файлами и папками
    public class MyFileWorker
    {
        public MyFileWorker(string path)
        {
            f_path = path.Trim();
            reset();
        }

        //возвращает путь от куда запущена прога
        public static string appPath() {return Environment.CurrentDirectory;}

        //возвращает список имен файлов (без полного пути) в указанной папке,
        // если задан параметр f_type в формате(".xxx"), то вернет список файлов с таким рассширением
        public static List<string> dirFiles(string dir_path, string f_type = "")
        {
            List<string> list = new List<string>();
            if (!Directory.Exists(dir_path.Trim())) return list; 

            f_type = f_type.Trim();
            if (MyString.takeLeft(ref f_type, 1) != ".") f_type = "";

            string [] fileEntries = Directory.GetFiles(dir_path);
            foreach(string fileName in fileEntries)
            {
                if (f_type != String.Empty)
                {
                    if (Path.GetExtension(fileName) == f_type)
                        list.Add(fileName);
                }
                else list.Add(fileName);
            }
            return list;
        }
        //возвращает путь к файлу без его имени, т.е. папку где он лежит
        public static string filePath(string full_name)
        {
            string dir_path = "";
            if (full_name.Trim() == "") return dir_path;
            if (!File.Exists(full_name)) return dir_path;

            FileInfo fi = new FileInfo(full_name);
            return fi.DirectoryName;
        }

        
        private string f_path;
        private string last_err;
        private string f_data;
        

        public string data() {return f_data;}
        public bool hasErr() {return (last_err != "");}
        public string errValue() {return last_err;}
        private void reset() {last_err = ""; f_data = "";}

        public void tryRead()
        {
            if (f_path == "")
            {
                last_err = "File is Empty";
                return;
            }
            if (!File.Exists(f_path))
            {
                last_err = String.Format("File not found: [{0}]", f_path);
                return;
            }
            try {f_data = File.ReadAllText(f_path);}
            catch {last_err = String.Format("Error reading file: [{0}]", f_path);}
        }

        //возвращает содержимое файла в виде массива строк
        public List<string> toStringList()
        {
            List<string> list = new List<string>();
            if (hasErr()) return list;
            MyString.split(ref f_data, "\n", list);
            return list;
        }

    }
}