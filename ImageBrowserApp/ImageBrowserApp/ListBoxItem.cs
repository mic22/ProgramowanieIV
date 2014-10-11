using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageBrowserApp
{
    class ListBoxItem
    {
         public string Name { get; set; }
         public DateTime Date { get; set; }
         public ListBoxItem(string name, DateTime date)
         {
            this.Name = name;
            this.Date = date;
         }

         public override string ToString() { return this.Name; }
    }
}
