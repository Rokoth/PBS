using System;

namespace Db.Attributes
{
    public class TableNameAttribute : Attribute
    {
        public string Name { get; }

        public TableNameAttribute(string name)
        {
            Name = name;
        }
    }

    public class IgnoreAttribute : Attribute
    {

    }

    public class PrimaryKeyAttribute : Attribute
    {

    }
}