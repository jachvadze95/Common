﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Database
{
    public class TestEntityItem
    {
        public int Id { get; set; }
        public long Amount { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }

        public TestEntity TestList { get; set; }
        public int TestListId { get; set; }
    }
}
