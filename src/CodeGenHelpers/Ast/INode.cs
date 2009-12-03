using System.Dataflow;
using DomainModel.Visitor;

namespace DomainModel.Ast
{
    public interface INode : IVisitable, ISourceLocation
    {
    }
}