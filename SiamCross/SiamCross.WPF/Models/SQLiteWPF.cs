﻿using SiamCross.DataBase;
using SiamCross.WPF.Models;
using System.IO;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLiteWPF))]
namespace SiamCross.WPF.Models
{
    public class SQLiteWPF : ISQLite
    {
        public SQLiteWPF() { }
        public string GetDatabasePath(string sqliteFilename)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), sqliteFilename);
            return path;
        }
    }
}
