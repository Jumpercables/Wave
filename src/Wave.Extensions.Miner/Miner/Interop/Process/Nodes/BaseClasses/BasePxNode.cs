﻿using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Miner.Interop.Process
{
    /// <summary>
    ///     An abstract class used to handle wrapping process framework nodes into workable objects.
    /// </summary>
    /// <typeparam name="TFrameworkExtension">The type of the framework extension.</typeparam>
    public abstract class BasePxNode<TFrameworkExtension> : IDisposable, IPxNode
    {
        #region Fields

        /// <summary>
        ///     The name of the node type.
        /// </summary>
        private readonly string _NodeTypeName;

        /// <summary>
        ///     Px Application object reference.
        /// </summary>
        private readonly IMMPxApplication _PxApp;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BasePxNode{TFrameworkExtension}" /> class.
        /// </summary>
        /// <param name="pxApp">The process framework application reference.</param>
        /// <param name="nodeTypeName">Name of the node type.</param>
        /// <param name="nodeId">The node identifier.</param>
        protected BasePxNode(IMMPxApplication pxApp, string nodeTypeName, int nodeId)
            : this(pxApp, nodeTypeName)
        {
            _PxApp = pxApp;
            _NodeTypeName = nodeTypeName;

            this.Initialize(nodeId);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BasePxNode{TFrameworkExtension}" /> class.
        /// </summary>
        /// <param name="pxApp">The process framework application reference.</param>
        /// <param name="nodeTypeName">Name of the node type.</param>
        protected BasePxNode(IMMPxApplication pxApp, string nodeTypeName)
        {
            _PxApp = pxApp;
            _NodeTypeName = nodeTypeName;

            this.Initialize();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the process framework application reference.
        /// </summary>
        public IMMPxApplication PxApplication
        {
            get { return _PxApp; }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        ///     Gets or sets a value indicating whether the <see cref="BasePxNode{TFrameworkExtension}" /> has pending updates.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the <see cref="BasePxNode{TFrameworkExtension}" /> has pending updates; otherwise, <c>false</c>.
        /// </value>
        protected bool Dirty { get; set; }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        #region IPxNode Members

        /// <summary>
        ///     Sets node as the current node.
        /// </summary>
        public void Set()
        {
            _PxApp.SetCurrentNode(this.Node);
        }

        /// <summary>
        ///     Gets the history for the node.
        /// </summary>
        public IMMPxNodeHistory History { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IPxNode" /> is locked.
        /// </summary>
        /// <value>
        ///     <c>true</c> if locked; otherwise, <c>false</c>.
        /// </value>
        public bool Locked
        {
            get { return this.Node.Locked; }
            set { ((IMMPxApplicationEx5) _PxApp).SetNodeLock(this.Node, value); }
        }

        /// <summary>
        ///     Gets or sets the state.
        /// </summary>
        /// <value>
        ///     The state.
        /// </value>
        public IMMPxState State
        {
            get { return this.Node.State; }
            set { ((IMMPxApplicationEx5) _PxApp).SetNodeState(this.Node, value); }
        }

        /// <summary>
        ///     Gets the ID.
        /// </summary>
        public abstract int ID { get; }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="BasePxNode{TFrameworkExtension}" /> is open.
        /// </summary>
        /// <value>
        ///     <c>true</c> if open; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsOpen { get; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public abstract string Name { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="BasePxNode{TFrameworkExtension}" /> is valid.
        /// </summary>
        /// <value>
        ///     <c>true</c> if valid; otherwise, <c>false</c>.
        /// </value>
        public abstract bool Valid { get; }

        /// <summary>
        ///     Gets the node.
        /// </summary>
        public IMMPxNode Node { get; protected set; }

        /// <summary>
        ///     Gets the name of the version.
        /// </summary>
        /// <value>
        ///     The name of the version.
        /// </value>
        /// <remarks>The version name will not be accurate until the node has been saved to the database.</remarks>
        public virtual string VersionName
        {
            get { return _PxApp.GetVersionName(this.Node); }
        }

        /// <summary>
        ///     Adds a new history record for the node using the
        ///     specified <paramref name="description" /> and <paramref name="extraData" />.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="extraData">The extra data.</param>
        public void AddHistory(string description, string extraData)
        {
            _PxApp.AddHistory(this.History, this.Node.Id, this.Node.NodeType, description, extraData);
        }

        /// <summary>
        ///     Deletes the node from the process framework database.
        /// </summary>
        public virtual void Delete()
        {
            if (this.Node != null)
            {
                string msg = string.Empty;
                int status = 0;

                IMMPxNodeDelete delete = (IMMPxNodeDelete) this.Node;
                delete.Delete(_PxApp, ref msg, ref status);
            }

            this.Dirty = false;
        }

        /// <summary>
        ///     Updates the node by flushing the information to the database and reinitializing the underlying
        ///     <see cref="Miner.Interop.Process.IMMPxNode" />.
        /// </summary>
        public virtual void Update()
        {
            if (this.Node != null)
            {
                ((IMMPxApplicationEx) _PxApp).UpdateNodeToDB(this.Node);
            }

            this.Dirty = false;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Copies the history from this <see cref="Miner.Interop.Process.IPxNode" /> to the specified <paramref name="node" />
        ///     object.
        /// </summary>
        /// <param name="node">The node.</param>
        protected virtual void CopyHistory(IPxNode node)
        {
            if (this.History == null || node == null) return;

            this.History.Reset();

            IMMPxHistory history;
            while ((history = this.History.Next()) != null)
            {
                var copy = history.Copy();

                copy.NodeId = node.Node.Id;
                copy.nodeTypeId = node.Node.NodeType;

                node.History.Add(copy);
            }

            // Add another record for the clone to record the clone operation.
            node.AddHistory(string.Format(CultureInfo.CurrentCulture, "{0} copied from {0} " + this.ID + ".", _NodeTypeName, _NodeTypeName.ToLower(CultureInfo.CurrentCulture)), "");
        }

        /// <summary>
        ///     Copies the packet from this <see cref="Miner.Interop.Process.IPxNode" /> to the specified <paramref name="node" />
        ///     object.
        /// </summary>
        /// <param name="packetPrefix">The packet ID prefix.</param>
        /// <param name="node">The node.</param>
        /// <exception cref="System.ArgumentNullException">node</exception>
        protected virtual void CopyPacket(string packetPrefix, IPxNode node)
        {
            if (node == null) throw new ArgumentNullException("node");

            // When the entity is not a mobile node we can stop here.
            IMMPxMobileHelper helper = (IMMPxMobileHelper) _PxApp;
            if (!helper.IsMobileSide(false)) return;

            // When the original packet doesn't exist exit out.
            string fileName = helper.GetPacketPath(this.Node, true);
            if (!File.Exists(fileName)) return;

            // Create a new packet for the clone.
            string packetID = string.Format(CultureInfo.InvariantCulture, "{1}_{0}", Regex.Replace(Guid.NewGuid().ToString(), "{|}", ""), packetPrefix);
            string clonePacket = helper.CreatePacket(node.Node, true, ref packetID);

            // Load the original packet into the cloned packet.
            XmlDocument cloneDoc = new XmlDocument();
            cloneDoc.Load(fileName);

            // Update the node data.
            XmlNode element = cloneDoc.SelectSingleNode("/XML_PACKET/PACKET_ADAPTER/NODE_DATA");
            if (element != null)
                if (element.Attributes != null)
                    element.Attributes["NODE_ID"].Value = node.Node.Id.ToString(CultureInfo.InvariantCulture);

            // Save the modified packet.
            cloneDoc.Save(clonePacket);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        ///     Gets the process framework extension.
        /// </summary>
        /// <returns>Returns the <see cref="T:TFrameworkExtension" /> representing the framework extension used for the node.</returns>
        protected abstract TFrameworkExtension GetFrameworkExtension();

        /// <summary>
        ///     Gets the list builder for the node.
        /// </summary>
        /// <returns>
        ///     Returns the <see cref="IMMListBuilder" /> representing the builder for the node.
        /// </returns>
        protected abstract IMMListBuilder GetListBuilder();

        /// <summary>
        ///     Hydrates the node from the database.
        /// </summary>
        protected void Hydrate()
        {
            int nodeType = _PxApp.Helper.GetNodeTypeID(_NodeTypeName);
            IMMPxNodeEdit nodeEdit = new MMPxNodeListClass();
            nodeEdit.Initialize(nodeType, _NodeTypeName, this.ID);
            nodeEdit.DisplayName = this.Name;

            this.Node = (IMMPxNode) nodeEdit;
            ((IMMPxApplicationEx) _PxApp).HydrateNodeFromDB(this.Node);

            IMMDynamicList list = (IMMDynamicList) nodeEdit;
            IMMListBuilder builder = this.GetListBuilder();
            if (builder != null)
            {
                list.BuildObject = builder;
            }

            this.History = _PxApp.GetHistory(this.Node);
        }

        /// <summary>
        ///     Initializes the process framework node wrapper using the specified <paramref name="nodeId" /> for the node.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <param name="nodeId">The node identifier.</param>
        /// <returns>
        ///     Returns <see cref="Boolean" /> representing <c>true</c> if the node was successfully initialized; otherwise
        ///     <c>false</c>.
        /// </returns>
        protected abstract bool Initialize(TFrameworkExtension extension, int nodeId);

        /// <summary>
        ///     Initializes the process framework node wrapper using the specified <paramref name="user" /> for the node.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <param name="user">The user.</param>
        /// <returns>
        ///     Returns <see cref="Boolean" /> representing <c>true</c> if the node was successfully created; otherwise
        ///     <c>false</c>.
        /// </returns>
        protected abstract bool Initialize(TFrameworkExtension extension, IMMPxUser user);

        #endregion

        #region Private Methods

        /// <summary>
        ///     Initializes the process framework node wrapper.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        private void Initialize(int nodeId)
        {
            var frameworkExtension = this.GetFrameworkExtension();

            if (this.Initialize(frameworkExtension, nodeId))
            {
                this.Hydrate();
            }
        }

        /// <summary>
        ///     Creates the process framework node wrapper.
        /// </summary>
        private void Initialize()
        {
            var frameworkExtension = this.GetFrameworkExtension();

            if (this.Initialize(frameworkExtension, _PxApp.User))
            {
                this.Update();
                this.Hydrate();
            }
        }

        #endregion
    }
}