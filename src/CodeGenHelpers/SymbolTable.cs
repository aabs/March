using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainModel.Visitor;
using DomainModel.Ast;
using Common;
using System.Diagnostics.Contracts;

namespace Parser
{
    public class SymbolTable
    {
        public List<string> Imports = new List<string>();
        public HashSet<string> ReportedReferences = new HashSet<string>();
        public Dictionary<string, Artefact> Artefacts = new Dictionary<string, Artefact>();
        public Dictionary<string, Application> Applications = new Dictionary<string, Application>();
        public Dictionary<string, Server> Servers = new Dictionary<string, Server>();
        public Queue<string> Errors = new Queue<string>();

        public void AddError(string fmt, params object[] args)
        {
            if (string.IsNullOrEmpty(fmt))
                throw new ArgumentException("fmt");

            Errors.Enqueue(string.Format(fmt, args));
        }

        #region NewArtefact event delegate and event args
        public class NewArtefactEventArgs : EventArgs
        {
            public NewArtefactEventArgs(Artefact artefact)
            {
                this.artefact = artefact;
            }
            public Artefact artefact { get; set; }
        }

        // Declare the delegate (if using non-generic pattern).
        public delegate void NewArtefactEventHandler(object sender, NewArtefactEventArgs e);

        // Declare the event.
        public event NewArtefactEventHandler NewArtefactEvent;

        // Wrap the event in a protected virtual method
        // to enable derived classes to raise the event.
        public virtual void RaiseNewArtefactEvent(Artefact artefact)
        {
            Artefacts.Add(artefact.ProperName, artefact);
            // Raise the event by using the () operator.
            NewArtefactEvent(this, new NewArtefactEventArgs(artefact));
        }
        #endregion // NewArtefact event
        #region NewApplication event delegate and event args
        public class NewApplicationEventArgs : EventArgs
        {
            public NewApplicationEventArgs(Application application)
            {
                this.application = application;
            }
            public Application application { get; set; }
        }

        // Declare the delegate (if using non-generic pattern).
        public delegate void NewApplicationEventHandler(object sender, NewApplicationEventArgs e);

        // Declare the event.
        public event NewApplicationEventHandler NewApplicationEvent;

        // Wrap the event in a protected virtual method
        // to enable derived classes to raise the event.
        public virtual void RaiseNewApplicationEvent(Application application)
        {
            Applications.Add(application.ProperName, application);
            // Raise the event by using the () operator.
            NewApplicationEvent(this, new NewApplicationEventArgs(application));
        }
        #endregion // NewApplication event
        #region NewServer event delegate and event args
        public class NewServerEventArgs : EventArgs
        {
            public NewServerEventArgs(Server server)
            {
                this.server = server;
            }
            public Server server { get; set; }
        }

        // Declare the delegate (if using non-generic pattern).
        public delegate void NewServerEventHandler(object sender, NewServerEventArgs e);

        // Declare the event.
        public event NewServerEventHandler NewServerEvent;

        // Wrap the event in a protected virtual method
        // to enable derived classes to raise the event.
        public virtual void RaiseNewServerEvent(Server server)
        {
            Servers.Add(server.ProperName, server);
            // Raise the event by using the () operator.
            NewServerEvent(this, new NewServerEventArgs(server));
        }
        #endregion // NewServer event

    }

}