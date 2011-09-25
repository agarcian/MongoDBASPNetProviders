using System;
using AltovientoSolutions.DAL.IPC.Model;

namespace AltovientoSolutions.DAL.IPC
{
    public interface IIPCMediator
    {
        /// <summary>
        /// When implemented, it saves the illustrated parts catalog to the data repository.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="spaceId">The space id.</param>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="langCode">The lang code.</param>
        /// <remarks>
        /// The implementation must adhere to the following rules:
        /// <list type="table">
        /// <item>
        /// <term>Create new Catalog</term>
        /// <description>The catalog will be stored into the database it the catalog id does not exist.</description>
        /// </item>
        /// <item>
        /// <term>Update existing Catalog</term>
        /// <description>The catalog will be stored into the database to overwrite an existing catalog.  The overwrite flag must be set to true, otherwise if the catalog exist and overwrite is false, it will raise an <see cref="InvalidOperationException"/>.</description>
        /// </item>
        /// </list>
        /// <para>Notice that the images are not stored as part of this call. A separate and independent call must be
        /// performed to <see cref="SaveIllustration"/> to store all the illustrations.  
        /// The catalog object will make a reference internally to the Illustration unique identifier.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">When trying to save a catalog that already exists and the <see cref="overwrite"/> attribute is set to false.</exception>
        void SaveCatalog(Catalog catalog, string spaceId, string catalogId, string langCode, bool overwrite);
        /// <summary>
        /// Saves the illustration.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="spaceId">The space id.</param>
        /// <param name="md5">The MD5.</param>
        /// <param name="fileName">Name of the file.</param>
        void SaveIllustration(byte[] buffer, string spaceId, string md5, string fileName);
        bool DoesCatalogExist(string spaceId, string catalogId, string langCode);
        Catalog GetCatalog(string spaceId, string catalogId, string langCode);
    }
}
