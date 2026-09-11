using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal class Category(int id, string name)
    {     
        public int Id {  get; set; } = id;
        public string Name { get; set; } = name;               
    }
}
