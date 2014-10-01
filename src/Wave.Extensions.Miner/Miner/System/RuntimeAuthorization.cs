using System;

using ESRI.ArcGIS.esriSystem;

using Miner.Interop;
using Miner.System.Internal;

namespace Miner.System
{
    /// <summary>
    ///     A supporting class used to check out the licenses necessary to run applications outside of Miner and Miner and ESRI
    ///     products.
    /// </summary>
    public class RuntimeAuthorization : IDisposable
    {
        #region Fields

        private readonly EsriRuntimeAuthorization _EsriRuntime;
        private readonly MinerRuntimeAuthorizatio _MinerRuntime;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RuntimeAuthorization" /> class.
        /// </summary>
        public RuntimeAuthorization()
        {
            _MinerRuntime = new MinerRuntimeAuthorizatio();
            _EsriRuntime = new EsriRuntimeAuthorization();
        }

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

        #region Public Methods

        /// <summary>
        ///     Attempts to checkout the license for the specific ESRI <paramref name="licensedProduct" />.
        /// </summary>
        /// <param name="licensedProduct">The licensed product.</param>
        /// <param name="licensedExtension">The licensed extension.</param>
        /// <returns>
        ///     Returns a <see cref="Boolean" /> representing <c>true</c> when the initialization is successful; otherwise
        ///     <c>false</c>.
        /// </returns>
        public bool Initialize(esriLicenseProductCode licensedProduct, params esriLicenseExtensionCode[] licensedExtension)
        {
            return _EsriRuntime.Initialize(new[] {licensedProduct}, licensedExtension);
        }

        /// <summary>
        ///     Attempts to checkout the license for the specific ArcFM <paramref name="licensedProduct" />.
        /// </summary>
        /// <param name="licensedProduct">The licensed product.</param>
        /// <param name="licensedExtension">The extension codes.</param>
        /// <returns>
        ///     Returns a <see cref="Boolean" /> representing <c>true</c> when the initialization is successful; otherwise
        ///     <c>false</c>.
        /// </returns>
        public bool Initialize(mmLicensedProductCode licensedProduct, params mmLicensedExtensionCode[] licensedExtension)
        {
            return _MinerRuntime.Initialize(new[] {licensedProduct}, licensedExtension);
        }

        /// <summary>
        ///     Attempts to checkout the license for the specific ArcFM <paramref name="mmLicensedProduct" /> and ESRI
        ///     <paramref name="esriLicensedProduct" />.
        /// </summary>
        /// <param name="esriLicensedProduct">The ArFM product code.</param>
        /// <param name="mmLicensedProduct">The mm licensed product.</param>
        /// <returns>
        ///     Returns a <see cref="Boolean" /> representing  <c>true</c> when the initialization is successful; otherwise
        ///     <c>false</c>.
        /// </returns>
        public bool Initialize(esriLicenseProductCode esriLicensedProduct, mmLicensedProductCode mmLicensedProduct)
        {
            return this.Initialize(esriLicensedProduct) && this.Initialize(mmLicensedProduct);
        }

        /// <summary>
        ///     Checks in all ArcGIS and ArcFM licenses that have been checked out.
        /// </summary>
        public void Shutdown()
        {
            this.Dispose(true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_MinerRuntime != null)
                    _MinerRuntime.Dispose();

                if (_EsriRuntime != null)
                    _EsriRuntime.Dispose();
            }
        }

        #endregion
    }
}