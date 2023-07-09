﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Filtering
{
    public class FilterRelationAttribute : Attribute
    {
        public string[] RelationNames { get; set; }
        public RelationType RelationType { get; set; }
        public LogicalOperator? LogicalOperator { get; set; }

        public FilterRelationAttribute(string relationName)
        {
            RelationNames = new string[1];
            RelationNames[0] = relationName;
            RelationType = RelationType.Class;
        }

        public FilterRelationAttribute(string relationName, RelationType relationType)
        {
            RelationNames = new string[1];
            RelationNames[0] = relationName;
            RelationType = relationType;
        }

        public FilterRelationAttribute(string[] relationNames, LogicalOperator logicalOperator)
        {
            RelationNames = relationNames;
            RelationType = RelationType.Class;
            LogicalOperator = logicalOperator;
        }

        public FilterRelationAttribute(string[] relationNames, RelationType relationType, LogicalOperator logicalOperator)
        {
            RelationNames = relationNames;
            RelationType = relationType;
            LogicalOperator = logicalOperator;
        }
    }
}