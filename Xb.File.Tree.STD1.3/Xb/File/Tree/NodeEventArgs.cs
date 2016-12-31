using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File.Tree
{
    public class NodeEventArgs : EventArgs
    {
        public INode Node { get; private set; }

        public NodeEventArgs(INode node)
        {
            this.Node = node;
        }
    }
}
