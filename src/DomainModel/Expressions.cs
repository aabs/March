using System;

namespace DomainModel
{
    public class Expression
    {
    }

    public class PropertyExpression : Expression
    {
        public string PropertyName { get; set; }
    }

    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }
    }

    public class Value : Expression
    {
        public string ValueType { get; set; }
        public string TheValue { get; set; }
    }
}