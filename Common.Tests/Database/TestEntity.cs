﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Database
{
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<TestEntityItem> TestItems { get; set; }
    }
}