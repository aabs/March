using System.Dataflow;
using DomainModel.Visitor;

namespace DomainModel.Ast
{
    public abstract class BaseNode : INode
    {
        public virtual void Visit(IVisitor visitor)
        {
        }

        public SourceSpan Span { get; set; }
        public string FileName { get; set; }
    }
}