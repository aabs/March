using Common;
using System.Linq;
using System.Collections.Generic;
namespace DomainModel.Ast
{
    partial class Architecture
    {
        public IEnumerable<Server> AllServers()
        {
            foreach (Network n in this.Artefacts.Where(s => s is Network))
            {
                foreach (var s in n.Servers)
                {
                    yield return s;
                }
            }
        }
    }
    partial class Server
    {
        public string Name
        {
            get
            {
                return DomainName.Value.Join("_");
            }
        }
    }

    partial class ArtefactBase
    {
        public string ProperName
        {
            get
            {
                if (Name == null || Name.Value == null)
                {
                    return "<Unknown>";
                }
                return Name.ProperName;
            }
        }

    }

    partial class ConstrainedDottedIdentifier
    {
        public string ProperName
        {
            get
            {
                return string.Join(".", Value.Select(s => s.IdentifierConstrained.ProperName).ToArray());
            }
        }
    }

    partial class Identifier
    {
        public string ProperName
        {
            get
            {
                return Value.Join(".");
            }
        }
   }

    partial class ConstrainedIdentifierSegment
    {
        public string ProperName
        {
            get
            {
                return IdentifierConstrained.ProperName;
            }
        }

    }
}