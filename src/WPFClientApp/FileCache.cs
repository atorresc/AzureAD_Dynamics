using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WPFClientApp
{
    public class FileCache : TokenCache
    {
        private string CacheFilePath;
        private static readonly object FileLock = new object();

        public FileCache(string filePath = @".\TokenCache.dat")
        {
            CacheFilePath = filePath;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;

            lock (FileLock)
            {
                this.Deserialize(File.Exists(CacheFilePath) ? ProtectedData.Unprotect(
                    File.ReadAllBytes(CacheFilePath), null, DataProtectionScope.CurrentUser) : null);
            }
        }
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                this.Deserialize(File.Exists(CacheFilePath) ? ProtectedData.Unprotect(
                    File.ReadAllBytes(CacheFilePath), null, DataProtectionScope.CurrentUser) : null);
            }
        }

        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (this.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changes in the persistent store
                    File.WriteAllBytes(CacheFilePath, ProtectedData.Protect(this.Serialize(), null, DataProtectionScope.CurrentUser));
                    // once the write operation took place, restore the HasStateChanged bit to false
                    this.HasStateChanged = false;
                }
            }
        }

        public override void Clear()
        {
            base.Clear();
            File.Delete(CacheFilePath);
        }

    }
}
