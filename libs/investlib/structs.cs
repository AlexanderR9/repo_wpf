using System;
using System.Collections.Generic;


using mylib.notifymodelbase;
using mylib.stringworker;


namespace investlib.structs
{

    /////////////// BondData ///////////////////////    
    public struct BondData
    {
        public BondData(ref BondData data)
        {
            this = data;
        }
        public BondData(string company)
        {
            company_name = company;
            bond_kks = "";
            price = -1;
            coupon_size = 0;
            years_finish = 0;
            coupon_date = new DateTime(1999, 1, 1);
            amortization = false;
            diff_coupon = false;
            coupon_number = 0;
            profitability = 0;
        }
        public string company_name;
        public string bond_kks;
        public double price; //текущая цена
        public double coupon_size; // размер купона в рублях
        public double years_finish; // лет до погашения
        public DateTime coupon_date; // дата купона
        public int coupon_number; //количество выплат в год
        public double profitability; //доходность относительно номинала
        public bool amortization; //амортизация
        public bool diff_coupon; //переменный купон

        public string toStr()
        {
            string s = "Bond: ";
            s += String.Format("company={0}  kks={1}  years={2}  price={3}  ", company_name, bond_kks, years_finish, price);
            s += String.Format("profitability={0}  coupon_number={1}  ", profitability, coupon_number, price);
            s += String.Format("coupon_size={0}  coupon_date={1}  amortization={2}  diff_coupon={3}", coupon_size, coupon_date, amortization, diff_coupon);     
            return s;
        }
        public bool invalid()
        {
            return (price < 0 || years_finish == 0 || bond_kks == "" || coupon_number == 0 || profitability <= 0);
        }
    }
    /////////////// CFDData ///////////////////////    
    public struct CFDData
    {
        public CFDData(string name) 
        {
            company_name = name; 
            company_ticker = "none"; 
            price= -1;
            debt = -1;
            divs = -1;
            market = 0;
            div_payout = 0;
            country = "unknown";
        }



        public string company_name;
        public string company_ticker;
        public double price; //текущая цена
        public double debt; //долг 
        public double divs; //дивиденды, %
        public double market; //капитализация компании
        public double div_payout; // доля выплат на дивиденды
        public string country; //страна

        public bool invalid() 
        {
            return (company_name == "" || company_ticker == "");
        }
        public void reset()
        {
            company_name = "none"; 
            company_ticker = ""; 
            country = "";
            price= -1;
            market = -1;
            debt = -1;
            divs = -1;
            div_payout = -1;
        }
        public string toStr()
        {
            string s = "CFD: ";
            s += String.Format("company={0} ({1})  price={2}  debt={3}  divs={4}  market={5}  payout={6}  country={7}", 
                        company_name, company_ticker, price, debt, divs, market, div_payout, country);
            return s;
        }

        public static List<string> tableHeaders()
        {
            List<string> list = new List<string>();
            list.Add("Company name");
            list.Add("Ticker");
            list.Add("Country");
            list.Add("Price");
            list.Add("Market, B");
            list.Add("Debt");
            list.Add("Dividend, %");
            list.Add("Payout");
            return list;
        }
    }
    /////////////// CFDDivData ///////////////////////    
    public struct CFDDivData
    {
        public CFDDivData(string name) 
        {
            company_ticker = name; 
            div= -1;
            div_size = -1;
            reestr_date = new  DateTime(0);
            buy_date = new  DateTime(0);
        }
        
        public string company_ticker;
        public double div; //дивиденды, %
        public double div_size; //дивиденды, абсолютная величина
        public DateTime reestr_date; //дата закрытия реестра
        public DateTime buy_date; //последняя дата покупки

        public string toStr()
        {
            string s = "CFD dividend: ";
            s += String.Format("reestr: [{0}]  company={1}  div={2}%  div_size={3}  buy date: [{4}]", 
                    reestr_date.ToString("dd.MM.yyyy"), company_ticker, div, div_size, buy_date.ToString("dd.MM.yyyy"));
            return s;
        }
    }


    //class CFDDataModel
    public class CFDDataModel : TableModelBase
    {
        public CFDDataModel() {}
        private CFDData m_data;
        public ref CFDData data() {return ref m_data;}

        public string CompanyName {get {return m_data.company_name;} set {m_data.company_name = value;}}
        public string CompanyTicker {get {return m_data.company_ticker;} set {m_data.company_ticker = value;}}
        public string Country {get {return m_data.country;} set {m_data.country = value;}}
        public double Price {get {return m_data.price;} set {m_data.price = value;}}
        public double Market {get {return m_data.market;} set {m_data.market = value;}}
        public double Debt {get {return m_data.debt;} set {m_data.debt = value;}}
        public double Divs {get {return m_data.divs;} set {m_data.divs = value;}}
        public double Payout {get {return m_data.div_payout;} set {m_data.div_payout = value;}}

        public void updateFields()
        {
            OnPropertyChanged("CompanyName");
            OnPropertyChanged("CompanyTicker");
            OnPropertyChanged("Country");
            OnPropertyChanged("Price");
            OnPropertyChanged("Market");
            OnPropertyChanged("Debt");
            OnPropertyChanged("Divs");
            OnPropertyChanged("Payout");
        }
        public string toFileLine()
        {
            return string.Format("{0} / {1} / {2} / {3} / {4} / {5} / {6} / {7}",
                CompanyName, CompanyTicker, Country, Price, Market, Debt, Divs, Payout);
        }
        public void fromFileLine(string line, out string err)
        {
            err = "";
            m_data.reset();

            List<string> split_list = new List<string>();
            MyString.split(ref line, " / ", split_list);
            if (split_list.Count != 8) 
            {
                err = String.Format("invalid line for CFD data: [{0}]", line);    
                return;
            }

            CompanyName = split_list[0].Trim();
            CompanyTicker = split_list[1].Trim();
            Country = split_list[2].Trim();
            Price = MyString.toDouble(split_list[3]);
            Market = MyString.toDouble(split_list[4]);
            Debt = MyString.toDouble(split_list[5]);
            Divs = MyString.toDouble(split_list[6]);
            Payout = MyString.toDouble(split_list[7]);
        }
        //загрузка не полных данных о CFD: (Name / Ticker / Country / Price)
        public void fromFileLine_ShortVersion(string line, out string err)
        {
            err = "";
            m_data.reset();

            List<string> split_list = new List<string>();
            MyString.split(ref line, " / ", split_list);
            if (split_list.Count < 4) 
            {
                err = String.Format("invalid line for CFD short data: [{0}]", line);    
                return;
            }

            CompanyName = split_list[0].Trim();
            CompanyTicker = split_list[1].Trim();
            Country = split_list[2].Trim();
            Price = MyString.toDouble(split_list[3]);
        }

    } //end class


} // end namespase


