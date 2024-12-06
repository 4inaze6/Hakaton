using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPoint
{
    public class ListViewItem
    {
        public DateTime Date { get; set; }
        public string KmlPath { get; set; }
        public string ImagePath { get; set; }

        public string FileName => System.IO.Path.GetFileName(ImagePath); // Получаем только имя файла
    }
}
