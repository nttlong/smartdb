﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LV.Db.Common
{
    public class BaseDbTable
    {
        public BaseDbTable()
        {
            Console.WriteLine(this.GetType().FullName);
        }
    }
}
