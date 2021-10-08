using System;
using investlib.structs;
using System.Collections.Generic;
using AngleSharp.Html.Dom;
using AngleSharp.Dom;

using mylib.stringworker;

//справка по выборке в дереве html:
// выбрать 1-й попавшийся элемент table у DomDocument: IElement table_node = dom.QuerySelector("table");
// выбрать все элементы tr у ноды table: var nodes = table.QuerySelectorAll("tr");
// выбрать все элементы th и td у ноды table: var nodes = table.QuerySelectorAll("th,td");
// выбрать все элементы td внутри tr у ноды table: var nodes = table.QuerySelectorAll("tr td");
// выбрать все элементы td у которых родитель tr у ноды table: var nodes = table.QuerySelectorAll("tr>td");
// выбрать все элементы у ноды table: var childs = table.QuerySelectorAll("*");
// выбрать все элементы p, которые расположены сразу после элементов div: var nodes = table.QuerySelectorAll("div+p");

// выбрать все элементы table у которых есть атрибут class со значением snapshot-table2: var nodes = dom.QuerySelectorAll("table.snapshot-table2");
// выбрать все элементы table у которых есть атрибут class с несколькими значениями: var nodes = dom.QuerySelectorAll("table.value1.value2.value3");
// выбрать все элементы div у которых есть атрибут id со значением day: var nodes = dom.QuerySelectorAll("div#day");
// выбрать все элементы div у которых есть атрибут data: var nodes = dom.QuerySelectorAll("div[data]");
// выбрать все элементы у которых есть атрибут data: var nodes = dom.QuerySelectorAll("[data]");
// выбрать все элементы у которых есть атрибут data со значением day: var nodes = dom.QuerySelectorAll("[data=\"day\"]");
// получить значение атрибута data:  string s = td_node.GetAttribute("data");


namespace investlib.htmlparser
{

    //парсер страниц с сайта https://smart-lab.ru
    public static class SmartLabParser
    {
        //возвращает текущую цену Российской компании
        public static double getCompanyPrice(IHtmlDocument dom, string ticker)
        {
            ticker = ticker.ToUpper().Trim();
            if (ticker == "") return -1;

            double price = -1;
            IElement e_table = dom.QuerySelector("table.simple-little-table.trades-table");
            var tds = e_table.QuerySelectorAll("td");
            int pos = -1;
            foreach (var item in tds)
            {
                if (pos >= 0)
                {
                    pos++;
                    if (pos == 4)
                    {
                        price = MyString.toDouble(item.TextContent);
                        break;
                    }
                    continue;
                }
                if (item.TextContent.ToUpper().Trim() == ticker) pos = 0;
            }

            return price;
        }

    }

    //парсер страниц с сайта https://finviz.com
    public static class FinvizParser
    {
        //заполняет контейнер data ценами с сайта для одной компании, 
        //1-й элемент это дата последнего поступившего значения (в формате: 20210708)
        public static void getCompanyPriceHistory(IHtmlDocument dom, List<double> data)
        {
            data.Clear();
            var tds = dom.QuerySelectorAll("script");
            int n = 0;
            foreach (var item in tds) 
            {
                n++;
                string s = item.TextContent;
                if (s.Contains("Charts.Quote"))
                {
                    //Console.WriteLine("find prices data!!!");
                    s = MyString.subStringByStr(ref s, "var data = ", "data.instrument =");
                    s = MyString.removeNextRowSymbols(ref s).Trim();

                    string last_date = MyString.subStringByStr(ref s, "\"lastDate\":", "");
                    last_date = MyString.takeLeft(ref last_date, 8);
                    data.Add(int.Parse(last_date));

                    int pos = s.IndexOf("\"open\"");
                    if (pos > 0)
                    {
                        s = MyString.removeLeft(ref s, pos+7);
                        s = MyString.subStringByStr(ref s, "[", "]");
                        List<string> split_list = new List<string>();
                        MyString.split(ref s, ",", split_list);
                        foreach(string s_price in split_list)
                        {
                            double p = MyString.toDouble(s_price);
                            data.Add(p);
                        }

                        Console.WriteLine("values number: {0},  last date {1}, prices: {2}/{3}", 
                            split_list.Count, last_date, data[0], data[data.Count-1]);
                    }
                }
              //  Console.WriteLine("{0}.  find script node, content size {1}", n, item.TextContent.Length);

            }
        }


        public static void getCompanyInfo(IHtmlDocument dom, ref CFDData data)
        {
            Console.WriteLine("TITLE: {0}", dom.Title);
            IElement e_table = dom.QuerySelector("table.snapshot-table2");
            var tds = e_table.QuerySelectorAll("td");
            
            string last_content = String.Empty;
            string s_value = String.Empty;
            foreach (var item in tds) 
            {
                s_value = item.TextContent.Trim();
                getCompanyValue(last_content, s_value, ref data);
                last_content = s_value;
            }

            data.country = getCompanyCountry(dom);
            //Console.WriteLine(data.toStr());    
        }
        public static string getCompanyCountry(IHtmlDocument dom)
        {
            string country = "???";
            IElement e_table = dom.QuerySelector("table.fullview-title");
            if (e_table == null) return country;
            IElement e_td = e_table.QuerySelector("td.fullview-links");
            if (e_td == null) return country;
            IElement e_a = e_td.QuerySelector("a[href*='geo']");
            if (e_a == null) return country;
            Console.WriteLine("find a node");
            return e_a.TextContent;
        }
        public static void getCompanyValue(string last, string value, ref CFDData data)
        {
            if (value.Length < 2) return;    

            if (last == "Market Cap") 
            {
                //Console.WriteLine("{0}/{1}", last, value);
                string bm = MyString.takeRight(ref value, 1).ToLower();
                if (!char.IsDigit(bm[0])) value = MyString.removeRight(ref value, 1);
                double k = ((bm == "m") ? 0.001 : 1);
                data.market = k * MyString.toDouble(value);
                data.market = Math.Round(data.market, 2);
            }
            else if (last == "Dividend %") 
            {
                //Console.WriteLine("{0}/{1}", last, value);
                string bm = MyString.takeRight(ref value, 1);
                if (bm == "%") value = MyString.removeRight(ref value, 1);
                data.divs = MyString.toDouble(value);
            }
            else if (last == "Payout") 
            {
                //Console.WriteLine("{0}/{1}", last, value);
                string bm = MyString.takeRight(ref value, 1);
                if (bm == "%") value = MyString.removeRight(ref value, 1);
                data.div_payout = MyString.toDouble(value);                
            }
            else if (last == "Debt/Eq") 
            {
                //Console.WriteLine("{0}/{1}", last, value);
                data.debt = MyString.toDouble(value);
            }
            else if (last == "Price") 
            {
                data.price = MyString.toDouble(value);
            }
        }



    }


    //парсер страниц с сайта https://a2-finance.com/ru
    public static class A2FinanceParser
    {
        //заполняет контейнер data информацией о предстроящих дивидендах
        public static void getDivsInfo(IHtmlDocument dom, List<CFDDivData> data)
        {
            Console.WriteLine("try get divs info..........");
            int n = 0;
            data.Clear();
            var nodes = dom.QuerySelectorAll("h4.tr-bottom-line.p-t-sm.p-l-none.m-b-md.p-b-xs,table.table.table-hover.upcomings-dividends-table");
            DateTime dt = DateTime.Now.AddYears(-25);
            foreach (var item in nodes) 
            {
                string node_name = item.NodeName.ToLower().Trim();
                if (node_name == "h4")
                {
                        string cur_date = item.TextContent.Trim();
                        cur_date += String.Format(", {0}", DateTime.Now.Year);
                        dt = DateTime.Parse(cur_date);
                }
                else if (node_name == "table")
                {
                    var trs = item.QuerySelectorAll("tbody>tr");
                    foreach (var tr_node in trs)
                    {
                        CFDDivData div_data = new CFDDivData("");
                        div_data.buy_date = dt;
                        div_data.reestr_date = dt.AddDays(2);

                        int i = 0;
                        var tds = tr_node.QuerySelectorAll("td[data-sort-value]");
                        foreach (var td_node in tds)
                        {
                            string s = td_node.GetAttribute("data-sort-value").Trim();
                            if (s == "") {i++; continue;}

                            switch(i)
                            {
                                case 0:
                                {
                                    div_data.company_ticker = s;
                                    break;
                                }
                                case 2:
                                {
                                    div_data.div_size = MyString.toDouble(s);
                                    break;
                                }
                                case 3:
                                {
                                    div_data.div = MyString.toDouble(s);
                                    break;
                                }
                                default: break;
                            }
                            i++;
                        }
                        if (div_data.company_ticker == "") continue;
                        data.Add(div_data);    
                        Console.WriteLine(div_data.toStr());

                    } 
                } 

                Console.WriteLine(item.NodeName);
                n++;
            }

/*
            IElement div = dom.QuerySelector("div#day");
            var trs = div.QuerySelectorAll("h4.tr-bottom-line, .p-t-sm, .p-l-none, .m-b-md, .p-b-xs");

            string cur_date = "";
            DateTime dt = new DateTime();
            foreach (var item in div.Children) 
            {
                if (cur_date == "")
                {
                    if (item.NodeName.ToLower().Trim() == "h4")
                    {
                        cur_date = item.TextContent.Trim();
                        cur_date += String.Format(", {0}", DateTime.Now.Year);
                        dt = DateTime.Parse(cur_date);
                    }
                    Console.WriteLine("cur_date={0}", cur_date);
                }
                else
                {
                    if (item.NodeName.ToLower().Trim() == "table")
                    {
                        IElement tbody = dom.QuerySelector("tbody");
                        if (tbody != null)
                        {
                            var div_sections = tbody.QuerySelectorAll("tr");
                            foreach (var div_section in div_sections) 
                            {
                                CFDDivData div_data = new CFDDivData("none");
                                div_data.buy_date = dt;

                                //string div_info = "\t\t";
                                var tds = div_section.QuerySelectorAll("td");
                                int td_index = 0;
                                foreach (var cell_item in tds) 
                                {
                                    string value = cell_item.TextContent;
                                    switch (td_index)
                                    {
                                        case 0:
                                        {
                                            div_data.company_ticker = value;
                                            break;
                                        }
                                        case 2:
                                        {
                                            value = value.Replace("%", " ");
                                            value = value.Replace("usd", " ");
                                            value = value.Replace("USD", " ");
                                            value = value.Trim();
                                            div_data.div = MyString.toDouble(value);
                                            break;
                                        }
                                        case 3:
                                        {
                                            value = value.Replace("%", " ");
                                            value = value.Replace("usd", " ");
                                            value = value.Replace("USD", " ");
                                            value = value.Trim();
                                            div_data.div_size = MyString.toDouble(value);
                                            break;
                                        }
                                        default: break;
                                    }
                                    td_index++;

                                    //div_info += String.Format("{0} / ", cell_item.TextContent);
                                }
                                data.Add(div_data);
                               // Console.WriteLine(div_info);
                            }

                            cur_date = "";
                        }
                    }
                }


                //Console.WriteLine(item.TextContent.Trim()); //date
                Console.WriteLine(item.NodeName);
                n++;
            }
            */
            //Console.WriteLine("finded trs count {0}", n);


        } //end func

    } //end class



    //парсер страниц с сайта https://investmint.ru/  (Российские компании)
    public static class investmintParser
    {
        //преобразует число в DateTime, чило - количество сек прошедших с 01.01.1970 
        // если is_gmt = true то функция накидывает еще 4 часа
        public static DateTime intToDate(int x, bool is_gmt = true)
        {
            DateTime dt = DateTime.Parse("01.01.1970");
            while (x > 0)
            {
                x -= 60;
                dt = dt.AddMinutes(1);        
            }
            if (is_gmt) dt = dt.AddMinutes(4*60 + 1);        
            return dt;
        }
        //заполняет контейнер data информацией о предстроящих дивидендах
        public static void getDivsInfo(IHtmlDocument dom, List<CFDDivData> data)
        {
            Console.WriteLine("try get RU divs info..........");
            data.Clear();
            IElement table = dom.QuerySelector("table");
            var trs = table.QuerySelectorAll("tr");
            DateTime dt = DateTime.Parse("01.01.1970");

            int n = 0;
            foreach (var item in trs)
            {
                //var divs = item.QuerySelectorAll("div");
                //foreach (var div_node in divs)
                {
                    //var a_nodes = div_node.QuerySelectorAll("a");
                    //foreach (var a_node in a_nodes) 
                      //  Console.WriteLine("a_node  content=[{0}]", a_node.TextContent);

                }

                int nn = 0;
                var ths = item.QuerySelectorAll("[data-bs-content=\"Утверждённые дивиденды\"]");
                foreach (var item2 in ths) nn++;
                if (nn == 0) continue;

                CFDDivData div_data = new CFDDivData("");
                var tds = item.QuerySelectorAll("span[class=\"smallbadge\"]");
                foreach (var td_node in tds)
                {
                    div_data = new CFDDivData(td_node.TextContent.ToUpper().Trim());
                    break;
                }
                if (div_data.company_ticker == "") continue;

                int i = 0;
                tds = item.QuerySelectorAll("td");
                foreach (var td_node in tds)
                {
                    Console.WriteLine("   i={0} content=[{1}]", i, td_node.TextContent);
                    string s = td_node.GetAttribute("data-order").Trim();
                    if (s == "") {i++; continue;}
                    switch(i)
                    {
                        case 1:
                        {
                            int x = MyString.toInt(s);
                            div_data.buy_date = intToDate(x);
                            break;
                        }
                        case 2:
                        {
                            int x = MyString.toInt(s);
                            div_data.reestr_date = intToDate(x);
                            break;
                        }
                        case 5:
                        {
                            div_data.div_size = MyString.toDouble(s);
                            break;
                        }
                        case 6:
                        {
                            div_data.div = MyString.toDouble(s);
                            break;
                        }
                        default: break;
                    }
                    i++;
                }
                data.Add(div_data);



                Console.WriteLine(div_data.toStr());
                n++;
                //if(n > 3) break;
            }
            Console.WriteLine("find table rows nodes: {0}", n);

        }

    }

}
