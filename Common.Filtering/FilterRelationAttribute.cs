using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Filtering
{
    public class FilterRelationAttribute : Attribute
    {
        public string RelationName { get; set; }
        public RelationType RelationType { get; set; }

        public FilterRelationAttribute(string relationName)
        {
            RelationName = relationName;
            RelationType = RelationType.Class;
        }

        public FilterRelationAttribute(string relationName, RelationType relationType)
        {
            RelationName = relationName;
            RelationType = relationType;
        }
    }
}
