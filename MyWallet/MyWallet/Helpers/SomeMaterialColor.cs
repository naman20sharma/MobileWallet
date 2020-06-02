using System;
using System.Collections.Generic;
using System.Text;

namespace MyWallet.Helpers
{
    public class SomeMaterialColor
    {
        List<string> colors = new List<string>();
        
        private void addColors()
        {
            colors.Add("f44336"); //red
            colors.Add("E91E63"); //pink
            colors.Add("9C27B0"); //puple
            colors.Add("673AB7"); //deep_puple
            colors.Add("3F51B5"); //indigo
            colors.Add("2196F3"); //blue
            colors.Add("03A9F4"); //ligth_blue
            colors.Add("00BCD4"); //cyan
            colors.Add("009688"); //teal
            colors.Add("4CAF50"); //green
            colors.Add("8BC34A"); //lightgreen
            colors.Add("CDDC39"); //lime
            colors.Add("FDD835"); //yellow_600
            colors.Add("FFC107"); //amber
            colors.Add("FF9800"); //orange
            colors.Add("FF5722"); //deep_orande
            colors.Add("795548"); //brown
            colors.Add("607D8B"); //blue_gray
        }

        public string GetColorFromString(string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            int a = BitConverter.ToInt32(bytes, 0);
            return colors[a%(colors.Count)];
        }
        public SomeMaterialColor()
        {
            addColors();
        }
    }

}
