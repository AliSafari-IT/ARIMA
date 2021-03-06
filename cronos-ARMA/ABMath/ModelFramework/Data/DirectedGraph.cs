﻿#region License Info
//Component of Cronos Package, http://www.codeplex.com/cronos
//Copyright (C) 2009 Anthony Brockwell

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
#endregion

using System;
using System.Collections.Generic;

namespace ABMath.ModelFramework.Data
{
    /// <summary>
    /// DirectedGraph is a collection of nodes and directed labeled (by socket) links.
    /// Each node contains a
    ///   (1) an IConnectable item
    /// Each node has incoming and outgoing links.
    /// The Graph can be viewed with a DirectedGraphViewer.
    /// </summary>
    [Serializable]
    public class DirectedGraph
    {
        [Serializable]
        public class NodeInfo
        {
            public IConnectable connectableItem;

            public double[] coordinate;

            public List<Link> outgoingLinks;
            public List<Link> incomingLinks;

            public void AdjustForNodeRemoval(int deletedNode)
            {
                for (int i=0 ; i<outgoingLinks.Count ; ++i)
                {
                    Link link = outgoingLinks[i];
                    if (outgoingLinks[i].start >= deletedNode)
                        --link.start;
                    if (outgoingLinks[i].end >= deletedNode)
                        --link.end;
                    outgoingLinks[i] = link;
                }
                for (int i = 0; i < incomingLinks.Count; ++i)
                {
                    Link link = incomingLinks[i];
                    if (link.start >= deletedNode)
                        --link.start;
                    if (link.end >= deletedNode)
                        --link.end;
                    incomingLinks[i] = link;
                }
            }

            public void RemoveOutgoingLinksTo(int other, int socket)
            {
                for (int i = 0; i < outgoingLinks.Count; ++i)
                    if (outgoingLinks[i].end == other)
                        if (outgoingLinks[i].endSocket == socket)
                        {
                            outgoingLinks.RemoveAt(i);
                            --i;
                        }
            }

            public void RemoveAllIncomingLinks(DirectedGraph parent)
            {
                int thisIndex = parent.GetNodeIndex(connectableItem);
                // remove the matching outgoing links first
                for (int targetSocket = 0; targetSocket < connectableItem.NumInputs(); ++targetSocket )
                    foreach (var link in incomingLinks)
                        parent.Nodes[link.start].RemoveOutgoingLinksTo(thisIndex, targetSocket);
                // then wipe out the incoming links
                incomingLinks = new List<Link>();
            }

            public void RemoveIncomingLinkFrom(int source, int socket)
            {
                for (int i=0 ; i<incomingLinks.Count ; ++i)
                    if (incomingLinks[i].start == source)
                        if (incomingLinks[i].endSocket == socket)
                            incomingLinks.RemoveAt(i);
            }

            public void RemoveAllOutgoingLinks(DirectedGraph parent)
            {
                int thisIndex = parent.GetNodeIndex(connectableItem);
                // remove the matching incoming links first
                foreach (var link in outgoingLinks)
                    parent.Nodes[link.end].RemoveIncomingLinkFrom(thisIndex, link.endSocket);

                // then wipe out the outgoing links
                outgoingLinks = new List<Link>();
            }

            public NodeInfo(IConnectable citem,  double[] coordinate)
            {
                connectableItem = citem;
                this.coordinate = coordinate;
                outgoingLinks = new List<Link>();
                incomingLinks = new List<Link>();
            }
        }

        [Serializable]
        public struct Link
        {
            public int start;
            public int end;
            public int startSocket;
            public int endSocket;
        }

        public List<NodeInfo> Nodes
        {
            get;
            set;
        }

        protected int GetNodeIndex(IConnectable item)
        {
            int found = -1;
            for (int i = 0; i < Nodes.Count; ++i)
                if (ReferenceEquals(item, Nodes[i].connectableItem))
                    found = i;
            return found;
        }

        public NodeInfo GetNodeContaining(IConnectable item)
        {
            int idx = GetNodeIndex(item);
            if (idx != -1)
                return Nodes[idx];
            return null;
        }

        public void AddNode(object newItem, double[] coordinate)
        {
            var ci = newItem as IConnectable;
            Nodes.Add(new NodeInfo(ci, coordinate));
        }

        public void RemoveNode(NodeInfo ni)
        {
            int found = -1;
            for (int i = 0; i < Nodes.Count; ++i)
                if (ReferenceEquals(ni, Nodes[i]))
                    found = i;
            if (found != -1)
            {
                Nodes[found].RemoveAllIncomingLinks(this);
                Nodes[found].RemoveAllOutgoingLinks(this);

                Nodes.RemoveAt(found);

                // then correct all links
                foreach (var x in Nodes)
                    x.AdjustForNodeRemoval(found);
            }
        }

        public void AddDirectionalLink(IConnectable fromItem, int fromSocket, IConnectable toItem, int toSocket)
        {
            // locate the two corresponding NodeInfo objects
            int ifrom = GetNodeIndex(fromItem);
            int ito = GetNodeIndex(toItem);
            if (ifrom != -1)
                if (ito != -1)
                {
                    var link = new Link();
                    link.start = ifrom;
                    link.startSocket = fromSocket;
                    link.end = ito;
                    link.endSocket = toSocket;

                    Nodes[ifrom].outgoingLinks.Add(link);
                    Nodes[ito].incomingLinks.Add(link);
                }
        }

        public void RemoveDirectionalLinksTo(IConnectable toItem, int toSocket)
        {
            int ito = GetNodeIndex(toItem);
            for (int i=0 ; i<Nodes[ito].incomingLinks.Count ; ++i)
            {
                var link = Nodes[ito].incomingLinks[i];
                if (link.endSocket == toSocket)
                {
                    int fromIndex = link.start;
                    Nodes[ito].incomingLinks.RemoveAt(i);
                    Nodes[fromIndex].RemoveOutgoingLinksTo(ito, toSocket);
                    --i;
                }
            }
        }

        public DirectedGraph()
        {
            Nodes = new List<NodeInfo>();
        }

        /// <summary>
        /// assumes that the link to ni1 has been changed and performs corresponding
        /// updates all the way down the chain of nodes
        /// </summary>
        /// <param name="ni1"></param>
        public void CascadeFrom(NodeInfo ni1)
        {
            int numOutputs = ni1.connectableItem.NumOutputs();

            if (numOutputs == 0)
                return;

            var connections = ni1.outgoingLinks;
            foreach (var x in connections)
                Nodes[x.end].connectableItem.SetInput(x.endSocket, ni1.connectableItem.GetOutput(x.startSocket), null);

            foreach (var x in connections)
                CascadeFrom(Nodes[x.end]);
        }
    }
}